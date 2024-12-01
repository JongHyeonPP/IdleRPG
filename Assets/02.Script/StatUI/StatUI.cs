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
    private VisualElement _scrollView;
    private bool _isDragging = false;
    private Vector2 _lastMousePosition;
    private VisualElement _content;
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
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        _gameManager = GameManager.instance;
        _scrollView = root.Q<VisualElement>("StatScrollView");
        _content = _scrollView.Q<VisualElement>("unity-content-container");
        _scrollView.RegisterCallback<PointerDownEvent>(OnScrollDown);
        _scrollView.RegisterCallback<PointerMoveEvent>(OnScrollMove);
        _scrollView.RegisterCallback<PointerUpEvent>(OnScrollUp);
        _scrollView.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            _isDragging = false;
        });
        foreach (var stat in _activeStats)
        {
            InitializeStatUI(root, stat);
        }
    }
    #region Scrollview
    private void OnScrollDown(PointerDownEvent evt)
    {
        _isDragging = true;
        _lastMousePosition = evt.position;
    }

    private void OnScrollMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        Vector2 delta = (Vector2)evt.position - _lastMousePosition;

        float currentY = _content.transform.position.y;

        float newY = currentY + delta.y;

        float minY = -1250;    
        float maxY = 50;
        if (newY > maxY) 
        {
            newY = maxY;
            StartCoroutine(SmoothMoveToOriginalY(-50)); 
        }
        else if(newY<minY)
        {
            newY = minY;
            StartCoroutine(SmoothMoveToOriginalY(-1150));
        }

        _content.transform.position = new Vector3(
            _content.transform.position.x,
            newY,
            0
        );

        _lastMousePosition = evt.position;
       
    }
    private IEnumerator SmoothMoveToOriginalY(float targetY)
    {
        float duration = 0.5f; 
        float elapsed = 0f;

        Vector3 startPosition = _content.transform.position;
        Vector3 targetPosition = new Vector3(
            _content.transform.position.x,
            targetY,
            0
        );

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _content.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / duration
            );
            yield return null;
        }

        _content.transform.position = targetPosition; 
    }
    private void OnScrollUp(PointerUpEvent evt)
    {
        _isDragging = false;
        evt.StopPropagation();
    }
    #endregion

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
    

    private void OnPointerDown(StatusType stat)
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

    private void OnPointerUp()
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
}
