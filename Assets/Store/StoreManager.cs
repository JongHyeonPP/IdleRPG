using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;
using EnumCollection;


public class StoreManager : MonoSingleton<StoreManager>
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // 무기 데이터들
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // 뽑힌 무기 데이터들

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // 이거 가져다 써 일단 정욱핑

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI 문서
    [SerializeField] private VisualTreeAsset _storeSlotItem;        // 슬롯아이템
    [SerializeField] private Sprite _hamsterSprite;                 // 햄스터 스프라이트
    [SerializeField] private AudioClip _popupSound;                 // 팝업 효과음
    [SerializeField] private AudioClip _drawSound;                  // 뽑기 효과음

    private VisualElement _root;

    // Weapon UI
    private VisualElement _panel;                                   // 상점 패널
    private VisualElement _weaponGrid;                              // 무기 그리드
    private Button _weapon1Btn;                                     // 무기 1회 뽑기 버튼
    private Button _weapon10Btn;                                    // 무기 10회 뽑기 버튼

    // Popup UI
    private VisualElement _popup;                                   // Popup VisualElement
    private VisualElement _rowVE1;                                  // Popup 줄 첫번째
    private VisualElement _rowVE2;                                  // Popup 줄 두번째
    private Button _popupCloseBtn;                                  // Popup 닫기 버튼
    private Button _openPopupBtn;                                   // Popup 열기 버튼

    // Hamster UI
    private VisualElement _hamsterUI;                               // 햄스터 UI 요소
    private Label _hamsterText;                                     // 햄스터 대화 텍스트
    private VisualElement _hamsterImage;                            // 햄스터 이미지

    // System
    // 제네릭 Gacha 시스템 (WeaponData가 IGachaItem을 구현한다고 가정)
    private GachaSystem<WeaponData> _gachaSystem;
    // 예시로 코스튬 관련 가차 시스템 (코스튬 데이터가 IGachaItem을 구현해야 함)
    private GachaSystem<CostumeItem> __costumeGachaSystem;

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;
    private AudioSource _audioSource;

    // 햄스터 대화 메시지
    private readonly string[] _hamsterMessages = new string[] {
        "어서오세요!",
        "와!",
        "행운을 빌어요!",
        "헤헤~!",
    };

    //Data
    private Dictionary<string, int> _weaponCount;

    private void Start() => InitStore();

    private void OnEnable() => InitStore(); // 테스트용

    /// <summary>
    /// 초반 세팅
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        // 오디오 소스 추가
        _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        // 제네릭 GachaSystem으로 변경 (타입 파라미터 추가)
        _gachaSystem = new GachaSystem<WeaponData>(_weaponDatas);
        // 만약 CostumeManager.instance.AllCostumeDatas가 CostumeData[] 타입이라면...
        __costumeGachaSystem = new GachaSystem<CostumeItem>(CostumeManager.Instance.AllCostumeDatas);


        var root = _storeUIDocument.rootVisualElement;
        _root = root; // 저장핑
        _panel = root?.Q<VisualElement>("Panel");
        _weaponGrid = root?.Q<VisualElement>("WeaponGrid");
        _rowVE1 = root?.Q<VisualElement>("RowVE1");
        _rowVE2 = root?.Q<VisualElement>("RowVE2");
        _weapon1Btn = root?.Q<Button>("Weapon1Btn");
        _weapon10Btn = root?.Q<Button>("Weapon10Btn");

        // Popup 초기화
        _popup = root?.Q<VisualElement>("Popup");
        _popupCloseBtn = root?.Q<Button>("PopupCloseBtn");
        _openPopupBtn = root?.Q<Button>("OpenPopupBtn");

        // 햄스터 UI 초기화
        _hamsterUI = root?.Q<VisualElement>("HamsterUI");
        _hamsterText = root?.Q<Label>("HamsterText");
        _hamsterImage = root?.Q<VisualElement>("HamsterImage");

        // 햄스터 이미지 설정
        if (_hamsterImage != null && _hamsterSprite != null)
        {
            _hamsterImage.style.backgroundImage = new StyleBackground(_hamsterSprite);
        }

        // 햄스터 초기 텍스트 설정
        SetHamsterText(_hamsterMessages[0]);

        // Popup 비활성화
        SetPopupVisibility(false);

        _panel.style.display = DisplayStyle.Flex;

        _weapon1Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        _popupCloseBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));
        _openPopupBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(true));

        // 루트 요소에 클릭 이벤트 추가
        _popup.RegisterCallback<PointerDownEvent>(evt => ClosePopup());

    }

    /// <summary>
    /// 햄스터 텍스트 설정 함수
    /// </summary>
    /// <param name="text">표시할 텍스트</param>
    public void SetHamsterText(string text)
    {
        if (_hamsterText == null) return;

        _hamsterText.text = text;

        // 햄스터 텍스트 표시 애니메이션
        StartCoroutine(AnimateHamsterText(_hamsterText));
    }

    /// <summary>
    /// 햄스터 텍스트 애니메이션
    /// </summary>
    private IEnumerator AnimateHamsterText(Label textLabel)
    {
        // 시작 상태
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

        // 최종 상태 확실히 설정
        textLabel.style.opacity = 1;
        textLabel.style.translate = new StyleTranslate(new Translate(0, 0, 0));
    }

   

    /// <summary>
    /// Popup 활성화 설정
    /// </summary>
    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null || _isPopupVisible == isVisible) return;

        _isPopupVisible = isVisible;

        // 팝업 효과음 재생
        if (_popupSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_popupSound);
        }

        // 팝업이 보이면 햄스터 메시지 변경
        if (isVisible)
        {
            SetHamsterText(_hamsterMessages[Random.Range(1, _hamsterMessages.Length)]);
        }
        else
        {
            SetHamsterText(_hamsterMessages[0]);
        }

        StartCoroutine(AniPopup(isVisible));  // 애니메이션 시작
    }

    /// <summary>
    /// 클릭이 팝업 외부에서 발생했을 경우 팝업을 닫는 함수
    /// </summary>
    public void ClosePopup()
    {
        SetPopupVisibility(false);
    }

    /// <summary>
    /// 무기들 뽑아서 UI 그리기
    /// </summary>
    private void DrawMultipleWeapons(int count)
    {
        // 뽑기 효과음 재생
        if (_drawSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_drawSound);
        }

        // 초기화
        ClearGrid();

        // 뽑기
        List<WeaponData> drawnWeapons = _gachaSystem.DrawItems(count);

        // 저장된 무기데이터들
        // 이거 가져다가 추가해서 쓰면 되는디

        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
        //PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;

        _weaponSaveDatas = drawnWeapons;

        // 햄스터 텍스트 업데이트 - 뽑기 후 반응
        if (drawnWeapons.Count > 0)
        {
            bool hasRare = drawnWeapons.Exists(weapon => weapon.WeaponRarity >= Rarity.Rare);
            if (hasRare)
            {
                SetHamsterText("와아! ");

            }
            else
            {
                SetHamsterText("무기가 나왔어요! 다음에는 더 좋은 무기가 나올거에요!");
            }
        }

        //그리기
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }

   

    //private void OnWeaponCountSet(object weaponDataObj, int count)
    //{
    //    WeaponData weaponData = (WeaponData)weaponDataObj;
    //    VisualElement slot = _slotDict[weaponData.UID];
    //    ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");
    //    int level = _weaponLevel[weaponData.UID];
    //    int price = CurrencyManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
    //    countProgressBar.title = $"{count}/{price}";
    //    countProgressBar.value = count / (float)price;
    //}

    /// <summary>
    /// 무기들 UI 그리기
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;

        SetPopupVisibility(true);

        int cnt = 0;
        float delayInterval = 0.2f;  // 슬롯 간 텀 (초) - 더 빠르게 조정

        foreach (var weapon in weapons)
        {
            // _storeSlotItem을 클론
            var slot = _storeSlotItem.CloneTree();

            // 아이콘과 이름을 업데이트
            var icon = slot.Q<VisualElement>("WeaponIcon");
            var weaponImageTexture = weapon.WeaponSprite.texture;
            var weaponImageStyle = new StyleBackground(weaponImageTexture);
            icon.style.backgroundImage = weaponImageStyle;

            var nameLabel = slot.Q<Label>("WeaponName");
            if (nameLabel != null)
            {
                nameLabel.text = $"{weapon.WeaponName}";
            }

            // 무기 희귀도에 따라 슬롯 스타일 변경
            SetSlotRarityStyle(slot, weapon.WeaponRarity);

            // 그리드에 추가
            if (cnt >= STORE_COLUMN)
                _rowVE2.Add(slot);
            else
                _rowVE1.Add(slot);

            // 슬롯 애니메이션 시작 (슬롯마다 지연 시간 증가)
            StartCoroutine(AniSlotDelay(slot, cnt * delayInterval));

            cnt++;
        }
    }

    /// <summary>
    /// 희귀도에 따라 슬롯 스타일 설정
    /// </summary>
    private void SetSlotRarityStyle(VisualElement slot, Rarity rarity)
    {
        if (slot == null) return;

        var background = slot.Q<VisualElement>("Background");
        if (background == null) return;

        // 희귀도에 따른 색상 설정
        Color rarityColor;

        switch (rarity)
        {
            case Rarity.Common:
                rarityColor = new Color(0.7f, 0.7f, 0.7f); // 회색
                break;
            case Rarity.Uncommon:
                rarityColor = new Color(0.2f, 0.8f, 0.2f); // 초록색
                break;
            case Rarity.Rare:
                rarityColor = new Color(0.2f, 0.4f, 1f); // 파란색
                break;
            case Rarity.Unique:
                rarityColor = new Color(0.8f, 0.2f, 0.8f); // 보라색
                break;
            case Rarity.Legendary:
                rarityColor = new Color(1f, 0.8f, 0f); // 금색
                break;
            default:
                rarityColor = Color.white;
                break;
        }

        background.style.backgroundColor = rarityColor;

        // 전설 등급일 경우 빛나는 효과 추가
        if (rarity == Rarity.Legendary)
        {
            StartCoroutine(GlowingEffect(background));
        }
    }

    /// <summary>
    /// 전설 등급 아이템용 빛나는 효과
    /// </summary>
    private IEnumerator GlowingEffect(VisualElement element)
    {
        if (element == null) yield break;

        float minOpacity = 0.7f;
        float maxOpacity = 1f;
        float glowDuration = 1.5f;

        while (true)
        {
            // 밝아지기
            float elapsedTime = 0f;
            while (elapsedTime < glowDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / (glowDuration / 2));
                float easedT = EaseInOutSine(t);

                element.style.opacity = Mathf.Lerp(minOpacity, maxOpacity, easedT);

                yield return null;
            }

            // 어두워지기
            elapsedTime = 0f;
            while (elapsedTime < glowDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / (glowDuration / 2));
                float easedT = EaseInOutSine(t);

                element.style.opacity = Mathf.Lerp(maxOpacity, minOpacity, easedT);

                yield return null;
            }
        }
    }

    // 그리드 초기화
    private void ClearGrid()
    {
        _rowVE1?.Clear();
        _rowVE2?.Clear();
    }

    // 로그뽑기
    private void UpdateLog(List<WeaponData> weapons)
    {
        string log = "뽑기 결과:";
        foreach (var weapon in weapons)
        {
            log += $"- {weapon.name} ({weapon.WeaponRarity})\n";
        }
        Debug.Log(log);
    }

    /// <summary>
    /// 슬롯 애니메이션
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private IEnumerator AnimateSlot(VisualElement slot)
    {
        float duration = 0.5f;  // 애니메이션 시간
        float elapsed = 0f;

        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);  // 작게 시작
        Vector3 endScale = new Vector3(1f, 1f, 1f);         // 원래 크기

        // 초기 상태 설정
        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;

        // 애니메이션 루프
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            // 스케일 및 투명도 조정
            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);

            yield return null;
        }

        // 애니메이션 완료 후 최종 상태 설정
        slot.style.scale = new StyleScale(endScale);
        slot.style.opacity = 1f;
    }

    /// <summary>
    /// 딜레이 준 애니메이션
    /// </summary>
    private IEnumerator AniSlotDelay(VisualElement slot, float delay)
    {
        // 텀을 먼저 대기
        yield return new WaitForSeconds(delay);

        // 애니메이션 시작
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 endScale = new Vector3(1.1f, 1.1f, 1f);  // 약간 더 크게 확대
        Vector3 finalScale = new Vector3(1f, 1f, 1f);    // 최종 크기

        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;
        slot.style.rotate = new StyleRotate(new Rotate(-5f)); // 살짝 틀어진 상태로 시작

        // 첫 번째 단계: 확대 + 회전 복구
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOutBack(t); // 튕기는 효과

            // 스케일 및 투명도 조정
            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);
            slot.style.rotate = new StyleRotate(new Rotate(Mathf.Lerp(-5f, 0f, easedT)));

            yield return null;
        }

        // 두 번째 단계: 정상 크기로 돌아가기
        elapsed = 0f;
        float secondDuration = 0.2f;

        while (elapsed < secondDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / secondDuration);
            float easedT = EaseInOutCubic(t);

            // 스케일 조정
            slot.style.scale = new StyleScale(Vector3.Lerp(endScale, finalScale, easedT));

            yield return null;
        }

        // 최종 상태 설정
        slot.style.scale = new StyleScale(finalScale);
        slot.style.opacity = 1f;
        slot.style.rotate = new StyleRotate(new Rotate(0f));
    }


    /// <summary>
    /// 팝업에 애니메이션 효과 적용
    /// </summary>
    private IEnumerator AniPopup(bool isVisible)
    {
        float duration = 0.4f;  // 애니메이션 시간 (초)
        float elapsed = 0f;

        // 초기값과 목표값 설정
        Vector3 startScale = isVisible ? new Vector3(0.7f, 0.7f, 1f) : new Vector3(1f, 1f, 1f);
        Vector3 endScale = isVisible ? new Vector3(1f, 1f, 1f) : new Vector3(0.7f, 0.7f, 1f);
        float startY = isVisible ? 30f : 0f;
        float endY = isVisible ? 0f : 30f;

        // 팝업 표시 스타일 변경
        if (isVisible) _popup.style.display = DisplayStyle.Flex;

        // 애니메이션 루프
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = isVisible ? EaseOutBack(t) : EaseInCubic(t); // 나타날 때는 튕기는 효과, 사라질 때는 부드럽게

            // 스케일 조정
            _popup.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));

            // Y축 이동 (위에서 아래로 내려오는 효과)
            _popup.style.translate = new StyleTranslate(new Translate(0, Mathf.Lerp(startY, endY, easedT), 0));

            // 투명도 조정
            _popup.style.opacity = Mathf.Lerp(isVisible ? 0f : 1f, isVisible ? 1f : 0f, easedT);

            yield return null;
        }

        // 애니메이션 완료 후 처리
        if (!isVisible) _popup.style.display = DisplayStyle.None;

        // 최종 상태 확실히 설정
        if (isVisible)
        {
            _popup.style.scale = new StyleScale(endScale);
            _popup.style.translate = new StyleTranslate(new Translate(0, 0, 0));
            _popup.style.opacity = 1f;
        }
    }

    /// <summary>
    /// 이징 함수 (부드러운 애니메이션 곡선)
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    /// <summary>
    /// Ease In Cubic - 처음에는 천천히, 나중에는 빠르게
    /// </summary>
    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

    /// <summary>
    /// Ease Out Cubic - 처음에는 빠르게, 나중에는 천천히
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }

    /// <summary>
    /// Ease Out Back - 약간 지나치다가 돌아오는 효과
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    /// <summary>
    /// Ease In Out Quad - 2차 곡선으로 부드럽게
    /// </summary>
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    /// <summary>
    /// Ease Out Quad - 2차 곡선으로 빨리 줄어드는
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    /// <summary>
    /// Ease In Quad - 2차 곡선으로 천천히 시작하는
    /// </summary>
    private float EaseInQuad(float t)
    {
        return t * t;
    }

    /// <summary>
    /// Ease In Out Sine - 사인 함수 베이스 부드러운 전환
    /// </summary>
    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }
}