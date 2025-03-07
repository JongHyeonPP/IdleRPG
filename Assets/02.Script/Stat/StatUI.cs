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
    private readonly StatusType[] _statsByGold =
    {
        StatusType.Power,
        StatusType.MaxHp,
        StatusType.HpRecover,
        StatusType.Critical,
        StatusType.CriticalDamage
    };
    private readonly StatusType[] _statsByStatPoint =
    {
        StatusType.Power,
        StatusType.MaxHp,
        StatusType.HpRecover,
        StatusType.CriticalDamage,
        StatusType.GoldAscend
    };

    private readonly Dictionary<StatusType, VisualElement> _goldStatDict = new();
    private readonly Dictionary<StatusType, VisualElement> _statPointStatDict = new();
    public VisualElement root { get; private set; }
    [SerializeField] DraggableScrollView _enhanceScrollView;
    [SerializeField] DraggableScrollView _growScrollView;
    private DraggableScrollView lockedScrollView;
    private Button[] _categoriButtons;
    private VisualElement[] _categoriPanels;
    private Label _statPointLabel;
    //ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    [Header("Sprite")]
    [SerializeField] private Sprite powerSprite;
    [SerializeField] private Sprite maxHpSprite;
    [SerializeField] private Sprite hpRecoverSprite;
    [SerializeField] private Sprite criticalSprite;
    [SerializeField] private Sprite criticalDamageSprite;
    [SerializeField] private Sprite goldAscendSprite;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        PlayerBroker.OnGoldStatusLevelSet += UpdateGoldStatText;
        PlayerBroker.OnStatPointStatusLevelSet += UpdateStatPointStatText;
        BattleBroker.OnStatPointSet += StatPointSet;
    }
    private void StatPointSet()
    {
        _statPointLabel.text = $"STAT POINT : {_gameData.statPoint}"; 
    }

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        _categoriPanels = root.Q<VisualElement>("PanelParent").Children().ToArray();
        _categoriButtons = root.Q<VisualElement>("ButtonParent").Children().Select(item => (Button)item).ToArray();
        InitButton();
        InitEnhancePanel();
        InitGrowPanel();
        OnCategoriButtonClick(0);
    }

    private void InitGrowPanel()
    {
        _statPointLabel = _categoriPanels[1].Q<Label>("StatPointLabel");
        StatPointSet();
        foreach (var stat in _statsByStatPoint)
        {
            InitGrowElement(stat);
        }
    }

    private void InitGrowElement(StatusType stat)
    {
        VisualElement elementRoot = _categoriPanels[1].Q<VisualElement>($"{stat}Element");
        _statPointStatDict[stat] = elementRoot;
        VisualElement button = elementRoot.Q<VisualElement>("EventVe");
        Label levelLabel = elementRoot.Q<Label>("StatLevel");
        Label riseLabel = elementRoot.Q<Label>("StatRise");
        VisualElement statIcon = elementRoot.Q<VisualElement>("StatIcon");
        Label statName = elementRoot.Q<Label>("StatName");
        
        Sprite iconSprite = null;
        switch (stat)
        {
            case StatusType.Power:
                statName.text = "STR";
                iconSprite = powerSprite;
                break;
            case StatusType.MaxHp:
                statName.text = "HP";
                iconSprite = maxHpSprite;
                break;
            case StatusType.HpRecover:
                statName.text = "VIT";
                iconSprite = hpRecoverSprite;
                break;

            case StatusType.CriticalDamage:
                statName.text = "CRI";
                iconSprite = criticalDamageSprite;
                break;
            case StatusType.GoldAscend:
                statName.text = "LUK";
                iconSprite = goldAscendSprite;
                break;
        }
        statIcon.style.backgroundImage = new(iconSprite);
        button.RegisterCallback<PointerDownEvent>(evt => { OnPointerDown(stat, false); });
        button.RegisterCallback<PointerUpEvent>(evt => { OnPointerUp(); });
        if (!_gameData.statLevel_StatPoint.ContainsKey(stat))
        {
            _gameData.statLevel_StatPoint[stat] = 0;
        }
        int currentLevel = _gameData.statLevel_StatPoint[stat];
        UpdateStatPointStatText(stat, currentLevel);
    }

    void Update()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                OnPointerUp();
            }
        }
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0)) // 마우스 왼쪽 버튼을 떼는 순간
        {
            OnPointerUp();
        }
