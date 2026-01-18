using System.Threading.Tasks;
using EnumCollection;
using Store.Gacha;
using Store.Money;
using Store.UI;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct ProductIconEntry
{
    public string key;
    public Texture2D iconTex;
}

/// <summary>
/// 상점 관리자 - 초기화 및 조율만 담당
/// </summary>
public class StoreManager : MonoSingleton<StoreManager>
{
    #region Fields

    [Header("UI Document")]
    [SerializeField] private UIDocument _storeUIDocument;

    [Header("Gacha Components")]
    [SerializeField] private GachaController _gachaController;
    [SerializeField] private GachaResultUI _gachaResultUI;
    [SerializeField] private HamsterUI _hamsterUI;

    [Header("Money Components")]
    [SerializeField] private MoneyStoreController _moneyStoreController;

    private VisualElement _root;
    private Button _weapon1Btn, _weapon10Btn;
    private Button _costume1Btn, _costume10Btn;
    private VisualElement _storeFX;

    #endregion

    #region Properties

    public System.Collections.Generic.List<WeaponData> WeaponSaveDatas => _gachaController?.WeaponSaveDatas;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        var gameData = StartBroker.GetGameData();

        InitUI();
        InitComponents(gameData);
    }

    #endregion

    #region Initialization

    private void InitUI()
    {
        if (_storeUIDocument == null) return;

        _root = _storeUIDocument.rootVisualElement;

        // 무기 패널
        var itemSlot0 = _root?.Q<VisualElement>("ItemSlot0");
        _weapon1Btn = itemSlot0?.Q<VisualElement>("StorePanel_0")?.Q<Button>("StoreBtn");
        _weapon10Btn = itemSlot0?.Q<VisualElement>("StorePanel_1")?.Q<Button>("StoreBtn");

        SetPriceLabel(itemSlot0?.Q<VisualElement>("StorePanel_0"), GachaType.Weapon, 1);
        SetPriceLabel(itemSlot0?.Q<VisualElement>("StorePanel_1"), GachaType.Weapon, 10);

        // 코스튬 패널
        var itemSlot1 = _root?.Q<VisualElement>("ItemSlot1");
        _costume1Btn = itemSlot1?.Q<VisualElement>("StorePanel_0")?.Q<Button>("StoreBtn");
        _costume10Btn = itemSlot1?.Q<VisualElement>("StorePanel_1")?.Q<Button>("StoreBtn");

        SetPriceLabel(itemSlot1?.Q<VisualElement>("StorePanel_0"), GachaType.Costume, 1);
        SetPriceLabel(itemSlot1?.Q<VisualElement>("StorePanel_1"), GachaType.Costume, 10);

        // 버튼 이벤트
        _weapon1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 1));
        _weapon10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 10));
        _costume1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 1));
        _costume10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 10));

        // FX
        _storeFX = _root?.Q<VisualElement>("StoreFX");
        if (_storeFX != null) _storeFX.pickingMode = PickingMode.Ignore;
    }

    private static void SetPriceLabel(VisualElement panel, GachaType type, int num)
    {
        var priceLabel = panel?.Q<Label>("PriceLabel");
        var infoLabel = panel?.Q<Label>("InfoLabel");

        if (priceLabel != null)
            priceLabel.text = GachaPriceService.GetAmount(type, num).ToString();
        if (infoLabel != null)
            infoLabel.text = $"{num}회 뽑기";
    }

    private void InitComponents(GameData gameData)
    {
        _gachaController?.Initialize(gameData);
        _gachaResultUI?.Initialize();
        _hamsterUI?.Initialize(_root);
        _moneyStoreController?.RefreshProducts();
    }

    #endregion

    #region Gacha

    private async Task OnClickGacha(GachaType type, int num)
    {
        SetButtonsEnabled(false);
        try
        {
            await _gachaController.ExecuteGacha(type, num);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool on)
    {
        _weapon1Btn?.SetEnabled(on);
        _weapon10Btn?.SetEnabled(on);
        _costume1Btn?.SetEnabled(on);
        _costume10Btn?.SetEnabled(on);
    }

    #endregion

    #region Store FX

    public void OpenStore()
    {
        if (_storeFX != null) _storeFX.style.display = DisplayStyle.Flex;
        ParticleFxManager.Instance.Play("StoreOpen");
    }

    #endregion

    #region Public API (호환성)

    public void SetHamsterText(string text) => _hamsterUI?.SetText(text);
    public void ShowErrorPopup(string msg) => _gachaResultUI?.ShowError(msg);
    public void ClosePopup() => _gachaResultUI?.HideResult();
    public void CloseErrorPopup() => _gachaResultUI?.HideError();

    #endregion
}
