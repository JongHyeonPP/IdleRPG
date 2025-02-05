using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;
using System;

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
        PlayerBroker.OnStatusLevelSet+=UpdateStatText;
    }
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
     

        foreach (var stat in _activeStats)
        {
            InitializeStatUI(stat);
        }
    }


    private void InitializeStatUI(StatusType stat)
    {
        VisualElement elementRoot = root.Q<VisualElement>($"{stat}Element");
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
        button.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(stat),TrickleDown.TrickleDown);
        button.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(), TrickleDown.TrickleDown);
        if (!_gameData.statLevel_Gold.ContainsKey(stat))
        {
            _gameData.statLevel_Gold[stat] = 1; 
        }
        int currentLevel = _gameData.statLevel_Gold[stat];
        UpdateStatText(stat, currentLevel);
    }
    

    private void OnPointerDown(StatusType stat)//스텟버튼누르기
    {
        int currentLevel = _gameData.statLevel_Gold[stat]; 
        int requiredGold = FormulaManager.GetGoldRequired(currentLevel); 

        if (_gameData.gold < requiredGold) 
        {
           Debug.Log("골드가 없습니다.");
           return; 
        }
        _gameData.gold -= requiredGold;
        if (_incrementCoroutine == null)
        {
            _incrementCoroutine = StartCoroutine(IncreaseLevelContinuously(stat));
        }
       
    }

    private void OnPointerUp()//버튼떼서 데이터저장
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
        var button = elementRoot.Q<Button>("StatButton");

        levelLabel.text = $"Level: {level}";

        riseLabel.text = FormulaManager.GetStatRiseText(level, stat);

        button.text = $"{FormulaManager.GetGoldRequired(level)}";
    }
    #region UIChange
    private void OnEnable()
    {
        BattleBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnMenuUIChange -= HandleUIChange;
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
