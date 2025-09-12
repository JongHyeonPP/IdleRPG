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


public class StoreManager : MonoSingleton<StoreManager>
{
    [Header("Data")]
    private GameData _gameData;


    //Ű = (� ��, �� �� ������), �� = (���� ��ȭ��, �� ������)
    //Ex) prices[(GachaType.Weapon, 1)] => (Resource.Dia, 10)
    Dictionary<(GachaType gachaType, int num), (Resource resource, int num)> prices = new();


    [SerializeField] private WeaponData[] _weaponDatas;             // ���� �����͵�
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // ���� ���� �����͵�

    //[SerializeField] private int _weapon1CoinPrice = 10;
    //[SerializeField] private int _weapon10CoinPrice = 100;

    //[SerializeField] private int _costume1CoinPrice = 10;
    //[SerializeField] private int _costume10CoinPrice = 100;



    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // �̰� ������ �� �ϴ� ������

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI ����
    [SerializeField] private UIDocument _storePopupDocument;           // UI ����
    [SerializeField] private VisualTreeAsset _storeSlotItem;        // ���Ծ�����
    [SerializeField] private Sprite _hamsterSprite;                 // �ܽ��� ��������Ʈ
    [SerializeField] private AudioClip _popupSound;                 // �˾� ȿ����
    [SerializeField] private AudioClip _drawSound;                  // �̱� ȿ����


    private VisualElement _root;

    // Weapon UI
    private VisualElement _panel;                                   // ���� �г�
    private VisualElement _weaponGrid;                              // ���� �׸���
    private Button _weapon1Btn;                                     // ���� 1ȸ �̱� ��ư
    private Button _weapon10Btn;                                    // ���� 10ȸ �̱� ��ư
    private Button _costume1Btn;                                    // �ڽ�Ƭ 1ȸ �̱� ��ư
    private Button _costume10Btn;                                   // �ڽ�Ƭ 10ȸ �̱� ��ư

    // Popup UI
    private VisualElement _popup;                                   // Popup VisualElement
    private VisualElement _rowVE1;                                  // Popup �� ù��°
    private VisualElement _rowVE2;                                  // Popup �� �ι�°
    private Button _popupCloseBtn;                                  // Popup �ݱ� ��ư
    private Button _openPopupBtn;                                   // Popup ���� ��ư

    // Hamster UI
    private VisualElement _hamsterUI;                               // �ܽ��� UI ���
    private Label _hamsterText;                                     // �ܽ��� ��ȭ �ؽ�Ʈ
    private VisualElement _hamsterImage;                            // �ܽ��� �̹���

    //��ƼŬ
    private VisualElement _storeFX;          // ��ƼŬ ��ġ ���ؿ� VE (Ŭ�� ���)

    // System
    // ���׸� Gacha �ý��� (WeaponData�� IGachaItem�� �����Ѵٰ� ����)
    private GachaSystem<WeaponData> _gachaSystem;
    // ���÷� �ڽ�Ƭ ���� ���� �ý��� (�ڽ�Ƭ �����Ͱ� IGachaItem�� �����ؾ� ��)
    private GachaSystem<CostumeItem> _costumeGachaSystem;

    private bool _isPopupVisible = false;
    private const int STORE_ROW = 2;
    private const int STORE_COLUMN = 5;
    private AudioSource _audioSource;

    // �ܽ��� ��ȭ �޽���
    private readonly string[] _hamsterMessages = new string[] {
        "�������!",
        "��!",
        "����� �����!",
        "����~!",
    };

    //Data
    private Dictionary<string, int> _weaponCount;

