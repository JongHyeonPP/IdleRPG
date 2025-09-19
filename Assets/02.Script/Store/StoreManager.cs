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

public class StoreManager : MonoSingleton<StoreManager>
{
    #region Fields ─ Data/Refs/State

    [Header("Data")]
    private GameData _gameData;

    // (가챠 종류, 횟수) → (재화, 수량)
    private readonly Dictionary<(GachaType gachaType, int num), (Resource resource, int num)> prices = new();

    [SerializeField] private WeaponData[] _weaponDatas;         // 무기 데이터 (UID 매핑용)
    [SerializeField] private List<WeaponData> _weaponSaveDatas; // 최근 뽑은 무기(옵션)
    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;       // 메인 스토어 UI
    [SerializeField] private UIDocument _storePopupDocument;    // 결과/에러 팝업 UI 루트
    [SerializeField] private VisualTreeAsset _storeSlotItem;    // (필요 시) 슬롯 템플릿
    [SerializeField] private Sprite _hamsterSprite;
    [SerializeField] private AudioClip _popupSound;
    [SerializeField] private AudioClip _drawSound;

    private VisualElement _root;

    // Buttons
    private Button _weapon1Btn;
    private Button _weapon10Btn;
    private Button _costume1Btn;
    private Button _costume10Btn;

    // Result Popup
    private VisualElement _popup;
    private VisualElement _rowVE1;
    private VisualElement _rowVE2;
    private Button _popupCloseBtn;

    // Error Popup
    private VisualElement _errorPopup;
    private Label _errorTxt;
    private Button _errorCloseBtn;

    // Hamster
    private VisualElement _hamsterUI;
    private Label _hamsterText;
    private VisualElement _hamsterImage;

    // FX
    private VisualElement _storeFX;

    // State
    private bool _isPopupVisible = false;
    private bool _isErrorPopupVisible = false;
    private AudioSource _audioSource;
    private bool _isProcessing;
    private Dictionary<string, WeaponData> _weaponByUid;        // UID → WeaponData
    private readonly List<VisualElement> _slots = new();        // 결과 슬롯들(Row1/2 하위 재사용)

