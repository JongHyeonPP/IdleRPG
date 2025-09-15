using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Newtonsoft.Json.Linq;
using System.Linq;

/// <summary>
/// 인코딩 주의: 한글이 깨질 경우 이 파일을 UTF-8 (BOM 여부 무관)로 저장하세요.
/// VS Code: 하단 상태바의 인코딩 클릭 → "다른 인코딩으로 저장" → UTF-8
/// Rider/Visual Studio: File → Save As → Save with Encoding → UTF-8
/// </summary>
public class StoreManager : MonoSingleton<StoreManager>
{
    [Header("Data")]
    private GameData _gameData;

    // 키 = (가챠 종류, 뽑기 횟수), 값 = (소모 재화, 소모 수량)
    // 예) prices[(GachaType.Weapon, 1)] => (Resource.Dia, 10)
    Dictionary<(GachaType gachaType, int num), (Resource resource, int num)> prices = new();

    [SerializeField] private WeaponData[] _weaponDatas;             // 무기 데이터들
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // 최근 뽑은 무기 데이터들(임시 저장)

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // 외부에서 읽기 전용으로 접근

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // 메인 스토어 UI
    [SerializeField] private UIDocument _storePopupDocument;        // 결과 팝업 UI
    [SerializeField] private VisualTreeAsset _storeSlotItem;        // 결과 슬롯 템플릿(UXML)
    [SerializeField] private Sprite _hamsterSprite;                 // 햄스터 스프라이트(선택)
    [SerializeField] private AudioClip _popupSound;                 // 팝업 사운드
    [SerializeField] private AudioClip _drawSound;                  // 뽑기 사운드

    private VisualElement _root;

    // Weapon UI
    private VisualElement _panel;                                   // 스토어 패널
    private VisualElement _weaponGrid;                              // (미사용) 무기 그리드
    private Button _weapon1Btn;                                     // 무기 1회 뽑기 버튼
    private Button _weapon10Btn;                                    // 무기 10회 뽑기 버튼
    private Button _costume1Btn;                                    // 코스튬 1회 뽑기 버튼
    private Button _costume10Btn;                                   // 코스튬 10회 뽑기 버튼

    // Popup UI
    private VisualElement _popup;                                   // 결과 팝업 루트
    private VisualElement _rowVE1;                                  // 결과 1행 컨테이너
    private VisualElement _rowVE2;                                  // 결과 2행 컨테이너
    private Button _popupCloseBtn;                                  // 팝업 닫기 버튼
    private Button _openPopupBtn;                                   // (선택) 팝업 열기 버튼

    // Hamster UI
    private VisualElement _hamsterUI;                               // 햄스터 UI 루트
    private Label _hamsterText;                                     // 햄스터 대사 텍스트
    private VisualElement _hamsterImage;                            // 햄스터 이미지

    // 파티클
    private VisualElement _storeFX;                                 // 파티클 표시용 VE(클릭 무시)

    // System
    private GachaSystem<WeaponData> _gachaSystem;                   // 무기 가챠 시스템
    private GachaSystem<CostumeItem> _costumeGachaSystem;           // 코스튬 가챠 시스템

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;
    private const int MAX_SLOTS = STORE_ROW * STORE_COLUMN;
    private AudioSource _audioSource;

    // 슬롯 풀(행/열 초기 생성 후 On/Off로 토글)
    private readonly List<VisualElement> _slots = new();

    // 햄스터 대사 메시지
    private readonly string[] _hamsterMessages = new string[] {
        "어서오세요!",
        "앗!",
        "좋은 걸 뽑아보자!",
        "가자~!",
    };

