using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EnumCollection;
using Newtonsoft.Json.Linq;
using Store.Gacha;
using Store.UI;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct ProductIconEntry
{
    public string key;
    public Texture2D iconTex;
}

/// <summary>
/// 상점 관리자 - 초기화 및 조율 담당
/// 가챠/UI 로직은 분리된 컴포넌트에 위임
/// </summary>
public class StoreManager : MonoSingleton<StoreManager>
{
    #region Fields

    [Header("Data")]
    private GameData _gameData;

    // (가챠 종류, 횟수) → (재화, 수량)
    private readonly Dictionary<(GachaType gachaType, int num), (Resource resource, int num)> prices = new();

    [Header("UI Documents")]
    [SerializeField] private UIDocument _storeUIDocument;
    [SerializeField] private UIDocument _storePopupDocument;
    [SerializeField] private AudioClip _popupSound;
    [SerializeField] private AudioClip _drawSound;

    [Header("Gacha Components")]
    [SerializeField] private GachaController _gachaController;
    [SerializeField] private GachaResultUI _gachaResultUI;
    [SerializeField] private SlotAnimator _slotAnimator;
    [SerializeField] private HamsterUI _hamsterUI;

    [Header("Money Components")]
    [SerializeField] private StoreMoneyListController _moneyList;
    [SerializeField] private List<ProductIconEntry> _iconEntries = new();

    private VisualElement _root;
    private Button _weapon1Btn;
    private Button _weapon10Btn;
    private Button _costume1Btn;
    private Button _costume10Btn;

    private AudioSource _audioSource;
    private Dictionary<string, Texture2D> _iconDic;

    // FX
    private VisualElement _storeFX;

    #endregion

    #region Properties

    public List<WeaponData> WeaponSaveDatas => _gachaController?.WeaponSaveDatas;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        _audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        InitPriceFromRc();
        InitUI();
        InitComponents();
        RefreshStore();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// RemoteConfig에서 가격 정보 로드
    /// </summary>
    private void InitPriceFromRc()
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
                if (string.IsNullOrEmpty(resourceStr))
                    throw new Exception($"resource가 비어 있음: {gType} x{n}");

                if (!Enum.TryParse<Resource>(resourceStr, true, out var resEnum))
                    throw new Exception($"알 수 없는 재화 타입: {resourceStr} ({gType} x{n})");