    private readonly string[] _hamsterMessages = { "어서오세요!", "앗!", "좋은 걸 뽑아보자!", "가자~!" };

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        InitPriceFromRc();
        InitStore();
        BuildWeaponUidIndexIfNeeded();
    }

    #endregion

    #region RemoteConfig (가격 초기화)

    public void InitPriceFromRc()
    {
        try
        {
            var json = RemoteConfigService.Instance.appConfig.GetJson("GACHA_INFO");
            if (string.IsNullOrEmpty(json)) throw new Exception("GACHA_INFO가 비어 있습니다.");

            var root = JObject.Parse(json);
            var cost = root["cost"] as JObject ?? throw new Exception("GACHA_INFO.cost 노드를 찾을 수 없습니다.");

            void SetPrice(GachaType gType, int n, JToken node)
            {
                if (node == null) throw new Exception($"cost 노드가 없습니다: {gType} x{n}");
                string resourceStr = node["resource"]?.ToString();
                if (string.IsNullOrEmpty(resourceStr)) throw new Exception($"resource가 비어 있음: {gType} x{n}");
                if (!Enum.TryParse<Resource>(resourceStr, true, out var resEnum))
                    throw new Exception($"알 수 없는 재화 타입: {resourceStr} ({gType} x{n})");
                int amount = node["amount"]?.Value<int>() ?? 0;
                if (amount <= 0) throw new Exception($"amount가 유효하지 않음: {amount} ({gType} x{n})");
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
            // 가격 초기화 실패도 사용자에게 명확히 전달
            ShowErrorPopup("상점 가격 정보를 불러오지 못했습니다.\n잠시 후 다시 시도해 주세요.");
            throw;
        }
    }

    #endregion

    #region Init & UI Wiring

    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        // Audio
        _audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Root & Panels
        _root = _storeUIDocument.rootVisualElement;

        #region Weapon Panel (버튼/라벨)

        var itemSlot0 = _root?.Q<VisualElement>("ItemSlot0");
        var storePanel0 = itemSlot0?.Q<VisualElement>("StorePanel_0");
        var storePanel1 = itemSlot0?.Q<VisualElement>("StorePanel_1");

        _weapon1Btn = storePanel0?.Q<Button>("StoreBtn");
        _weapon10Btn = storePanel1?.Q<Button>("StoreBtn");

        var priceLabel0 = storePanel0?.Q<Label>("PriceLabel");
        var infoLabel0 = storePanel0?.Q<Label>("InfoLabel");
        var priceLabel1 = storePanel1?.Q<Label>("PriceLabel");
        var infoLabel1 = storePanel1?.Q<Label>("InfoLabel");

        if (priceLabel0 != null && prices.TryGetValue((GachaType.Weapon, 1), out var pW1)) priceLabel0.text = pW1.num.ToString();
        if (infoLabel0 != null) infoLabel0.text = "1회 뽑기";
        if (priceLabel1 != null && prices.TryGetValue((GachaType.Weapon, 10), out var pW10)) priceLabel1.text = pW10.num.ToString();
        if (infoLabel1 != null) infoLabel1.text = "10회 뽑기";

        #endregion

        #region Costume Panel (버튼/라벨)

        var itemSlot1 = _root?.Q<VisualElement>("ItemSlot1");
        var storePanel0_1 = itemSlot1?.Q<VisualElement>("StorePanel_0");
        var storePanel1_1 = itemSlot1?.Q<VisualElement>("StorePanel_1");

        _costume1Btn = storePanel0_1?.Q<Button>("StoreBtn");
        _costume10Btn = storePanel1_1?.Q<Button>("StoreBtn");

        var priceLabel0_1 = storePanel0_1?.Q<Label>("PriceLabel");
        var infoLabel0_1 = storePanel0_1?.Q<Label>("InfoLabel");
        var priceLabel1_1 = storePanel1_1?.Q<Label>("PriceLabel");
        var infoLabel1_1 = storePanel1_1?.Q<Label>("InfoLabel");

        if (priceLabel0_1 != null && prices.TryGetValue((GachaType.Costume, 1), out var pC1)) priceLabel0_1.text = pC1.num.ToString();
        if (infoLabel0_1 != null) infoLabel0_1.text = "1회 뽑기";
        if (priceLabel1_1 != null && prices.TryGetValue((GachaType.Costume, 10), out var pC10)) priceLabel1_1.text = pC10.num.ToString();
        if (infoLabel1_1 != null) infoLabel1_1.text = "10회 뽑기";

        #endregion

        #region Button Callbacks (서버 호출만)

        _weapon1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 1));
        _weapon10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 10));
        _costume1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 1));
        _costume10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 10));

        #endregion

        #region Popups (Result + Error)

        var popuproot = _storePopupDocument?.rootVisualElement;

        // Result Popup
        _popup = popuproot?.Q<VisualElement>("Popup");
        _popupCloseBtn = popuproot?.Q<Button>("PopupCloseBtn");
        _rowVE1 = popuproot?.Q<VisualElement>("RowVE1");
        _rowVE2 = popuproot?.Q<VisualElement>("RowVE2");
        if (_popup != null) _popup.style.display = DisplayStyle.None;
        _popupCloseBtn?.RegisterCallback<ClickEvent>(_ => SetPopupVisibility(false));
        _popup?.RegisterCallback<PointerDownEvent>(_ => ClosePopup());

        // Error Popup
        _errorPopup = popuproot?.Q<VisualElement>("ErrorPopup");
        _errorTxt = popuproot?.Q<Label>("ErrorTxt");
        _errorCloseBtn = popuproot?.Q<Button>("ErrorCloseBtn");
        if (_errorPopup != null) _errorPopup.style.display = DisplayStyle.None;
        _errorCloseBtn?.RegisterCallback<ClickEvent>(_ => SetErrorPopupVisibility(false));
        _errorPopup?.RegisterCallback<PointerDownEvent>(_ => CloseErrorPopup());

        #endregion

        #region Hamster

        _hamsterUI = _root?.Q<VisualElement>("HamsterUI");
        _hamsterText = _root?.Q<Label>("HamsterText");
        _hamsterImage = _root?.Q<VisualElement>("HamsterImage");
        SetHamsterText(_hamsterMessages[0]);
        SetPopupVisibility(false);
        SetErrorPopupVisibility(false);

        #endregion

        #region FX

        _storeFX = _root?.Q<VisualElement>("StoreFX");
        if (_storeFX != null) _storeFX.pickingMode = PickingMode.Ignore;

        #endregion

        #region Slots Pool

        BuildSlotPool();
        HideAllSlots();

        #endregion
    }

    private void BuildSlotPool()
    {
        if (_rowVE1 == null || _rowVE2 == null) return;
        if (_slots.Count > 0) return;

        foreach (var child in _rowVE1.Children()) _slots.Add(child);
        foreach (var child in _rowVE2.Children()) _slots.Add(child);

        Debug.Log($"[BuildSlotPool] 슬롯 수집 완료: {_slots.Count}개");
    }

    #endregion

    #region FX Helpers

    public void OpenStore() => PlayStoreFxAt(_storeFX);

    private void PlayStoreFxAt(VisualElement ve)
    {
        if (ve == null) return;
        ve.style.display = DisplayStyle.Flex;
        ParticleFxManager.Instance.Play("StoreOpen");
    }

    #endregion

    #region Hamster Helpers

    public void SetHamsterText(string text)
    {
        if (_hamsterText == null) return;
        _hamsterText.text = text;
        StartCoroutine(AnimateHamsterText(_hamsterText));
    }

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

    #endregion

    #region Result Popup Helpers

    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null || _isPopupVisible == isVisible) return;

        // 에러 팝업과 상호배제
        if (isVisible) SetErrorPopupVisibility(false);

        _isPopupVisible = isVisible;

        if (_popupSound != null && _audioSource != null)
            _audioSource.PlayOneShot(_popupSound);

        SetHamsterText(isVisible ? _hamsterMessages[Random.Range(1, _hamsterMessages.Length)] : _hamsterMessages[0]);
        _popup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ClosePopup() => SetPopupVisibility(false);

    #endregion

    #region Error Popup Helpers

    private void SetErrorPopupVisibility(bool isVisible)
    {
        if (_errorPopup == null || _isErrorPopupVisible == isVisible) return;

        // 결과 팝업과 상호배제
        if (isVisible) SetPopupVisibility(false);

        _isErrorPopupVisible = isVisible;

        if (_popupSound != null && _audioSource != null && isVisible)
            _audioSource.PlayOneShot(_popupSound);

        // 해칭터 텍스트는 에러일 때는 고정 멘트로 전환
        if (isVisible) SetHamsterText("문제가 발생했습니다.");
        else SetHamsterText(_hamsterMessages[0]);

        _errorPopup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ShowErrorPopup(string msg)
    {
        HideAllSlots();                  // 결과 슬롯 숨김
        if (_errorTxt != null) _errorTxt.text = string.IsNullOrEmpty(msg) ? "가챠에 실패했습니다." : msg;
        SetErrorPopupVisibility(true);
    }

    public void CloseErrorPopup() => SetErrorPopupVisibility(false);

    #endregion

    #region CloudCode (CallGacha)

    private async Task<GachaResult> CallGacha(GachaType type, int num)
    {
        try
        {
            var args = new Dictionary<string, object> {
                { "gachaType", type.ToString() },
                { "gachaNum",  num }
            };

            var result = await CloudCodeService.Instance
                .CallModuleEndpointAsync<GachaResult>("PurchaseProcessor", "ProcessGacha", args);

            if (!result.Success)
            {
                Debug.LogWarning($"[Gacha] 실패: {result.Message}");
                return result;
            }

            // 성공 시 로컬 반영
            _gameData.dia = result.RemainDia;
            PlayerBroker.OnDiaSet();

            if (type == GachaType.Weapon)
            {
                foreach (var id in result.Items)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (_gameData.weaponCount.ContainsKey(id)) _gameData.weaponCount[id]++;
                    else _gameData.weaponCount[id] = 1;
                    PlayerBroker.OnWeaponCountSet?.Invoke(id, _gameData.weaponCount[id]);
                }
            }
            else if (type == GachaType.Costume)
            {
                foreach (var id in result.Items)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (!_gameData.ownedCostumes.Contains(id)) _gameData.ownedCostumes.Add(id);
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Gacha] 예외: {e.Message}");
            return new GachaResult
            {
                Success = false,
                Message = "서버 통신에 실패했습니다.",
                Items = new List<string>(),
                RemainDia = _gameData.dia
            };
        }
    }

    #endregion

    #region Click Handler

    private void BuildWeaponUidIndexIfNeeded()
    {
        if (_weaponByUid != null) return;
        _weaponByUid = _weaponDatas?
            .Where(w => w != null && !string.IsNullOrEmpty(w.UID))
            .ToDictionary(w => w.UID, w => w) ?? new Dictionary<string, WeaponData>();
    }

    private async Task OnClickGacha(GachaType type, int num)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        SetButtonsEnabled(false);

        try
        {
            if (_drawSound != null && _audioSource != null)
                _audioSource.PlayOneShot(_drawSound);

            SetHamsterText("돌리는 중...");
            var result = await CallGacha(type, num);

            if (result == null || !result.Success)
            {
                ShowErrorPopup(result?.Message ?? "알 수 없는 오류가 발생했습니다.");
                return;
            }

            if (type == GachaType.Weapon)
            {
                BuildWeaponUidIndexIfNeeded();
                var list = new List<WeaponData>();
                foreach (var id in result.Items)
                {
                    if (!string.IsNullOrWhiteSpace(id) && _weaponByUid.TryGetValue(id, out var w))
                        list.Add(w);
                    else
                        Debug.LogWarning($"[Gacha] UID 매핑 실패: '{id}'");
                }

                _weaponSaveDatas = list;
                SetPopupVisibility(true);
                UpdateWeaponGridUI(list);
                UpdateLog(list);
            }
            else // Costume
            {
                var list = result.Items
                    .Select(id => CostumeManager.Instance.AllCostumeDatas.FirstOrDefault(c => c.Uid == id))
                    .Where(c => c != null).ToList();

                SetPopupVisibility(true);
                UpdateCostumeGridUI(list);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            ShowErrorPopup("알 수 없는 오류가 발생했습니다.");
        }
        finally
        {
            _isProcessing = false;
            SetButtonsEnabled(true);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("GachaTest/Weapon x1")] public async void GachaTest_Weapon_1() => await OnClickGacha(GachaType.Weapon, 1);
    [ContextMenu("GachaTest/Weapon x10")] public async void GachaTest_Weapon_10() => await OnClickGacha(GachaType.Weapon, 10);
    [ContextMenu("GachaTest/Costume x1")] public async void GachaTest_Costume_1() => await OnClickGacha(GachaType.Costume, 1);
    [ContextMenu("GachaTest/Costume x10")] public async void GachaTest_Costume_10() => await OnClickGacha(GachaType.Costume, 10);
#endif

    #endregion

    #region UI Update

    private void SetButtonsEnabled(bool on)
    {
        _weapon1Btn?.SetEnabled(on);
        _weapon10Btn?.SetEnabled(on);
        _costume1Btn?.SetEnabled(on);
        _costume10Btn?.SetEnabled(on);
    }

    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null || _slots.Count == 0) return;

        int n = Mathf.Min(weapons.Count, _slots.Count);
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (i < n)
            {
                var weapon = weapons[i];

                var icon = slot.Q<VisualElement>("WeaponIcon");
                if (icon != null && weapon.WeaponSprite != null)
                {
                    icon.style.width = 180;
                    icon.style.height = 180;
                    icon.style.backgroundImage = new StyleBackground(weapon.WeaponSprite.texture);
#pragma warning disable 618
                    icon.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
#pragma warning restore 618
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

    private void UpdateCostumeGridUI(List<CostumeItem> costumes)
    {
        HideAllSlots();
        if (costumes == null || _slots.Count == 0) return;

        int n = Mathf.Min(costumes.Count, _slots.Count);
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (i < n)
            {
                var costume = costumes[i];
                var icon = slot.Q<VisualElement>("WeaponIcon");
                if (icon != null && costume.IconTexture != null)
                {
                    icon.style.width = 180;
                    icon.style.height = 180;
                    icon.style.backgroundImage = new StyleBackground(costume.IconTexture);
#pragma warning disable 618
                    icon.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
#pragma warning restore 618
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

    private void HideAllSlots()
    {
        foreach (var s in _slots) s.style.display = DisplayStyle.None;
    }

    private void UpdateLog(List<WeaponData> weapons)
    {
        string log = "뽑기 결과:\n";
        foreach (var weapon in weapons) log += $"- {weapon.name} ({weapon.WeaponRarity})\n";
        Debug.Log(log);
    }

    #endregion

    #region Utils (텍스트 줄바꿈/애니/이징)

    private string WrapText(string text, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var words = text.Split(' ');
        var sb = new System.Text.StringBuilder();
        int current = 0;
        foreach (var w in words)
        {
            if (current + w.Length <= maxCharsPerLine)
            {
                if (current > 0) { sb.Append(" "); current++; }
                sb.Append(w); current += w.Length;
            }
            else
            {
                sb.Append("\n"); sb.Append(w); current = w.Length;
            }
        }
        return sb.ToString();
    }

    private IEnumerator AnimateSlot(VisualElement slot)
    {
        float duration = 0.5f, elapsed = 0f;
        Vector3 start = new(0.5f, 0.5f, 1f), end = new(1f, 1f, 1f);
        slot.style.scale = new StyleScale(start);
        slot.style.opacity = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = EaseInOutCubic(t);
            slot.style.scale = new StyleScale(Vector3.Lerp(start, end, e));
            slot.style.opacity = Mathf.Lerp(0f, 1f, e);
            yield return null;
        }
        slot.style.scale = new StyleScale(end);
        slot.style.opacity = 1f;
    }

    private float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

    #endregion
}
