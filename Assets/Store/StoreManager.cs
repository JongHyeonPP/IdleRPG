using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // ���� �����͵�
    [SerializeField] private List<WeaponData> _weaponSaveDatas;             // ���� ���� �����͵�

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas; // �̰� ������ �� �ϴ� ������

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI ����
    [SerializeField] private VisualTreeAsset _storeSlotItem;        //���Ծ����� 

    private VisualElement _root; 

    // Weapon UI
    private VisualElement _weaponGrid;                              // ���� �׸���
    private Button _weapon1Btn;                                     // ���� 1ȸ �̱� ��ư
    private Button _weapon10Btn;                                    // ���� 10ȸ �̱� ��ư
    
    // Popup UI
    private VisualElement _popup;                                   // Popup VisualElement
    private VisualElement _rowVE1;                                  // Popup �� ù��°
    private VisualElement _rowVE2;                                  // Popup �� �ι�°
    private Button _popupCloseBtn;                                  // Popup �ݱ� ��ư
    private Button _openPopupBtn;                                   // Popup ���� ��ư

    // System
    private GachaSystem _gachaSystem;                               // ���� �ý���

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;

    private void Start() => InitStore();

    /// <summary>
    /// �ʹ� ����
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        _gachaSystem = new GachaSystem(_weaponDatas);

        var root = _storeUIDocument.rootVisualElement;
        _root = root; // ������
        _weaponGrid = root?.Q<VisualElement>("WeaponGrid");
        _rowVE1 = root?.Q<VisualElement>("RowVE1");
        _rowVE2 = root?.Q<VisualElement>("RowVE2");
        _weapon1Btn = root?.Q<Button>("Weapon1Btn");
        _weapon10Btn = root?.Q<Button>("Weapon10Btn");

        // Popup �ʱ�ȭ
        _popup = root?.Q<VisualElement>("Popup");
        _popupCloseBtn = root?.Q<Button>("PopupCloseBtn");
        _openPopupBtn = root?.Q<Button>("OpenPopupBtn");

        // Popup ��Ȱ��ȭ
        SetPopupVisibility(false);

        _weapon1Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        _popupCloseBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));
        _openPopupBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(true));

        // ��Ʈ ��ҿ� Ŭ�� �̺�Ʈ �߰�
        _popup.RegisterCallback<PointerDownEvent>(evt=>ClosePopup());

    }

    /// <summary>
    /// Popup Ȱ��ȭ ����
    /// </summary>
    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null) return;
        
        _popup.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        _isPopupVisible = isVisible;
    }

    /// <summary>
    /// Ŭ���� �˾� �ܺο��� �߻����� ��� �˾��� �ݴ� �Լ�
    /// </summary>
    public void ClosePopup()
    {
        SetPopupVisibility(false);
    }

    /// <summary>
    /// ����� �̾Ƽ� UI �׸���
    /// </summary>
    private void DrawMultipleWeapons(int count)
    {
        // �ʱ�ȭ
        ClearGrid();

        // �̱�
        List<WeaponData> drawnWeapons = _gachaSystem.DrawWeapons(count);

        // ����� ���ⵥ���͵�
        // �̰� �����ٰ� �߰��ؼ� ���� �Ǵµ�
        _weaponSaveDatas = drawnWeapons;
        
        //�׸���
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }

    /// <summary>
    /// ����� UI �׸���
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;
        
        SetPopupVisibility(true);

        int cnt = 0;
        foreach (var weapon in weapons)
        {
            // _storeSlotItem�� Ŭ��
            var slot = _storeSlotItem.CloneTree();

            // �����ܰ� �̸��� ������Ʈ
            var icon = slot.Q<VisualElement>("WeaponIcon"); // ������
            var weaponImageTexture = weapon.WeaponSprite.texture;
            var weaponImageStyle = new StyleBackground(weaponImageTexture);
            icon.style.backgroundImage = weaponImageStyle;

            var nameLabel = slot.Q<Label>("WeaponName"); // ��
            if (nameLabel != null)
            {
                nameLabel.text = $"{weapon.WeaponName}"; // �̸� ����
            }

            cnt++;

            if(cnt > STORE_COLUMN)
                _rowVE2.Add(slot);
            else 
                _rowVE1.Add(slot);
        }
    }

    // �׸��� �ʱ�ȭ
    private void ClearGrid() 
    {
        _rowVE1?.Clear();
        _rowVE2?.Clear();
    }

    // �α׻̱�
    private void UpdateLog(List<WeaponData> weapons)
    {
        string log = "�̱� ���:";
        foreach (var weapon in weapons)
        {
            log += $"- {weapon.name} ({weapon.WeaponRarity})\n";
        }
        Debug.Log(log);
    }
}
