using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // 무기 데이터들
    [SerializeField] private List<WeaponData> _weaponSaveDatas;             // 뽑힌 무기 데이터들

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas; // 이거 가져다 써 일단 정욱핑

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI 문서
    [SerializeField] private VisualTreeAsset _storeSlotItem;        //슬롯아이템 

    private VisualElement _root; 

    // Weapon UI
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
    private GachaSystem _gachaSystem;                               // 가차 시스템

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;

    private void Start() => InitStore();

    /// <summary>
    /// 초반 세팅
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        _gachaSystem = new GachaSystem(_weaponDatas);

        var root = _storeUIDocument.rootVisualElement;
        _root = root; // 저장핑
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
        if (_popup == null) return;
        
        _popup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        _isPopupVisible = isVisible;
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
        List<WeaponData> drawnWeapons = _gachaSystem.DrawWeapons(count);

        // 저장된 무기데이터들
        // 이거 가져다가 추가해서 쓰면 되는디
        _weaponSaveDatas = drawnWeapons;
        
        //그리기
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }

    /// <summary>
    /// 무기들 UI 그리기
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;
        
        SetPopupVisibility(true);

        int cnt = 0;
        foreach (var weapon in weapons)
        {
            // _storeSlotItem을 클론
            var slot = _storeSlotItem.CloneTree();

            // 아이콘과 이름을 업데이트
            var icon = slot.Q<VisualElement>("WeaponIcon"); // 아이콘
            var weaponImageTexture = weapon.WeaponSprite.texture;
            var weaponImageStyle = new StyleBackground(weaponImageTexture);
            icon.style.backgroundImage = weaponImageStyle;

            var nameLabel = slot.Q<Label>("WeaponName"); // 라벨
            if (nameLabel != null)
            {
                nameLabel.text = $"{weapon.WeaponName}"; // 이름 설정
            }

            cnt++;

            if(cnt > STORE_COLUMN)
                _rowVE2.Add(slot);
            else 
                _rowVE1.Add(slot);
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
}