#endif
    }


    private void InitButton()
    {
        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        for (int i = 0; i < 3; i++)
        {
            Button categoriButton = _categoriButtons[i];
            int index = i;//로컬 변수화
            categoriButton.RegisterCallback<ClickEvent>(evt =>
            {
                OnCategoriButtonClick(index);
            });
        }
    }

    private void InitEnhancePanel()
    {
        foreach (var stat in _statsByGold)
        {
            InitEnhanceElement(stat);
        }
        
    }
    void InitEnhanceElement(StatusType stat)
    {
        VisualElement elementRoot = _categoriPanels[0].Q<VisualElement>($"{stat}Element");
        _goldStatDict[stat] = elementRoot;
        VisualElement button = elementRoot.Q<VisualElement>("EventVe");

        VisualElement statIcon = elementRoot.Q<VisualElement>("StatIcon");
        Label statName = elementRoot.Q<Label>("StatName");

        Sprite iconSprite = null;
        switch (stat)
        {
            case StatusType.Power:
                statName.text = "공격력";
                iconSprite = powerSprite;
                break;
            case StatusType.MaxHp:
                statName.text = "체력";
                iconSprite = maxHpSprite;
                break;
            case StatusType.HpRecover:
                statName.text = "체력 회복";
                iconSprite = hpRecoverSprite;
                break;
            case StatusType.Critical:
                statName.text = "치명타";
                iconSprite = criticalSprite;
                break;
            case StatusType.CriticalDamage:
                statName.text = "치명타 공격력";
                iconSprite = criticalDamageSprite;
                break;
        }
        statIcon.style.backgroundImage = new(iconSprite);
        button.RegisterCallback<PointerDownEvent>(evt => { OnPointerDown(stat, true); });
        button.RegisterCallback<PointerUpEvent>(evt => { OnPointerUp(); });
        if (!_gameData.statLevel_Gold.ContainsKey(stat))
        {
            _gameData.statLevel_Gold.Add(stat, 0);
        }
        int currentLevel = _gameData.statLevel_Gold[stat];
        UpdateGoldStatText(stat, currentLevel);
    }
    private void OnCategoriButtonClick(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            if (index == i)
            {
                _categoriPanels[i].style.display = DisplayStyle.Flex;
                _categoriButtons[i].style.unityBackgroundImageTintColor = new Color(activeColor.r,activeColor.g,activeColor.b, 0.1f);
                _categoriButtons[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;    
                _categoriButtons[i].Q<Label>().style.color = activeColor;    
            }
            else
            {
                _categoriPanels[i].style.display = DisplayStyle.None;
                _categoriButtons[i].style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                _categoriButtons[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                _categoriButtons[i].Q<Label>().style.color = inactiveColor;
            }
        }
    }

    private void OnPointerDown(StatusType stat, bool isByGold)//스텟버튼누르기
    {
        if (_incrementCoroutine == null)
        {
            lockedScrollView = isByGold ? _enhanceScrollView : _growScrollView;
            lockedScrollView.LockScrollPosition();
            _incrementCoroutine = StartCoroutine(PointerDownCoroutine(stat, isByGold));
        }
    }

    private IEnumerator PointerDownCoroutine(StatusType stat, bool isByGold)
    {
        if (isByGold)
        {
            IncreaseGoldStat(stat);
        }
        else
        {
            IncreaseStatPointStat(stat);
        }
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            if (isByGold)
            {
                IncreaseGoldStat(stat);
            }
            else
            {
                IncreaseStatPointStat(stat);
            }
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void OnPointerUp()//버튼떼서 데이터저장
    {
        if (_incrementCoroutine != null)
        {
            if (lockedScrollView)
            {
                lockedScrollView.UnlockScrollPosition();
                lockedScrollView = null;
            }
            StopCoroutine(_incrementCoroutine);
            _incrementCoroutine = null;
            GameManager.instance.SaveLocalData();
        }
    }

    private void IncreaseGoldStat(StatusType stat)
    {
        int requiredGold = FormulaManager.GetGoldRequired(_gameData.statLevel_Gold[stat]);

        if (_gameData.gold < requiredGold)
        {
            Debug.Log("골드가 없습니다.");
            return;
        }
        _gameData.gold -= requiredGold;
        _gameData.statLevel_Gold[stat]++;
        
        PlayerBroker.OnGoldStatusLevelSet(stat, _gameData.statLevel_Gold[stat]);
        BattleBroker.OnGoldSet?.Invoke();
    }
    private void IncreaseStatPointStat(StatusType stat)
    {
        int currentLevel = _gameData.statLevel_StatPoint[stat];

        if (_gameData.statPoint==0)
        {
            Debug.Log("스탯 포인트가 없습니다.");
            return;
        }
        _gameData.statPoint--;
        _gameData.statLevel_StatPoint[stat]++;
        PlayerBroker.OnStatPointStatusLevelSet(stat, _gameData.statLevel_StatPoint[stat]);
        BattleBroker.OnStatPointSet?.Invoke();

    }


    private void UpdateGoldStatText(StatusType statType, int level)
    {
        VisualElement elementRoot = _goldStatDict[statType];
        Label levelLabel = elementRoot.Q<Label>("StatLevel");
        Label riseLabel = elementRoot.Q<Label>("StatRise");
        Label priceLabel = elementRoot.Q<Label>("PriceLabel");

        levelLabel.text = $"Lv.{level}";

        int currentStat = FormulaManager.GetGoldStatus(level, statType);
        int nextStat = FormulaManager.GetGoldStatus(level+1, statType);
        riseLabel.text = FormulaManager.GetGoldStatRiseText(currentStat, nextStat, statType);

        priceLabel.text = $"{FormulaManager.GetGoldRequired(level)}";
    }
    private void UpdateStatPointStatText(StatusType statType, int level)
    {
        VisualElement elementRoot = _statPointStatDict[statType];
        Label levelLabel = elementRoot.Q<Label>("StatLevel");
        Label riseLabel = elementRoot.Q<Label>("StatRise");

        levelLabel.text = $"Lv.{level}";

        int currentStat = FormulaManager.GetStatPointStatus(level, statType);
        int nextStat = FormulaManager.GetStatPointStatus(level+1, statType);
        riseLabel.text = FormulaManager.GetStatPointStatRiseText(currentStat, nextStat, statType);
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