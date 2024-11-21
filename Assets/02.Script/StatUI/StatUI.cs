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
   
    private void Start()
    {
        _gameManager = GameManager.instance;
      

        var root = GetComponent<UIDocument>().rootVisualElement;
        _scrollView = root.Q<VisualElement>("StatScrollView");
        _content = _scrollView.Q<VisualElement>("unity-content-container");

        _content.RegisterCallback<PointerDownEvent>(OnScrollDown);
        _content.RegisterCallback<PointerMoveEvent>(OnScrollMove);
        _content.RegisterCallback<PointerUpEvent>(OnScrollUp);
        foreach (var stat in _activeStats)
        {
            InitializeStatUI(root, stat);
        }

    }

    private void OnScrollDown(PointerDownEvent evt)
    {
        _isDragging = true;
        _lastMousePosition = evt.position; 
        evt.StopPropagation(); 
    }

    private void OnScrollMove(PointerMoveEvent evt)
    {
        if (_isDragging)
        {
          
            Vector2 delta = (Vector2)evt.position - _lastMousePosition;

            _content.transform.position = new Vector3(_content.transform.position.x, _content.transform.position.y - delta.y, 0);

            _lastMousePosition = evt.position;
        }
    }

    private void OnScrollUp(PointerUpEvent evt)
    {
        _isDragging = false; 
    }


    private void InitializeStatUI(VisualElement root, StatusType stat)
    {
        var button = root.Q<Button>($"{stat}Button");
        var levelLabel = root.Q<Label>($"{stat}Level");
        var riseLabel = root.Q<Label>($"{stat}rise");
    
        _statElements[stat] = levelLabel;

        button.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(stat),TrickleDown.TrickleDown);
        button.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(), TrickleDown.TrickleDown);
       

    }
    

    private void OnPointerDown(StatusType stat)
    {
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
      //  _gameManager.ChangeStatLevel(stat,0);
        Debug.Log(stat + "¿Ã¶ù´Ù1");
        UpdateStatLevelLabel(stat);
    }

    private void UpdateStatLevelLabel(StatusType stat)
    {
        var levelLabel = _statElements[stat] as Label;
        if (levelLabel != null)
        {
            levelLabel.text = $"Level ";
            Debug.Log(stat + "¿Ã¶ù´Ù");
        }
    }
    private void UpdateStatRiseLabel(StatusType stat)
    {
        
    }
}
