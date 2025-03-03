using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionUI : MonoBehaviour
{
    //UI Element
    public VisualElement root { get; private set;  }
    private VisualElement[] _panelArr;
    private Button[] _buttonArr;
    private VisualElement[] _clickVeArr;
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    [SerializeField] CompanionInfoUI _companionInfoUI;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
    }
    private void Start()
    {
        InitUI();
    }
    private void InitUI()
    {
        VisualElement panelParent = root.Q<VisualElement>("PanelParent");
        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        VisualElement clickVeParent = root.Q<VisualElement>("ClickVeParent");
        _panelArr = new VisualElement[panelParent.childCount];
        _buttonArr = new Button[buttonParent.childCount];
        _clickVeArr = new VisualElement[clickVeParent.childCount];

        for (int i = 0; i < panelParent.childCount; i++)
        {
            _panelArr[i] = panelParent.ElementAt(i);
        }
        for (int i = 0; i < buttonParent.childCount; i++)
        {
            int index = i;
            _buttonArr[i] = (Button)buttonParent.ElementAt(i);
            buttonParent.ElementAt(i).RegisterCallback<ClickEvent>(evt => OnClickButton(index));
        }
        InitCompanionPanel(clickVeParent);
        OnClickButton(0);
    }

    private void InitCompanionPanel(VisualElement clickVeParent)
    {
        for (int i = 0; i < clickVeParent.childCount; i++)
        {
            int index = i;
            _clickVeArr[i] = clickVeParent.ElementAt(i);
            clickVeParent.ElementAt(i).RegisterCallback<ClickEvent>(evt => OnClickClickVe(index));
        }
        CompanionController[] companionArr = CompanionManager.instance.companionArr;
        VisualElement statusParent = root.Q<VisualElement>("StatusParent");
        for (int i = 0; i < statusParent.childCount; i++)
        {
            
            VisualElement statusElement = statusParent.ElementAt(i);
            Label levelLabel = statusElement.Q<Label>("LevelLabel");
            Label nameLabel = statusElement.Q<Label>("NameLabel");
            nameLabel.text = CompanionManager.instance.companionArr[i].companionStatus.companionName;
            ProgressBar expProgressBar = statusElement.Q<ProgressBar>();
            (int, int) levelExp = CompanionManager.instance.GetCompanionLevelExp(i);
            levelLabel.text = $"Lv.{levelExp.Item1}";
            expProgressBar.value = levelExp.Item2 / (float)CompanionManager.EXPINTERVAL;
            expProgressBar.title = $"{levelExp.Item2}/{CompanionManager.EXPINTERVAL}";
        }

    }

    private void OnClickClickVe(int companionIndex)
    {
        _companionInfoUI.ActiveUI(companionIndex);
    }

    private void OnClickButton(int currentIndex)
    {
        for (int i = 0; i < _panelArr.Length; i++)
        {
            if (i == currentIndex)
            {
                _panelArr[i].style.display = DisplayStyle.Flex;
                _buttonArr[i].style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
                _buttonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
                _buttonArr[i].Q<Label>().style.color = activeColor;
            }
            else
            {
                _panelArr[i].style.display = DisplayStyle.None;
                _buttonArr[i].style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                _buttonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                _buttonArr[i].Q<Label>().style.color = inactiveColor;
            }
        }
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
        if (uiType == 3)
            root.style.display = DisplayStyle.Flex;
        else
            root.style.display = DisplayStyle.None;
    }
    #endregion
}