                int amount = node["amount"]?.Value<int>() ?? 0;
                if (amount <= 0)
                    throw new Exception($"amount가 유효하지 않음: {amount} ({gType} x{n})");

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
            _gachaResultUI?.ShowError("상점 가격 정보를 불러오지 못했습니다.\n잠시 후 다시 시도해 주세요.");
            throw;
        }
    }

    /// <summary>
    /// UI 요소 초기화
    /// </summary>
    private void InitUI()
    {
        if (_storeUIDocument == null) return;

        _root = _storeUIDocument.rootVisualElement;

        // 무기 패널
        var itemSlot0 = _root?.Q<VisualElement>("ItemSlot0");
        var storePanel0 = itemSlot0?.Q<VisualElement>("StorePanel_0");
        var storePanel1 = itemSlot0?.Q<VisualElement>("StorePanel_1");

        _weapon1Btn = storePanel0?.Q<Button>("StoreBtn");
        _weapon10Btn = storePanel1?.Q<Button>("StoreBtn");

        SetPriceLabel(storePanel0, GachaType.Weapon, 1);
        SetPriceLabel(storePanel1, GachaType.Weapon, 10);

        // 코스튬 패널
        var itemSlot1 = _root?.Q<VisualElement>("ItemSlot1");
        var storePanel0_1 = itemSlot1?.Q<VisualElement>("StorePanel_0");
        var storePanel1_1 = itemSlot1?.Q<VisualElement>("StorePanel_1");

        _costume1Btn = storePanel0_1?.Q<Button>("StoreBtn");
        _costume10Btn = storePanel1_1?.Q<Button>("StoreBtn");

        SetPriceLabel(storePanel0_1, GachaType.Costume, 1);
        SetPriceLabel(storePanel1_1, GachaType.Costume, 10);

        // 버튼 이벤트
        _weapon1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 1));
        _weapon10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Weapon, 10));
        _costume1Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 1));
        _costume10Btn?.RegisterCallback<ClickEvent>(async _ => await OnClickGacha(GachaType.Costume, 10));

        // FX
        _storeFX = _root?.Q<VisualElement>("StoreFX");
        if (_storeFX != null) _storeFX.pickingMode = PickingMode.Ignore;
    }

    private void SetPriceLabel(VisualElement panel, GachaType type, int num)
    {
        var priceLabel = panel?.Q<Label>("PriceLabel");
        var infoLabel = panel?.Q<Label>("InfoLabel");

        if (priceLabel != null && prices.TryGetValue((type, num), out var p))
            priceLabel.text = p.num.ToString();
        if (infoLabel != null)
            infoLabel.text = $"{num}회 뽑기";
    }

    /// <summary>
    /// 분리된 컴포넌트 초기화
    /// </summary>
    private void InitComponents()
    {
        _gachaController?.Initialize(_gameData, _audioSource);
        _gachaResultUI?.Initialize(_audioSource);
        _hamsterUI?.Initialize(_root);
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

    #region Money Store

    private void RefreshStore()
    {
        if (_moneyList == null || PurchaseManager.Instance == null) return;

        var pm = PurchaseManager.Instance;
        var items = new List<StoreMoneyItemData>();

        var products = pm.GetProducts(includeAdvertise: true)
            .OrderBy(p => CurrencyRank(p.grant.res))
            .ThenBy(p => AdRank(pm.IsAdvertise(p.productId)))
            .ThenBy(p => PriceKey(p.priceString))
            .ThenBy(p => p.grant.amt)
            .ToList();

        foreach (var p in products)
        {
            string priceString = string.IsNullOrEmpty(p.priceString) ? "-" : p.priceString;
            bool isAd = pm.IsAdvertise(p.productId) || p.source == "advertise";
            string moneyLabel = isAd ? "광고보기" : priceString;
            var icon = GetStoreIconTex(p.productId);

            items.Add(new StoreMoneyItemData
            {
                Gold = p.grant.amt.ToString(),
                GoldEx = p.grant.res.ToString(),
                Money = moneyLabel,
                Icon = icon,
                OnClick = () => TriggerProduct(p.productId)
            });
        }

        _moneyList.SetItems(items);
    }

    private static decimal PriceKey(string priceString)
    {
        if (string.IsNullOrWhiteSpace(priceString)) return decimal.MaxValue;
        var cleaned = new string(priceString.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        cleaned = cleaned.Replace(",", ".");
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : decimal.MaxValue;
    }

    private static void TriggerProduct(string productId)
    {
        if (PurchaseManager.Instance == null)
        {
            Debug.LogError("[Store] PurchaseManager.Instance is null");
            return;
        }
        PlayerBroker.PurchaseCurrency.Invoke(productId);
    }

    private Texture2D GetStoreIconTex(string productId)
    {
        if (_iconDic == null)
        {
            _iconDic = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in _iconEntries)
                if (!string.IsNullOrWhiteSpace(e.key) && e.iconTex != null)
                    _iconDic[e.key] = e.iconTex;
        }

        return !string.IsNullOrEmpty(productId) && _iconDic.TryGetValue(productId, out var tex) ? tex : null;
    }

    private static int CurrencyRank(Resource r) => r switch
    {
        Resource.Dia => 0,
        Resource.Clover => 1,
        _ => 2
    };

    private static int AdRank(bool isAd) => isAd ? 0 : 1;

    #endregion

    #region Public API (호환성)

    public void SetHamsterText(string text) => _hamsterUI?.SetText(text);
    public void ShowErrorPopup(string msg) => _gachaResultUI?.ShowError(msg);
    public void ClosePopup() => _gachaResultUI?.HideResult();
    public void CloseErrorPopup() => _gachaResultUI?.HideError();

    #endregion
}
