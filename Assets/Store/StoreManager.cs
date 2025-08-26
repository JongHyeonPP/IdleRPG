using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;
using EnumCollection;


public class StoreManager : MonoSingleton<StoreManager>
{
    [Header("Data")]
    [SerializeField] private WeaponData[] _weaponDatas;             // ���� �����͵�
    [SerializeField] private List<WeaponData> _weaponSaveDatas;     // ���� ���� �����͵�

    public List<WeaponData> WeaponSaveDatas => _weaponSaveDatas;    // �̰� ������ �� �ϴ� ������

    [Header("UI")]
    [SerializeField] private UIDocument _storeUIDocument;           // UI ����
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

    // System
    // ���׸� Gacha �ý��� (WeaponData�� IGachaItem�� �����Ѵٰ� ����)
    private GachaSystem<WeaponData> _gachaSystem;
    // ���÷� �ڽ�Ƭ ���� ���� �ý��� (�ڽ�Ƭ �����Ͱ� IGachaItem�� �����ؾ� ��)
    private GachaSystem<CostumeItem> __costumeGachaSystem;

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

    private void Start() => InitStore();

    private void OnEnable() => InitStore(); // �׽�Ʈ��

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
        __costumeGachaSystem = new GachaSystem<CostumeItem>(CostumeManager.Instance.AllCostumeDatas);


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

        // �ܽ��� UI �ʱ�ȭ
        _hamsterUI = root?.Q<VisualElement>("HamsterUI");
        _hamsterText = root?.Q<Label>("HamsterText");
        _hamsterImage = root?.Q<VisualElement>("HamsterImage");

        // �ܽ��� �̹��� ����
        if (_hamsterImage != null && _hamsterSprite != null)
        {
            _hamsterImage.style.backgroundImage = new StyleBackground(_hamsterSprite);
        }

        // �ܽ��� �ʱ� �ؽ�Ʈ ����
        SetHamsterText(_hamsterMessages[0]);

        // Popup ��Ȱ��ȭ
        SetPopupVisibility(false);

        _panel.style.display = DisplayStyle.Flex;

        _weapon1Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(1));
        _weapon10Btn.RegisterCallback<ClickEvent>(evt => DrawMultipleWeapons(10));
        _popupCloseBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(false));
        _openPopupBtn.RegisterCallback<ClickEvent>(evt => SetPopupVisibility(true));

        // ��Ʈ ��ҿ� Ŭ�� �̺�Ʈ �߰�
        _popup.RegisterCallback<PointerDownEvent>(evt => ClosePopup());

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

        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
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
    //    int price = CurrencyManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
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

            var nameLabel = slot.Q<Label>("WeaponName");
            if (nameLabel != null)
            {
                nameLabel.text = $"{weapon.WeaponName}";
            }

            // ���� ��͵��� ���� ���� ��Ÿ�� ����
            SetSlotRarityStyle(slot, weapon.WeaponRarity);

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
        Vector3 endScale = new Vector3(1.1f, 1.1f, 1f);  // �ణ �� ũ�� Ȯ��
        Vector3 finalScale = new Vector3(1f, 1f, 1f);    // ���� ũ��

        slot.style.scale = new StyleScale(startScale);
        slot.style.opacity = 0f;
        slot.style.rotate = new StyleRotate(new Rotate(-5f)); // ��¦ Ʋ���� ���·� ����

        // ù ��° �ܰ�: Ȯ�� + ȸ�� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOutBack(t); // ƨ��� ȿ��

            // ������ �� ���� ����
            slot.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));
            slot.style.opacity = Mathf.Lerp(0f, 1f, easedT);
            slot.style.rotate = new StyleRotate(new Rotate(Mathf.Lerp(-5f, 0f, easedT)));

            yield return null;
        }

        // �� ��° �ܰ�: ���� ũ��� ���ư���
        elapsed = 0f;
        float secondDuration = 0.2f;

        while (elapsed < secondDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / secondDuration);
            float easedT = EaseInOutCubic(t);

            // ������ ����
            slot.style.scale = new StyleScale(Vector3.Lerp(endScale, finalScale, easedT));

            yield return null;
        }

        // ���� ���� ����
        slot.style.scale = new StyleScale(finalScale);
        slot.style.opacity = 1f;
        slot.style.rotate = new StyleRotate(new Rotate(0f));
    }


    /// <summary>
    /// �˾��� �ִϸ��̼� ȿ�� ����
    /// </summary>
    private IEnumerator AniPopup(bool isVisible)
    {
        float duration = 0.4f;  // �ִϸ��̼� �ð� (��)
        float elapsed = 0f;

        // �ʱⰪ�� ��ǥ�� ����
        Vector3 startScale = isVisible ? new Vector3(0.7f, 0.7f, 1f) : new Vector3(1f, 1f, 1f);
        Vector3 endScale = isVisible ? new Vector3(1f, 1f, 1f) : new Vector3(0.7f, 0.7f, 1f);
        float startY = isVisible ? 30f : 0f;
        float endY = isVisible ? 0f : 30f;

        // �˾� ǥ�� ��Ÿ�� ����
        if (isVisible) _popup.style.display = DisplayStyle.Flex;

        // �ִϸ��̼� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = isVisible ? EaseOutBack(t) : EaseInCubic(t); // ��Ÿ�� ���� ƨ��� ȿ��, ����� ���� �ε巴��

            // ������ ����
            _popup.style.scale = new StyleScale(Vector3.Lerp(startScale, endScale, easedT));

            // Y�� �̵� (������ �Ʒ��� �������� ȿ��)
            _popup.style.translate = new StyleTranslate(new Translate(0, Mathf.Lerp(startY, endY, easedT), 0));

            // ���� ����
            _popup.style.opacity = Mathf.Lerp(isVisible ? 0f : 1f, isVisible ? 1f : 0f, easedT);

            yield return null;
        }

        // �ִϸ��̼� �Ϸ� �� ó��
        if (!isVisible) _popup.style.display = DisplayStyle.None;

        // ���� ���� Ȯ���� ����
        if (isVisible)
        {
            _popup.style.scale = new StyleScale(endScale);
            _popup.style.translate = new StyleTranslate(new Translate(0, 0, 0));
            _popup.style.opacity = 1f;
        }
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
}