    private void Start() {
        _gameData = StartBroker.GetGameData();
        InitPriceFromRc();
        InitStore();
    }
    public void InitPriceFromRc()
    {
        try
        {
            // RC���� GACHA_INFO ��ü JSON ���ڿ� �������� (���� ĳ�ÿ��� ����)
            var json = RemoteConfigService.Instance.appConfig.GetJson("GACHA_INFO");
            if (string.IsNullOrEmpty(json))
                throw new Exception("GACHA_INFO�� ��� �ֽ��ϴ�.");

            var root = JObject.Parse(json);
            var cost = root["cost"] as JObject;
            if (cost == null)
                throw new Exception("GACHA_INFO.cost ��带 ã�� �� �����ϴ�.");

            void SetPrice(GachaType gType, int n, JToken node)
            {
                if (node == null) throw new Exception($"cost ��尡 �����ϴ�: {gType} x{n}");

                string resourceStr = node["resource"]?.ToString();
                if (string.IsNullOrEmpty(resourceStr))
                    throw new Exception($"resource�� ��� �ֽ��ϴ�: {gType} x{n}");

                if (!Enum.TryParse<Resource>(resourceStr, true, out var resEnum))
                    throw new Exception($"�� �� ���� ��ȭ Ÿ���Դϴ�: {resourceStr} ({gType} x{n})");

                int amount = node["amount"]?.Value<int>() ?? 0;
                if (amount <= 0)
                    throw new Exception($"amount�� �ùٸ��� �ʽ��ϴ�: {amount} ({gType} x{n})");

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
            Debug.LogError($"InitPriceFromRc ����: {e.Message}");
            throw;
        }
    }

    //private void OnEnable() => InitStore(); // ���� ������ �ּ� ó����

    /// <summary>
    /// �ʹ� ����
    /// </summary>
    private void InitStore()
    {
        if (_storeUIDocument == null) return;

        // ����� �ҽ� �߰�
        _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        // ���׸� GachaSystem���� ���� (Ÿ�� �Ķ���� �߰�)
        _gachaSystem = new GachaSystem<WeaponData>(_weaponDatas);
        // ���� CostumeManager.instance.AllCostumeDatas�� CostumeData[] Ÿ���̶��...
        //  __costumeGachaSystem = new GachaSystem<CostumeItem>(CostumeManager.Instance.AllCostumeDatas);

        // ���� ������ �޾Ƽ�? price�� ����
        //_weapon1CoinPrice = 1;
        //_weapon1CoinPrice = 10;

        var root = _storeUIDocument.rootVisualElement;
        _root = root; // ������
        _panel = root?.Q<VisualElement>("Panel");
        _weaponGrid = root?.Q<VisualElement>("WeaponGrid");
 

        #region ���� ����
        // Itemslot0 ��Ʈ���� ��������
        var itemSlot0 = _root?.Q<VisualElement>("ItemSlot0");

        // StorePanel_0, StorePanel_1 ����
        var storePanel0 = itemSlot0?.Q<VisualElement>("StorePanel_0");
        var storePanel1 = itemSlot0?.Q<VisualElement>("StorePanel_1");

        _weapon1Btn = storePanel0?.Q<Button>("StoreBtn");
        _weapon10Btn = storePanel1?.Q<Button>("StoreBtn");

        var priceLabel0 = storePanel0?.Q<Label>("PriceLabel");
        var infoLabel0 = storePanel0?.Q<Label>("InfoLabel");

        var priceLabel1 = storePanel1?.Q<Label>("PriceLabel");
        var infoLabel1 = storePanel1?.Q<Label>("InfoLabel");

        // �� �ٲٱ�
        if (priceLabel0 != null) priceLabel0.text = prices[(GachaType.Weapon, 1)].num.ToString();//���� 1�� �̴� ����
        if (infoLabel0 != null) infoLabel0.text = "1ȸ �̱�";

        if (priceLabel1 != null) priceLabel1.text = prices[(GachaType.Weapon, 10)].num.ToString();//���� 10�� �̴� ����
        if (infoLabel1 != null) infoLabel1.text = "10ȸ �̱�";
        #endregion

        #region �ڽ�Ƭ ����
        // Itemslot1  ��������
        var itemSlot1 = _root?.Q<VisualElement>("ItemSlot1");

        // StorePanel_0, StorePanel_1 ����
        var storePanel0_1 = itemSlot1?.Q<VisualElement>("StorePanel_0");
        var storePanel1_1 = itemSlot1?.Q<VisualElement>("StorePanel_1");

        _costume1Btn = storePanel0?.Q<Button>("StoreBtn");
        _costume10Btn = storePanel1?.Q<Button>("StoreBtn");

        var priceLabel0_1 = storePanel0_1?.Q<Label>("PriceLabel");
        var infoLabel0_1 = storePanel0_1?.Q<Label>("InfoLabel");

        var priceLabel1_1 = storePanel1_1?.Q<Label>("PriceLabel");
        var infoLabel1_1 = storePanel1_1?.Q<Label>("InfoLabel");

        // �� �ٲٱ�

        if (priceLabel0_1 != null) priceLabel0_1.text = prices[(GachaType.Costume, 1)].num.ToString();//���� 1�� �̴� ����
        if (infoLabel0_1 != null) infoLabel0_1.text = "1ȸ �̱�";

        if (priceLabel1_1 != null) priceLabel1_1.text = prices[(GachaType.Costume, 10)].num.ToString();//�ڽ�Ƭ 10�� �̴� ����
        if (infoLabel1_1 != null) infoLabel1_1.text = "10ȸ �̱�";
        #endregion

        var popuproot = _storePopupDocument.rootVisualElement;

        // Popup �ʱ�ȭ
        _popup = popuproot?.Q<VisualElement>("Popup");
        _popupCloseBtn = popuproot?.Q<Button>("PopupCloseBtn");
        _popup.style.display = DisplayStyle.None;
        _rowVE1 = popuproot?.Q<VisualElement>("RowVE1");
        _rowVE2 = popuproot?.Q<VisualElement>("RowVE2");
        // _openPopupBtn = root?.Q<Button>("OpenPopupBtn");


        // �ܽ��� UI �ʱ�ȭ
        _hamsterUI = root?.Q<VisualElement>("HamsterUI");
        _hamsterText = root?.Q<Label>("HamsterText");
        _hamsterImage = root?.Q<VisualElement>("HamsterImage");

/*        // �ܽ��� �̹��� ����
        if (_hamsterImage != null && _hamsterSprite != null)
        {
            _hamsterImage.style.backgroundImage = new StyleBackground(_hamsterSprite);
        }
*/
        // �ܽ��� �ʱ� �ؽ�Ʈ ����
        SetHamsterText(_hamsterMessages[0]);

        // Popup ��Ȱ��ȭ
        SetPopupVisibility(false);

        _weapon1Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        //_costume1Btn.RegisterCallback<ClickEvent>(evt => DrawCostumeItems(1));
       // _costume10Btn.RegisterCallback<ClickEvent>(evt => DrawCostumeItems(10));
        _popupCloseBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));
        //_openPopupBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(true));

