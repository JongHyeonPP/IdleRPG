using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;
using System;
using System.Linq;

public class StatUI : MonoBehaviour
{
    private GameData _gameData;
    private Coroutine _incrementCoroutine;
    
    private readonly StatusType[] _activeStats =
    {
        StatusType.Power,
        StatusType.MaxHp,
        StatusType.HpRecover,
        StatusType.Critical,
        StatusType.CriticalDamage
    };
    private Dictionary<StatusType, VisualElement> _statElements = new();
    public VisualElement root { get; private set; }
    private DraggableScrollView draggableScrollView;
    Button[] categoriButtons;
    VisualElement[] categoriPanels;
    //ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    [Header("Sprite")]
    [SerializeField] private Sprite powerSprite;
    [SerializeField] private Sprite maxHpSprite;
    [SerializeField] private Sprite hpRecoverSprite;
    [SerializeField] private Sprite criticalSprite;
    [SerializeField] private Sprite criticalDamageSprite;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        draggableScrollView = GetComponent<DraggableScrollView>();
        PlayerBroker.OnStatusLevelSet += UpdateStatText;
    }
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        categoriPanels = root.Q<VisualElement>("PanelParent").Children().ToArray();
        categoriButtons = root.Q<VisualElement>("ButtonParent").Children().Select(item=>(Button)item).ToArray();
        InitButton();
        InitEnhancePanel();
        OnCategoriButtonClick(0);
    }

    private void InitButton()
    {
        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        for (int i = 0; i < 3; i++)
        {
            Button categoriButton = categoriButtons[i];
            int index = i;//���� ����ȭ
            categoriButton.RegisterCallback<ClickEvent>(evt =>
            {
                OnCategoriButtonClick(index);
            });
        }
    }

    private void InitEnhancePanel()
    {
        foreach (var stat in _activeStats)
        {
            InitEnhanceElement(stat);
        }
        
    }
    void InitEnhanceElement(StatusType stat)
    {
        VisualElement elementRoot = categoriPanels[0].Q<VisualElement>($"{stat}Element");
        _statElements[stat] = elementRoot;
        Button button = elementRoot.Q<Button>("StatButton");
        Label levelLabel = elementRoot.Q<Label>("StatLevel");
        Label riseLabel = elementRoot.Q<Label>("StatRise");
        VisualElement statIcon = elementRoot.Q<VisualElement>("StatIcon");
        Label statName = elementRoot.Q<Label>("StatName");

        Sprite iconSprite = null;
        switch (stat)
        {
            case StatusType.Power:
                statName.text = "���ݷ�";
                iconSprite = powerSprite;
                break;
            case StatusType.MaxHp:
                statName.text = "ü��";
                iconSprite = maxHpSprite;
                break;
            case StatusType.HpRecover:
                statName.text = "ü�� ȸ��";
                iconSprite = hpRecoverSprite;
                break;
            case StatusType.Critical:
                statName.text = "ġ��Ÿ";
                iconSprite = criticalSprite;
                break;
            case StatusType.CriticalDamage:
                statName.text = "ġ��Ÿ ���ݷ�";
                iconSprite = criticalDamageSprite;
                break;
        }
        statIcon.style.backgroundImage = new(iconSprite);
        button.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(stat), TrickleDown.TrickleDown);
        button.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(), TrickleDown.TrickleDown);
        if (!_gameData.statLevel_Gold.ContainsKey(stat))
        {
            _gameData.statLevel_Gold[stat] = 1;
        }
        int currentLevel = _gameData.statLevel_Gold[stat];
        UpdateStatText(stat, currentLevel);
    }


    private void OnCategoriButtonClick(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            if (index == i)
            {
                categoriPanels[i].style.display = DisplayStyle.Flex;
                categoriButtons[i].style.unityBackgroundImageTintColor = new Color(activeColor.r,activeColor.g,activeColor.b, 0.1f);
                categoriButtons[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;    
                categoriButtons[i].Q<Label>().style.color = activeColor;    
            }
            else
            {
                categoriPanels[i].style.display = DisplayStyle.None;
                categoriButtons[i].style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                categoriButtons[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                categoriButtons[i].Q<Label>().style.color = inactiveColor;
            }
        }
    }

    private void OnPointerDown(StatusType stat)//���ݹ�ư������
    {
        int currentLevel = _gameData.statLevel_Gold[stat];
        int requiredGold = FormulaManager.GetGoldRequired(currentLevel); 

        if (_gameData.gold < requiredGold) 
        {
           Debug.Log("��尡 �����ϴ�.");
           return; 
        }
        _gameData.gold -= requiredGold;
        if (_incrementCoroutine == null)
        {
            _incrementCoroutine = StartCoroutine(IncreaseLevelContinuously(stat));
        }
       
    }
    private void OnPointerUp()//��ư���� ����������
    {
        if (_incrementCoroutine != null)
        {
            StopCoroutine(_incrementCoroutine);
            _incrementCoroutine = null;
            GameManager.instance.SaveLocalData();
        }
    }
    private IEnumerator IncreaseLevelContinuously(StatusType stat)
    {
        IncrementStat(stat);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            IncrementStat(stat);
        }
    }
    private void IncrementStat(StatusType stat)
    {
        _gameData.statLevel_Gold[stat]++;
        PlayerBroker.OnStatusLevelSet(stat, _gameData.statLevel_Gold[stat]);
        UpdateStatText(stat, _gameData.statLevel_Gold[stat]);
    }

    private void UpdateStatText(StatusType stat, int level)
    {
        var elementRoot = _statElements[stat];
        var levelLabel = elementRoot.Q<Label>("StatLevel");
        var riseLabel = elementRoot.Q<Label>("StatRise");
        var priceLabel = elementRoot.Q<Label>("PriceLabel");

        levelLabel.text = $"Level: {level}";

        riseLabel.text = FormulaManager.GetStatRiseText(level, stat);

        priceLabel.text = $"{FormulaManager.GetGoldRequired(level)}";
    }
    #region UIChange
    private void OnEnable()
    {
        UIBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        UIBroker.OnMenuUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 0)
            ShowStatUI();
        else
            HideStatUI();
    }
    public void HideStatUI()
    {
        root.style.display = DisplayStyle.None;

    }
    public void ShowStatUI()
    {
        root.style.display = DisplayStyle.Flex;

    }
    #endregion
}