using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;


public class StoreManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // ���� �����͵�
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // ���� ���� �����͵�

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // �̰� ������ �� �ϴ� ������

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI ����
    [SerializeField] private VisualTreeAsset _storeSlotItem;        //���Ծ����� 

    private VisualElement _root;

    // Weapon UI
    private VisualElement _panel;                                   // ���� �г�
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
    // ���׸� Gacha �ý��� (WeaponData�� IGachaItem�� �����Ѵٰ� ����)
    private GachaSystem<WeaponData> _gachaSystem;
    // ���÷� �ڽ�Ƭ ���� ���� �ý��� (�ڽ�Ƭ �����Ͱ� IGachaItem�� �����ؾ� ��)
    private GachaSystem<CostumeItem> __costumeGachaSystem;

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;

    //Data
    private Dictionary<string, int> _weaponCount;

    private void Start() => InitStore();

    private void OnEnable() => InitStore(); // �׽�Ʈ��

    /// <summary>
    /// �ʹ� ����
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        // ���׸� GachaSystem���� ���� (Ÿ�� �Ķ���� �߰�)
        _gachaSystem = new GachaSystem<WeaponData>(_weaponDatas);
        // ���� CostumeManager.instance.AllCostumeDatas�� CostumeData[] Ÿ���̶��...
        __costumeGachaSystem = new GachaSystem<CostumeItem>(CostumeManager.instance.AllCostumeDatas);


        var root = _storeUIDocument.rootVisualElement;
        _root = root; // ������
        _panel = root?.Q<VisualElement>("Panel");
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

        _panel.style.display = DisplayStyle.Flex;

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
        if (_popup == null || _isPopupVisible == isVisible) return;

        _isPopupVisible = isVisible;
        StartCoroutine(AniPopup(isVisible));  // �ִϸ��̼� ����
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
        List<WeaponData> drawnWeapons = _gachaSystem.DrawItems(count);

        // ����� ���ⵥ���͵�
        // �̰� �����ٰ� �߰��ؼ� ���� �Ǵµ�

        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
        PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;

        _weaponSaveDatas = drawnWeapons;
        
        //�׸���
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
    /// ����� UI �׸���
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;

        SetPopupVisibility(true);

        int cnt = 0;
        float delayInterval = 1f;  // ���� �� �� (��)

        foreach (var weapon in weapons)
        {
            // _storeSlotItem�� Ŭ��
            var slot = _storeSlotItem.CloneTree();

            // �����ܰ� �̸��� ������Ʈ
            var icon = slot.Q<VisualElement>("WeaponIcon");
            var weaponImageTexture = weapon.WeaponSprite.texture;
            var weaponImageStyle = new StyleBackground(weaponImageTexture);
            icon.style.backgroundImage = weaponImageStyle;

            var nameLabel = slot.Q<Label>("WeaponName");
            if (nameLabel != null)
            {
                nameLabel.text = $"{weapon.WeaponName}";
            }

            // �׸��忡 �߰�
            if (cnt >= STORE_COLUMN)
                _rowVE2.Add(slot);
            else
                _rowVE1.Add(slot);

            // ���� �ִϸ��̼� ���� (���Ը��� ���� �ð� ����)
            StartCoroutine(AniSlotDelay(slot, cnt * delayInterval));

            cnt++;
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

    /// <summary>
    /// ���� �ִϸ��̼�
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private IEnumerator AnimateSlot(VisualElement slot)
    {
        float duration = 0.5f;  // �ִϸ��̼� �ð�
        float elapsed = 0f;

        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);  // �۰� ����
        Vector3 endScale = new Vector3(1f, 1f, 1f);         // ���� ũ��

        // �ʱ� ���� ����
        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;

        // �ִϸ��̼� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            // ������ �� ���� ����
            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);

            yield return null;
        }

        // �ִϸ��̼� �Ϸ� �� ���� ���� ����
        slot.style.scale = new StyleScale(endScale);
        slot.style.opacity = 1f;
    }

    /// <summary>
    /// ������ �� �ִϸ��̼�
    /// </summary>
    private IEnumerator AniSlotDelay(VisualElement slot, float delay)
    {
        // ���� ���� ���
        yield return new WaitForSeconds(delay);

        // �ִϸ��̼� ����
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

            // ������ �� ���� ����
            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);

            yield return null;
        }

        slot.style.scale = new StyleScale(endScale);
        slot.style.opacity = 1f;
    }


    /// <summary>
    /// �˾��� �ִϸ��̼� ȿ�� ����
    /// </summary>
    private IEnumerator AniPopup(bool isVisible)
    {
        float duration = 0.3f;  // �ִϸ��̼� �ð� (��)
        float elapsed = 0f;

        // �ʱⰪ�� ��ǥ�� ����
        Vector3 startScale = isVisible ? new Vector3(0.8f, 0.8f, 1f) : new Vector3(1f, 1f, 1f);
        Vector3 endScale = isVisible ? new Vector3(1f, 1f, 1f) : new Vector3(0.8f, 0.8f, 1f);

        // �˾� ǥ�� ��Ÿ�� ����
        if (isVisible) _popup.style.display = DisplayStyle.Flex;

        // �ִϸ��̼� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOutCubic(t);

            // ������ ����
            _popup.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));

            // ���� ���� (���ϴ� ���)
            _popup.style.opacity = Mathf.Lerp(isVisible ? 0f : 1f, isVisible ? 1f : 0f, easedT);

            yield return null;
        }

        // �ִϸ��̼� �Ϸ� �� ó��
        if (!isVisible) _popup.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// ��¡ �Լ� (�ε巯�� �ִϸ��̼� �)
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

}
