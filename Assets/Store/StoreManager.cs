using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;


public class StoreManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // 무기 데이터들
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // 뽑힌 무기 데이터들

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // 이거 가져다 써 일단 정욱핑

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI 문서
    [SerializeField] private VisualTreeAsset _storeSlotItem;        //슬롯아이템 

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

    // System
    // 제네릭 Gacha 시스템 (WeaponData가 IGachaItem을 구현한다고 가정)
    private GachaSystem<WeaponData> _gachaSystem;
    // 예시로 코스튬 관련 가차 시스템 (코스튬 데이터가 IGachaItem을 구현해야 함)
    private GachaSystem<CostumeItem> __costumeGachaSystem;

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;

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

        // 제네릭 GachaSystem으로 변경 (타입 파라미터 추가)
        _gachaSystem = new GachaSystem<WeaponData>(_weaponDatas);
        // 만약 CostumeManager.instance.AllCostumeDatas가 CostumeData[] 타입이라면...
        __costumeGachaSystem = new GachaSystem<CostumeItem>(CostumeManager.instance.AllCostumeDatas);


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

        // Popup 비활성화
        SetPopupVisibility(false);

        _panel.style.display = DisplayStyle.Flex;

        _weapon1Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        _popupCloseBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));
        _openPopupBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(true));

        // 루트 요소에 클릭 이벤트 추가
        _popup.RegisterCallback<PointerDownEvent>(evt=>ClosePopup());

    }

    /// <summary>
    /// Popup 활성화 설정
    /// </summary>
    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null || _isPopupVisible == isVisible) return;

        _isPopupVisible = isVisible;
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
        // 초기화
        ClearGrid();

        // 뽑기
        List<WeaponData> drawnWeapons = _gachaSystem.DrawItems(count);

        // 저장된 무기데이터들
        // 이거 가져다가 추가해서 쓰면 되는디

        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
        PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;

        _weaponSaveDatas = drawnWeapons;
        
        //그리기
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }

    private void OnWeaponCountSet(object weaponDataObj, int count)
    {
        WeaponData weaponData = (WeaponData)weaponDataObj;
        VisualElement slot = _slotDict[weaponData.UID];
        ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");
        int level = _weaponLevel[weaponData.UID];
        int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
        countProgressBar.title = $"{count}/{price}";
        countProgressBar.value = count / (float)price;
    }

    /// <summary>
    /// 무기들 UI 그리기
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;

        SetPopupVisibility(true);

        int cnt = 0;
        float delayInterval = 1f;  // 슬롯 간 텀 (초)

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
        Vector3 endScale = new Vector3(1f, 1f, 1f);

        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;

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

        slot.style.scale = new StyleScale(endScale);
        slot.style.opacity = 1f;
    }


    /// <summary>
    /// 팝업에 애니메이션 효과 적용
    /// </summary>
    private IEnumerator AniPopup(bool isVisible)
    {
        float duration = 0.3f;  // 애니메이션 시간 (초)
        float elapsed = 0f;

        // 초기값과 목표값 설정
        Vector3 startScale = isVisible ? new Vector3(0.8f, 0.8f, 1f) : new Vector3(1f, 1f, 1f);
        Vector3 endScale = isVisible ? new Vector3(1f, 1f, 1f) : new Vector3(0.8f, 0.8f, 1f);

        // 팝업 표시 스타일 변경
        if (isVisible) _popup.style.display = DisplayStyle.Flex;

        // 애니메이션 루프
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            // 스케일 조정
            _popup.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));

            // 투명도 조정 (원하는 경우)
            _popup.style.opacity = Mathf.Lerp(isVisible ? 0f : 1f, isVisible ? 1f : 0f, easedT);

            yield return null;
        }

        // 애니메이션 완료 후 처리
        if (!isVisible) _popup.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// 이징 함수 (부드러운 애니메이션 곡선)
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

}