        // ��Ʈ ��ҿ� Ŭ�� �̺�Ʈ �߰�
        _popup.RegisterCallback<PointerDownEvent>(evt => ClosePopup());

        _storeFX = root.Q<VisualElement>("StoreFX");
        if (_storeFX != null)
        {
            _storeFX.pickingMode = PickingMode.Ignore; // Ŭ��/����ĳ��Ʈ ���
        }

        // �����ý���
/*        _costumeGachaSystem = new GachaSystem<CostumeItem>(
            CostumeManager.Instance.AllCostumeDatas
                .Where(c => !CostumeManager.Instance.IsOwned(c.Uid)) // �ߺ� ����
                .ToArray()
        );*/
    }

    public void OpenStore()
    {
        // ���� �г� �����ֱ�(������Ʈ ��Ŀ� �°�)
        //if (_panel != null) _panel.style.display = DisplayStyle.Flex;

        // �ʿ��ϸ� �ʱ⿡ �˾�/�׸��� ����
//        SetPopupVisibility(false);
       // ClearGrid();

        // ��ƼŬ ���
        PlayStoreFxAt(_storeFX);
    }

    private void PlayStoreFxAt(VisualElement ve)
    {
        ve.style.display = DisplayStyle.Flex;
        ParticleFxManager.Instance.Play("StoreOpen");
    }


    /// <summary>
    /// �ܽ��� �ؽ�Ʈ ���� �Լ�
    /// </summary>
    /// <param name="text">ǥ���� �ؽ�Ʈ</param>
    public void SetHamsterText(string text)
    {
        if (_hamsterText == null) return;

        _hamsterText.text = text;

        // �ܽ��� �ؽ�Ʈ ǥ�� �ִϸ��̼�
        StartCoroutine(AnimateHamsterText(_hamsterText));
    }

    /// <summary>
    /// �ܽ��� �ؽ�Ʈ �ִϸ��̼�
    /// </summary>
    private IEnumerator AnimateHamsterText(Label textLabel)
    {
        // ���� ����
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

        // ���� ���� Ȯ���� ����
        textLabel.style.opacity = 1;
        textLabel.style.translate = new StyleTranslate(new Translate(0, 0, 0));
    }

   

    /// <summary>
    /// Popup Ȱ��ȭ ����
    /// </summary>
    private void SetPopupVisibility(bool isVisible)
    {
        if (_popup == null || _isPopupVisible == isVisible) return;

        _isPopupVisible = isVisible;

        // �˾� ȿ���� ���
        if (_popupSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_popupSound);
        }

        // �˾��� ���̸� �ܽ��� �޽��� ����
        if (isVisible)
        {
            SetHamsterText(_hamsterMessages[Random.Range(1, _hamsterMessages.Length)]);
        }
        else
        {
            SetHamsterText(_hamsterMessages[0]);
        }


        _popup.style.display = isVisible?DisplayStyle.Flex: DisplayStyle.None;
        //StartCoroutine(AniPopup(isVisible));  // �ִϸ��̼� ����
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
    public void DrawMultipleWeapons(int count)
    {
        // �̱� ȿ���� ���
        if (_drawSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_drawSound);
        }

        // �ʱ�ȭ
        ClearGrid();

        // �̱�
        List<WeaponData> drawnWeapons = _gachaSystem.DrawItems(count);

        // ����� ���ⵥ���͵�
        // �̰� �����ٰ� �߰��ؼ� ���� �Ǵµ�

        //var gameData = StartBroker.GetGameData();
        //var gameData = StartBroker.GetGameData();
       // _weaponCount = gameData.weaponCount;
        //PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;

        _weaponSaveDatas = drawnWeapons;

        // �ܽ��� �ؽ�Ʈ ������Ʈ - �̱� �� ����
        if (drawnWeapons.Count > 0)
        {
            bool hasRare = drawnWeapons.Exists(weapon => weapon.WeaponRarity >= Rarity.Rare);
            if (hasRare)
            {
                SetHamsterText("�;�! ");

            }
            else
            {
                SetHamsterText("���Ⱑ ���Ծ��! �������� �� ���� ���Ⱑ ���ðſ���!");
            }
        }

        //�׸���
        UpdateWeaponGridUI(drawnWeapons);
        UpdateLog(drawnWeapons);
    }


   

    //private void OnWeaponCountSet(object weaponDataObj, int count)
    //{
    //    WeaponData weaponData = (WeaponData)weaponDataObj;
    //    VisualElement slot = _slotDict[weaponData.UID];
    //    ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");
    //    int level = _weaponLevel[weaponData.UID];
    //    int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
    //    countProgressBar.title = $"{count}/{price}";
    //    countProgressBar.value = count / (float)price;
    //}

    /// <summary>
    /// ����� UI �׸���
    /// </summary>
    private void UpdateWeaponGridUI(List<WeaponData> weapons)
    {
        if (weapons == null) return;

        SetPopupVisibility(true);

        int cnt = 0;
        float delayInterval = 0.2f;  // ���� �� �� (��) - �� ������ ����

        foreach (var weapon in weapons)
        {
            // _storeSlotItem�� Ŭ��
            var slot = _storeSlotItem.CloneTree();

            // �����ܰ� �̸��� ������Ʈ
            var icon = slot.Q<VisualElement>("WeaponIcon");
            var weaponImageTexture = weapon.WeaponSprite.texture;
            var weaponImageStyle = new StyleBackground(weaponImageTexture);
            icon.style.backgroundImage = weaponImageStyle;
            icon.style.width = 180;
            icon.style.height = 180;
            //  icon.style.display = DisplayStyle.Flex;
            /*            icon.style.backgroundImage = new StyleBackground(weapon.WeaponSprite); // ��������Ʈ ����
                        icon.style.backgroundColor = Color.clear; // Ȥ�� ���� ���������� ����*/

            var nameLabel = slot.Q<Label>("WeaponName");
            if (nameLabel != null)
            {
               // nameLabel.text = WrapText(weapon.WeaponName.Replace(" ", "\n"), 7);
                nameLabel.text = WrapText(weapon.WeaponName, 7);
                nameLabel.style.height = 30;
            }

            // ���� ��͵��� ���� ���� ��Ÿ�� ����
            //  SetSlotRarityStyle(slot, weapon.WeaponRarity);

            // �׸��忡 �߰�
            if (cnt >= STORE_COLUMN)
                _rowVE2.Add(slot);
            else
                _rowVE1.Add(slot);

            // ���� �ִϸ��̼� ���� (���Ը��� ���� �ð� ����)
            //StartCoroutine(AniSlotDelay(slot, cnt * delayInterval));

            cnt++;
        }
    }

    /// <summary>
    /// ��͵��� ���� ���� ��Ÿ�� ����
    /// </summary>
    private void SetSlotRarityStyle(VisualElement slot, Rarity rarity)
    {
        if (slot == null) return;

        var background = slot.Q<VisualElement>("Background");
        if (background == null) return;

        // ��͵��� ���� ���� ����
        Color rarityColor;

        switch (rarity)
        {
            case Rarity.Common:
                rarityColor = new Color(0.7f, 0.7f, 0.7f); // ȸ��
                break;
            case Rarity.Uncommon:
                rarityColor = new Color(0.2f, 0.8f, 0.2f); // �ʷϻ�
                break;
            case Rarity.Rare:
                rarityColor = new Color(0.2f, 0.4f, 1f); // �Ķ���
                break;
            case Rarity.Unique:
                rarityColor = new Color(0.8f, 0.2f, 0.8f); // �����
                break;
            case Rarity.Legendary:
                rarityColor = new Color(1f, 0.8f, 0f); // �ݻ�
                break;
            default:
                rarityColor = Color.white;
                break;
        }

        background.style.backgroundColor = rarityColor;

        // ���� ����� ��� ������ ȿ�� �߰�
        if (rarity == Rarity.Legendary)
        {
            StartCoroutine(GlowingEffect(background));
        }
    }

    /// <summary>
    /// ���� ��� �����ۿ� ������ ȿ��
    /// </summary>
    private IEnumerator GlowingEffect(VisualElement element)
    {
        if (element == null) yield break;

        float minOpacity = 0.7f;
        float maxOpacity = 1f;
        float glowDuration = 1.5f;

        while (true)
        {
            // �������
            float elapsedTime = 0f;
            while (elapsedTime < glowDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / (glowDuration / 2));
                float easedT = EaseInOutSine(t);

                element.style.opacity = Mathf.Lerp(minOpacity, maxOpacity, easedT);

                yield return null;
            }

            // ��ο�����
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

    private void DrawCostumeItems(int count)
    {
        if (_drawSound != null && _audioSource != null)
            _audioSource.PlayOneShot(_drawSound);

        // �ߺ� ���ŵ� �����ͷ� �̱� �ý��� �����
        _costumeGachaSystem = new GachaSystem<CostumeItem>(
            CostumeManager.Instance.AllCostumeDatas
                .Where(c => !CostumeManager.Instance.IsOwned(c.Uid))
                .ToArray()
        );

        var drawnCostumes = _costumeGachaSystem.DrawItems(count);
        foreach (var costume in drawnCostumes)
        {
            CostumeManager.Instance.OwnedCostumes.Add(costume.Uid); // ���� ���
        }

        CostumeManager.Instance.UpdateCostumeData();

        SetHamsterText("�ڽ�Ƭ�� �����߾��!");

        SetPopupVisibility(true);
        UpdateCostumeGridUI(drawnCostumes);  // �Ʒ� �Լ� �ʿ�
    }

    private void UpdateCostumeGridUI(List<CostumeItem> costumes)
    {
        ClearGrid();

        int cnt = 0;
        foreach (var costume in costumes)
        {
            var slot = _storeSlotItem.CloneTree();

            var icon = slot.Q<VisualElement>("WeaponIcon"); // ���� �̸� ����
            icon.style.backgroundImage = new StyleBackground(costume.IconTexture);
            icon.style.width = 180;
            icon.style.height = 180;

            var nameLabel = slot.Q<Label>("WeaponName");
            if (nameLabel != null)
            {
                nameLabel.text = WrapText(costume.Name, 7);
                nameLabel.style.height = 30;
            }

            if (cnt >= STORE_COLUMN)
                _rowVE2.Add(slot);
            else
                _rowVE1.Add(slot);

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

    string WrapText(string text, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var words = text.Split(' ');
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int currentLineLength = 0;

        foreach (var word in words)
        {
            // �ܾ ���� �ٿ� �� �� ������ �߰�
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
                // �� �� ����
                sb.Append("\n");
                sb.Append(word);
                currentLineLength = word.Length;
            }
        }

        return sb.ToString();
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

            // ������ �� ������ ����
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


    /// <summary>
    /// �˾��� �ִϸ��̼� ȿ�� ����
    /// </summary>
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

    /// <summary>
    /// ��¡ �Լ� (�ε巯�� �ִϸ��̼� �)
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    /// <summary>
    /// Ease In Cubic - ó������ õõ��, ���߿��� ������
    /// </summary>
    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

    /// <summary>
    /// Ease Out Cubic - ó������ ������, ���߿��� õõ��
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }

    /// <summary>
    /// Ease Out Back - �ణ ����ġ�ٰ� ���ƿ��� ȿ��
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    /// <summary>
    /// Ease In Out Quad - 2�� ����� �ε巴��
    /// </summary>
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    /// <summary>
    /// Ease Out Quad - 2�� ����� ���� �پ���
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    /// <summary>
    /// Ease In Quad - 2�� ����� õõ�� �����ϴ�
    /// </summary>
    private float EaseInQuad(float t)
    {
        return t * t;
    }

    /// <summary>
    /// Ease In Out Sine - ���� �Լ� ���̽� �ε巯�� ��ȯ
    /// </summary>
    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }
    //�� �������� �� �� ������ �Ű������� �־ ȣ��
    private async Task CallGacha(GachaType type, int num)
    {
        Dictionary<string, object> args = new()
    {
        { "gachaType", type.ToString() },
        { "gachaNum",  num }
    };

        List<string> result = await CloudCodeService.Instance
            .CallModuleEndpointAsync<List<string>>(
                "PurchaseProcessor",
                "ProcessGacha",
                args);

        Debug.Log($"[Gacha] {type} x{num} => {string.Join(", ", result)}");

        var priceInfo = prices[(type, num)];
        switch (priceInfo.resource)
        {
            case Resource.Dia:
                _gameData.dia -= priceInfo.num;
                break;
            default:
                throw new Exception($"�������� �ʴ� ��ȭ Ÿ��: {priceInfo.resource}");
        }

        PlayerBroker.OnGacha?.Invoke(type, num); //��í ������ ������ ��������Ʈ�� ���� ���
    }

#if UNITY_EDITOR
    // ---- Weapon �׽�Ʈ ----
    [ContextMenu("GachaTest/Weapon x1")]
    public async void GachaTest_Weapon_1() => await CallGacha(GachaType.Weapon, 1);

    [ContextMenu("GachaTest/Weapon x10")]
    public async void GachaTest_Weapon_10() => await CallGacha(GachaType.Weapon, 10);

    // ---- Costume �׽�Ʈ ----
    [ContextMenu("GachaTest/Costume x1")]
    public async void GachaTest_Costume_1() => await CallGacha(GachaType.Costume, 1);

    [ContextMenu("GachaTest/Costume x10")]
    public async void GachaTest_Costume_10() => await CallGacha(GachaType.Costume, 10);
#endif

}