using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionUI : MonoBehaviour, IMenuUI
{
    private GameData _gameData;
    //UI Element
    public VisualElement root { get; private set;  }
    private VisualElement[] _panelArr;
    private Button[] _panelButtonArr;
    
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    [SerializeField] CompanionInfoUI _companionInfoUI;

    //Companion
    [SerializeField] CompanionTechUI _companionTechUI;
    private VisualElement[] _companionClickVeArr;
    //Tech
    private Button[] _techButtonArr;
    private int currentTechCompanionIndex;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        VisualElement panelParent = root.Q<VisualElement>("PanelParent");
        _panelArr = new VisualElement[panelParent.childCount];
        for (int i = 0; i < panelParent.childCount; i++)
        {
            _panelArr[i] = panelParent.ElementAt(i);
        }

        InitPanelButton();
        InitCompanionPanel();
        InitTechPanel();
        PlayerBroker.OnCompanionExpSet += OnCompanionExpSet;
        
    }
    private void InitPanelButton()
    {
        VisualElement buttonParent = root.Q<VisualElement>("ButtonParent");
        
        _panelButtonArr = new Button[buttonParent.childCount];
        
        for (int i = 0; i < buttonParent.childCount; i++)
        {
            int index = i;
            _panelButtonArr[i] = (Button)buttonParent.ElementAt(i);
            buttonParent.ElementAt(i).RegisterCallback<ClickEvent>(evt => OnPanelButtonClick(index));
        }
        OnPanelButtonClick(0);
    }

    private void InitCompanionPanel()
    {
        VisualElement companionClickVeParent = _panelArr[0].Q<VisualElement>("CompanionClickVeParent");
        _companionClickVeArr = new VisualElement[companionClickVeParent.childCount];
        
        for (int i = 0; i < companionClickVeParent.childCount; i++)
        {
            int index = i;
            _companionClickVeArr[i] = companionClickVeParent.ElementAt(i);
            companionClickVeParent.ElementAt(i).RegisterCallback<ClickEvent>(evt => OnClickCompanionVe(index));
        }
        CompanionController[] companionArr = CompanionManager.instance.companionArr;
        VisualElement statusParent = _panelArr[0].Q<VisualElement>("StatusParent");
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
    private void InitTechPanel()
    {
        VisualElement techPlankParent = _panelArr[1].Q<VisualElement>("TechPlankParent");
        Label[] plankLabelArr = techPlankParent.Children().Select(item =>item.Q<Label>()).ToArray();
        plankLabelArr[0].text = "기본 직업";
        plankLabelArr[1].text = "1차 전직";
        plankLabelArr[2].text = "2차 전직";
        plankLabelArr[3].text = "3차 전직";
        currentTechCompanionIndex = 0;
        InitTechButton();
        InitTechClickPanel(0,0);
        InitTechClickPanel(1,0);
        InitTechClickPanel(1,1);
        InitTechClickPanel(2,0);
        InitTechClickPanel(2,1);
        InitTechClickPanel(3,0);
        InitTechClickPanel(3,1);
    }
    private void InitTechButton()
    {
        VisualElement techButtonParent = root.Q<VisualElement>("TechButtonParent");

        _techButtonArr = new Button[techButtonParent.childCount];

        for (int i = 0; i < techButtonParent.childCount; i++)
        {
            int index = i;
            _techButtonArr[i] = (Button)techButtonParent.ElementAt(i);
            techButtonParent.ElementAt(i).RegisterCallback<ClickEvent>(evt => OnTechButtonClick(index));
        }
    }

    private void OnTechButtonClick(int techCompanionIndex)
    {
        currentTechCompanionIndex = techCompanionIndex;
        for (int i = 0; i < _techButtonArr.Length; i++)
        {
            if (i == techCompanionIndex)
            {
                _techButtonArr[i].style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
                _techButtonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
                _techButtonArr[i].Q<Label>().style.color = activeColor;
            }
            else
            {
                _techButtonArr[i].style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                _techButtonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                _techButtonArr[i].Q<Label>().style.color = inactiveColor;
            }
        }
        PlayerBroker.CompanionTechRenderSet(techCompanionIndex);
        SetCurrentTechData(0,0);

        SetCurrentTechData(1,0);
        SetCurrentTechData(1,1);
        
        SetCurrentTechData(2,0);
        SetCurrentTechData(2,1);
        
        SetCurrentTechData(3,0);
        SetCurrentTechData(3,1);
    }
    private void SetCurrentTechData(int techIndex_0, int techIndex_1)
    {
        VisualElement techClickPanel = _panelArr[1].Q<VisualElement>($"TechClickPanel_{techIndex_0}_{techIndex_1}");
        CompanionTechData companionTechData = CompanionManager.instance.GetCompanionTechData(currentTechCompanionIndex, techIndex_0, techIndex_1);
        if (_gameData.companionPromoteTech[currentTechCompanionIndex][techIndex_1] >= techIndex_0)
            PlayerBroker.CompanionTechRgbSet(1f, (techIndex_0, techIndex_1));
        else if (_gameData.companionPromoteTech[currentTechCompanionIndex][techIndex_1] + 1 == techIndex_0)
            PlayerBroker.CompanionTechRgbSet(0.5f, (techIndex_0, techIndex_1));
        else
            PlayerBroker.CompanionTechRgbSet(0f, (techIndex_0, techIndex_1));
        techClickPanel.Q<Label>("TechNameLabel").text = companionTechData.techName;
    }
    private void InitTechClickPanel(int techIndex_0, int techIndex_1)
    {
        VisualElement techClickPanel = _panelArr[1].Q<VisualElement>($"TechClickPanel_{techIndex_0}_{techIndex_1}");
        techClickPanel.Q<Button>("ClickButton").RegisterCallback<ClickEvent>(evt=>OnClickTechVe(techIndex_0, techIndex_1));
    }
    private void OnClickTechVe(int techIndex_0, int techIndex_1)
    {
        _companionTechUI.ActiveUI(currentTechCompanionIndex, techIndex_0, techIndex_1);
    }
    private void OnClickCompanionVe(int companionIndex)
    {
        _companionInfoUI.ActiveUI(companionIndex);
    }

    private void OnPanelButtonClick(int currentIndex)
    {
        for (int i = 0; i < _panelArr.Length; i++)
        {
            if (i == currentIndex)
            {
                _panelArr[i].style.display = DisplayStyle.Flex;
                _panelButtonArr[i].style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
                _panelButtonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
                _panelButtonArr[i].Q<Label>().style.color = activeColor;
            }
            else
            {
                _panelArr[i].style.display = DisplayStyle.None;
                _panelButtonArr[i].style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                _panelButtonArr[i].Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                _panelButtonArr[i].Q<Label>().style.color = inactiveColor;
            }
        }
    }
    private void OnCompanionExpSet(int companionIndex)
    {
        (int, int) levelExp = CompanionManager.instance.GetCompanionLevelExp(companionIndex);
        VisualElement status = root.Q<VisualElement>("StatusParent").ElementAt(companionIndex);

        Label levelLabel = status.Q<Label>("LevelLabel");
        ProgressBar expProgressBar = status.Q<ProgressBar>("ExpProgressBar");
        levelLabel.text = $"Lv.{levelExp.Item1}";
        expProgressBar.value = levelExp.Item2 / (float)CompanionManager.EXPINTERVAL;
        expProgressBar.title = $"{levelExp.Item2}/{CompanionManager.EXPINTERVAL}";
    }

    void IMenuUI.ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        OnTechButtonClick(currentTechCompanionIndex);
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
    }
}