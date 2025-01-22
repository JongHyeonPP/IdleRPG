using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;

public class StatUI : MonoBehaviour
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
    }
    private void Start()
    {
        _gameManager = GameManager.instance;
     
        
        foreach (var stat in _activeStats)
        {
            InitializeStatUI(stat);
        }
    }
 
  

    private void InitializeStatUI(StatusType stat)
    {
        var elementRoot = root.Q<VisualElement>($"{stat}Element");
        _statElements[stat] = elementRoot;
        var button = elementRoot.Q<Button>("StatButton");
        var levelLabel = elementRoot.Q<Label>("StatLevel");
        var riseLabel = elementRoot.Q<Label>("StatRise");
        var statIcon = elementRoot.Q<VisualElement>("StatIcon");

        Sprite iconSprite = null;
        switch (stat)
        {
            case StatusType.Power:
                iconSprite = powerSprite;
                break;
            case StatusType.MaxHp:
                iconSprite = maxHpSprite;
                break;
            case StatusType.HpRecover:
                iconSprite = hpRecoverSprite;
                break;
            case StatusType.Critical:
                iconSprite = criticalSprite;
                break;
            case StatusType.CriticalDamage:
                iconSprite = criticalDamageSprite;
                break;
        }
        statIcon.style.backgroundImage = new(iconSprite);
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
        PlayerBroker.OnStatusChange(stat, 1);
        UpdateStatText(stat, _gameManager.gameData.statLevel_Gold[stat]);
    }

    private void UpdateStatText(StatusType stat, int level)
    {
        var elementRoot = _statElements[stat];
        var levelLabel = elementRoot.Q<Label>("StatLevel");
        var riseLabel = elementRoot.Q<Label>("StatRise");
        var button = elementRoot.Q<Button>("StatButton");

        levelLabel.text = $"Level: {level}";

        riseLabel.text = FomulaManager.GetStatRiseText(level, stat);

        button.text = $"{FomulaManager.GetGoldRequired(level)}";
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
