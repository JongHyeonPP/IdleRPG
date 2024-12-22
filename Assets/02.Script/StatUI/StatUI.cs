using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;

public class StatUI : DraggableScrollView
{
    private GameManager _gameManager;
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
    protected override float MinY => -1150;
    protected override float MaxY => -50f;
    protected override void Awake()
    {
        _scrollviewName = "StatScrollView";
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();
        _gameManager = GameManager.instance;
     
        
        foreach (var stat in _activeStats)
        {
            InitializeStatUI(root, stat);
        }
    }
 
  

    private void InitializeStatUI(VisualElement root, StatusType stat)
    {
        var button = root.Q<Button>($"{stat}Button");
        var levelLabel = root.Q<Label>($"{stat}Level");
        var riseLabel = root.Q<Label>($"{stat}Rise");
    
        _statElements[stat] = levelLabel;

        button.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(stat),TrickleDown.TrickleDown);
        button.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(), TrickleDown.TrickleDown);
        if (!_gameManager.gameData.statLevel_Gold.ContainsKey(stat))
        {
            _gameManager.gameData.statLevel_Gold[stat] = 1; 
        }
        int currentLevel = _gameManager.gameData.statLevel_Gold[stat];
        UpdateStatText(stat, currentLevel);
    }
    

    private void OnPointerDown(StatusType stat)//스텟버튼누르기
    {
        int currentLevel = _gameManager.gameData.statLevel_Gold[stat]; 
        int requiredGold = FomulaManager.GetGoldRequired(currentLevel); 

        if (_gameManager.gameData.gold < requiredGold) 
        {
           Debug.Log("골드가 없습니다.");
           return; 
        }
        _gameManager.gameData.gold -= requiredGold;
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
            DataManager.SaveToPlayerPrefs("GameData", _gameManager.gameData);
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
        _gameManager.gameData.statLevel_Gold[stat]++;
        BattleBroker.OnStatusChange(stat, 1);
        UpdateStatText(stat, _gameManager.gameData.statLevel_Gold[stat]);
    }

    private void UpdateStatText(StatusType stat, int level)
    {
        var levelLabel = (Label)_statElements[stat];
        var riseLabel = _scrollView.Q<Label>($"{stat}Rise");
        var button = _scrollView.Q<Button>($"{stat}Button");

        levelLabel.text = $"Level: {level}";

        riseLabel.text = FomulaManager.GetStatRiseText(level, stat);

        button.text = $"{FomulaManager.GetGoldRequired(level)}";
    }
    #region UIChange
    private void OnEnable()
    {
        BattleBroker.OnUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 1)
            ShowStatUI();
        else
            HideStatUI();
    }
    public void HideStatUI()
    {
        _scrollView.style.display = DisplayStyle.None;

    }
    public void ShowStatUI()
    {
        _scrollView.style.display = DisplayStyle.Flex;

    }
    #endregion
}