    //Data
    private Dictionary<string, int> _weaponCount;

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        InitPriceFromRc();
        InitStore();
    }

    public void InitPriceFromRc()
    {
        try
        {
            // RC에서 GACHA_INFO 전체 JSON 문자열 가져오기
            var json = RemoteConfigService.Instance.appConfig.GetJson("GACHA_INFO");
            if (string.IsNullOrEmpty(json))
                throw new Exception("GACHA_INFO가 비어 있습니다.");

            var root = JObject.Parse(json);
            var cost = root["cost"] as JObject;
            if (cost == null)
                throw new Exception("GACHA_INFO.cost 노드를 찾을 수 없습니다.");

            void SetPrice(GachaType gType, int n, JToken node)
            {
                if (node == null) throw new Exception($"cost 노드가 없습니다: {gType} x{n}");

                string resourceStr = node["resource"]?.ToString();
                if (string.IsNullOrEmpty(resourceStr))
                    throw new Exception($"resource가 비어 있습니다: {gType} x{n}");

                if (!Enum.TryParse<Resource>(resourceStr, true, out var resEnum))
                    throw new Exception($"알 수 없는 재화 타입입니다: {resourceStr} ({gType} x{n})");

                int amount = node["amount"]?.Value<int>() ?? 0;
                if (amount <= 0)
                    throw new Exception($"amount가 유효하지 않습니다: {amount} ({gType} x{n})");

                prices[(gType, n)] = (resEnum, amount);
            }

            var weapon = cost["weapon"];
            var costume = cost["costume"];

            SetPrice(GachaType.Weapon, 1, weapon?["single"]);
            SetPrice(GachaType.Weapon, 10, weapon?["multi10"]);
            SetPrice(GachaType.Costume, 1, costume?["single"]);
            SetPrice(GachaType.Costume, 10, costume?["multi10"]);
        }
        catch (Exception e)
        {
            Debug.LogError($"InitPriceFromRc 오류: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        // 오디오 소스 준비
        _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        // 가챠 시스템 구성
        _gachaSystem = new GachaSystem<WeaponData>(_weaponDatas);

        var root = _storeUIDocument.rootVisualElement;
        _root = root; // 캐시
        _panel = root?.Q<VisualElement>("Panel");
        _weaponGrid = root?.Q<VisualElement>("WeaponGrid");

        #region 무기 패널
        var itemSlot0 = _root?.Q<VisualElement>("ItemSlot0");
        var storePanel0 = itemSlot0?.Q<VisualElement>("StorePanel_0");
        var storePanel1 = itemSlot0?.Q<VisualElement>("StorePanel_1");

        _weapon1Btn = storePanel0?.Q<Button>("StoreBtn");
        _weapon10Btn = storePanel1?.Q<Button>("StoreBtn");

        var priceLabel0 = storePanel0?.Q<Label>("PriceLabel");
        var infoLabel0 = storePanel0?.Q<Label>("InfoLabel");
        var priceLabel1 = storePanel1?.Q<Label>("PriceLabel");
        var infoLabel1 = storePanel1?.Q<Label>("InfoLabel");

        if (priceLabel0 != null) priceLabel0.text = prices[(GachaType.Weapon, 1)].num.ToString();
        if (infoLabel0 != null) infoLabel0.text = "1회 뽑기";
        if (priceLabel1 != null) priceLabel1.text = prices[(GachaType.Weapon, 10)].num.ToString();
        if (infoLabel1 != null) infoLabel1.text = "10회 뽑기";
        #endregion

        #region 코스튬 패널
        var itemSlot1 = _root?.Q<VisualElement>("ItemSlot1");
        var storePanel0_1 = itemSlot1?.Q<VisualElement>("StorePanel_0");
        var storePanel1_1 = itemSlot1?.Q<VisualElement>("StorePanel_1");

        // ⚠️ BUGFIX: 잘못된 참조 수정 (storePanel0 → storePanel0_1 등)
        _costume1Btn = storePanel0_1?.Q<Button>("StoreBtn");
        _costume10Btn = storePanel1_1?.Q<Button>("StoreBtn");

        var priceLabel0_1 = storePanel0_1?.Q<Label>("PriceLabel");
        var infoLabel0_1 = storePanel0_1?.Q<Label>("InfoLabel");
        var priceLabel1_1 = storePanel1_1?.Q<Label>("PriceLabel");
        var infoLabel1_1 = storePanel1_1?.Q<Label>("InfoLabel");

        if (priceLabel0_1 != null) priceLabel0_1.text = prices[(GachaType.Costume, 1)].num.ToString();
        if (infoLabel0_1 != null) infoLabel0_1.text = "1회 뽑기";
        if (priceLabel1_1 != null) priceLabel1_1.text = prices[(GachaType.Costume, 10)].num.ToString();
        if (infoLabel1_1 != null) infoLabel1_1.text = "10회 뽑기";
        #endregion

        var popuproot = _storePopupDocument.rootVisualElement;

        // Popup 초기화
        _popup = popuproot?.Q<VisualElement>("Popup");
        _popupCloseBtn = popuproot?.Q<Button>("PopupCloseBtn");
        _popup.style.display = DisplayStyle.None;
        _rowVE1 = popuproot?.Q<VisualElement>("RowVE1");
        _rowVE2 = popuproot?.Q<VisualElement>("RowVE2");

        // 햄스터 UI 초기화
        _hamsterUI = root?.Q<VisualElement>("HamsterUI");
        _hamsterText = root?.Q<Label>("HamsterText");
        _hamsterImage = root?.Q<VisualElement>("HamsterImage");

        SetHamsterText(_hamsterMessages[0]);
        SetPopupVisibility(false);

        _weapon1Btn?.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn?.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        _costume1Btn?.RegisterCallback<ClickEvent>(evt => DrawCostumeItems(1));
        _costume10Btn?.RegisterCallback<ClickEvent>(evt => DrawCostumeItems(10));
        _popupCloseBtn?.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));

        // 팝업 배경 클릭 시 닫기
        _popup?.RegisterCallback<PointerDownEvent>(evt => ClosePopup());

        _storeFX = root.Q<VisualElement>("StoreFX");
        if (_storeFX != null)
        {
            _storeFX.pickingMode = PickingMode.Ignore; // 클릭/포인터 이벤트 무시
        }

        // ──────────────────────────────
        // 슬롯 풀 초기 생성(행/열 레이아웃 유지)
        // ──────────────────────────────
        BuildSlotPool();
        HideAllSlots();
    }

    /// <summary>
    /// 결과 슬롯 풀을 미리 생성해두고, 내용만 교체 + On/Off로 토글하는 방식
    /// </summary>
    private void BuildSlotPool()
    {
        if (_storeSlotItem == null || _rowVE1 == null || _rowVE2 == null) return;
        if (_slots.Count > 0) return; // 이미 생성됨

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            var slot = _storeSlotItem.CloneTree();
            if (i < STORE_COLUMN) _rowVE1.Add(slot); else _rowVE2.Add(slot);
            _slots.Add(slot);
        }
    }

    public void OpenStore()
    {
        // 필요 시 패널 표시
        //_panel?.style.display = DisplayStyle.Flex;

        // 파티클 실행
        PlayStoreFxAt(_storeFX);
    }

    private void PlayStoreFxAt(VisualElement ve)
    {
        if (ve == null) return;
        ve.style.display = DisplayStyle.Flex;
        ParticleFxManager.Instance.Play("StoreOpen");
    }

    /// <summary>
    /// 햄스터 대사 설정
    /// </summary>
    public void SetHamsterText(string text)
    {
        if (_hamsterText == null) return;
        _hamsterText.text = text;
        StartCoroutine(AnimateHamsterText(_hamsterText));
    }

    /// <summary>
    /// 햄스터 텍스트 페이드/슬라이드 애니메이션
    /// </summary>
    private IEnumerator AnimateHamsterText(Label textLabel)
    {
        textLabel.style.opacity = 0;
        textLabel.style.translate = new StyleTranslate(new Translate(0, 10f, 0));

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutQuad(t);

            textLabel.style.opacity = easedT;
            textLabel.style.translate = new StyleTranslate(new Translate(0, 10f * (1 - easedT), 0));

            yield return null;
        }

        textLabel.style.opacity = 1;
        textLabel.style.translate = new StyleTranslate(new Translate(0, 0, 0));
    }

    /// <summary>
    /// 팝업 표시/숨김
    /// </summary>
    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null || _isPopupVisible == isVisible) return;

        _isPopupVisible = isVisible;

        if (_popupSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_popupSound);
        }

        if (isVisible)
            SetHamsterText(_hamsterMessages[Random.Range(1, _hamsterMessages.Length)]);
        else
            SetHamsterText(_hamsterMessages[0]);

        _popup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// 배경 클릭으로 팝업 닫기
    /// </summary>
    public void ClosePopup()
    {
        SetPopupVisibility(false);
    }

    /// <summary>
    /// 무기 여러 개 뽑기 + UI 갱신
    /// </summary>
    public void DrawMultipleWeapons(int count)
    {
        if (_drawSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_drawSound);
        }

        HideAllSlots();

        List<WeaponData> drawnWeapons = _gachaSystem.DrawItems(count);
        _weaponSaveDatas = drawnWeapons; // 임시 저장

        if (drawnWeapons.Count > 0)
        {
            bool hasRare = drawnWeapons.Exists(weapon => weapon.WeaponRarity >= Rarity.Rare);
            if (hasRare) SetHamsterText("좋아! 레어가 나왔어!");
            else SetHamsterText("운이 조금 아쉽네. 한 번 더? ");
        }

        SetPopupVisibility(true);
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }

    /// <summary>
    /// 무기 결과 슬롯 채우기(풀 재사용, On/Off)
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null || _slots.Count == 0) return;

        int n = Mathf.Min(weapons.Count, MAX_SLOTS);
        
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            Debug.Log("왜안됨");
            var slot = _slots[i];
            if (i < n)
            {
                var weapon = weapons[i];

                var icon = slot.Q<VisualElement>("WeaponIcon");
                if (icon != null && weapon.WeaponSprite != null)
                {
                    // 중요: Texture 아닌 Sprite로 배경 지정 (아틀라스 전체가 아니라 Sprite 영역만)
                    SetIcon(icon, new StyleBackground(weapon.WeaponSprite), 180f);
                }

                var nameLabel = slot.Q<Label>("WeaponName");
                if (nameLabel != null)
                {
                    nameLabel.text = WrapText(weapon.WeaponName, 7);
                    nameLabel.style.height = 30;
                }

                slot.style.display = DisplayStyle.Flex;
            }
            else
            {
                slot.style.display = DisplayStyle.None;
            }
        }
    }

    /// <summary>
    /// 코스튬 여러 개 뽑기 + UI 갱신
    /// </summary>
    private void DrawCostumeItems(int count)
    {
        if (_drawSound != null && _audioSource != null)
            _audioSource.PlayOneShot(_drawSound);

        // 중복 소유 방지 데이터로 가챠 풀 구성
        _costumeGachaSystem = new GachaSystem<CostumeItem>(
            CostumeManager.Instance.AllCostumeDatas
                .Where(c => !CostumeManager.Instance.IsOwned(c.Uid))
                .ToArray()
        );

        var drawnCostumes = _costumeGachaSystem.DrawItems(count);
        foreach (var costume in drawnCostumes)
        {
            CostumeManager.Instance.OwnedCostumes.Add(costume.Uid); // 획득 처리
        }

        CostumeManager.Instance.UpdateCostumeData();

        SetHamsterText("코스튬을 획득했어!");

        SetPopupVisibility(true);
        UpdateCostumeGridUI(drawnCostumes);
    }

    /// <summary>
    /// 코스튬 결과 슬롯 채우기(풀 재사용, On/Off)
    /// </summary>
    private void UpdateCostumeGridUI(List<CostumeItem> costumes)
    {
        HideAllSlots();

        int n = Mathf.Min(costumes.Count, MAX_SLOTS);
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            var slot = _slots[i];
            if (i < n)
            {
                var costume = costumes[i];
                var icon = slot.Q<VisualElement>("WeaponIcon"); // 동일 템플릿을 사용하므로 이름 유지
                if (icon != null)
                {
                    // 코스튬은 Texture만 있다면 Texture로 세팅(스케일 모드 유지)
                    SetIcon(icon, new StyleBackground(costume.IconTexture), 180f);
                }

                var nameLabel = slot.Q<Label>("WeaponName");
                if (nameLabel != null)
                {
                    nameLabel.text = WrapText(costume.Name, 7);
                    nameLabel.style.height = 30;
                }

                slot.style.display = DisplayStyle.Flex;
            }
            else
            {
                slot.style.display = DisplayStyle.None;
            }
        }
    }

    /// <summary>
    /// 슬롯 모두 숨김(클리어 대신 On/Off 방식)
    /// </summary>
    private void HideAllSlots()
    {
        foreach (var s in _slots)
            s.style.display = DisplayStyle.None;
    }

    // 로그 출력
    private void UpdateLog(List<WeaponData> weapons)
    {
        string log = "뽑기 결과:\n";
        foreach (var weapon in weapons)
        {
            log += $"- {weapon.name} ({weapon.WeaponRarity})\n";
        }
        Debug.Log(log);
    }

    string WrapText(string text, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var words = text.Split(' ');
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int currentLineLength = 0;

        foreach (var word in words)
        {
            if (currentLineLength + word.Length <= maxCharsPerLine)
            {
                if (currentLineLength > 0)
                {
                    sb.Append(" ");
                    currentLineLength++;
                }
                sb.Append(word);
                currentLineLength += word.Length;
            }
            else
            {
                sb.Append("\n");
                sb.Append(word);
                currentLineLength = word.Length;
            }
        }

        return sb.ToString();
    }

    // 슬롯 등장 애니메이션(선택)
    private IEnumerator AnimateSlot(VisualElement slot)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 endScale = new Vector3(1f, 1f, 1f);

        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);

            yield return null;
        }

        slot.style.scale = new StyleScale(endScale);
        slot.style.opacity = 1f;
    }

    private IEnumerator AniSlotDelay(VisualElement slot, float delay)
    {
        yield return new WaitForSeconds(delay);

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 endScale = Vector3.one;

        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);

            yield return null;
        }

        slot.style.scale = new StyleScale(Vector3.one);
        slot.style.opacity = 1f;
    }

    private IEnumerator AniPopup(bool isVisible)
    {
        float duration = 1f;
        float elapsed = 0f;

        if (_popup == null) yield break;

        if (isVisible)
            _popup.style.display = DisplayStyle.Flex;

        float startOpacity = isVisible ? 0f : 0.6f;
        float endOpacity = isVisible ? 0.6f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutQuad(t);

            _popup.style.opacity = Mathf.Lerp(startOpacity, endOpacity, easedT);

            yield return null;
        }

        _popup.style.opacity = endOpacity;

        if (!isVisible)
            _popup.style.display = DisplayStyle.None;
    }

    // Easing helpers
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
    private float EaseInCubic(float t) => t * t * t;
    private float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
    private float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
    private float EaseInQuad(float t) => t * t;
    private float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

    // 아이콘 공통 스타일 적용(USS 제약을 무력화하기 위해 min/max/flex도 고정)
    private void SetIcon(VisualElement icon, StyleBackground bg, float size)
    {
        icon.style.backgroundImage = bg;


        icon.style.width = size;
        icon.style.height = size;
        icon.style.minWidth = size;
        icon.style.minHeight = size;
        icon.style.maxWidth = size;
        icon.style.maxHeight = size;

        icon.style.flexGrow = 0;
        icon.style.flexShrink = 0;
        icon.style.flexBasis = size;
        icon.style.alignSelf = Align.Center;
    }

    // 서버 가챠 호출(샘플)
    private async Task CallGacha(GachaType type, int num)
    {
        Dictionary<string, object> args = new()
        {
            { "gachaType", type.ToString() },
            { "gachaNum",  num }
        };

        GachaResult result = await CloudCodeService.Instance
            .CallModuleEndpointAsync<GachaResult>(
                "PurchaseProcessor",
                "ProcessGacha",
                args);

        if (!result.Success)
        {
            Debug.LogWarning($"[Gacha] 실패: {result.Message}");
            return;
        }

        Debug.Log($"[Gacha] {type} x{num} => {string.Join(", ", result.Items)}");

        _gameData.dia = result.RemainDia;
        PlayerBroker.OnDiaSet();

        switch (type)
        {
            case GachaType.Weapon:
                foreach (var weaponId in result.Items)
                {
                    if (string.IsNullOrWhiteSpace(weaponId))
                        continue;

                    if (_gameData.weaponCount.ContainsKey(weaponId))
                        _gameData.weaponCount[weaponId]++;
                    else
                        _gameData.weaponCount[weaponId] = 1;

                    PlayerBroker.OnWeaponCountSet?.Invoke(weaponId, _gameData.weaponCount[weaponId]);
                }
                break;

            case GachaType.Costume:
                foreach (var costumeId in result.Items)
                {
                    if (string.IsNullOrWhiteSpace(costumeId))
                        continue;

                    if (!_gameData.ownedCostumes.Contains(costumeId))
                        _gameData.ownedCostumes.Add(costumeId);
                }
                break;
        }
    }

#if UNITY_EDITOR
    // ---- 무기 테스트 ----
    [ContextMenu("GachaTest/Weapon x1")]
    public async void GachaTest_Weapon_1() => await CallGacha(GachaType.Weapon, 1);

    [ContextMenu("GachaTest/Weapon x10")]
    public async void GachaTest_Weapon_10() => await CallGacha(GachaType.Weapon, 10);

    // ---- 코스튬 테스트 ----
    [ContextMenu("GachaTest/Costume x1")]
    public async void GachaTest_Costume_1() => await CallGacha(GachaType.Costume, 1);

    [ContextMenu("GachaTest/Costume x10")]
    public async void GachaTest_Costume_10() => await CallGacha(GachaType.Costume, 10);
#endif
}
