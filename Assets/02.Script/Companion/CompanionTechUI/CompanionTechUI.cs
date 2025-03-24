using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionTechUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label _nameLabel;
    private GameData _gameData;
    private readonly Color _acquireColor = new Color(1f, 1f, 1f);
    private readonly Color _unacquireColor = new Color(0.8f,0.8f,0.8f);
    private VisualElement[][] _renderTextureArr = new VisualElement[4][];
    private int _currentIndex_0;
    private int _currentIndex_1;
    private bool _isAcquired;
    private Button _confirmButton;
    private CompanionTechData _currentTechData;
    private int _currentCompanionIndex;
    private (int,int) _currentTech;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _nameLabel = root.Q<Label>("NameLabel");
        _gameData = StartBroker.GetGameData();
        _renderTextureArr[0] = new VisualElement[1];
        _renderTextureArr[1] = new VisualElement[2];
        _renderTextureArr[2] = new VisualElement[2];
        _renderTextureArr[3] = new VisualElement[2];
        InitRenderTexture(0, 0);
        InitRenderTexture(1, 0);
        InitRenderTexture(1, 1);
        InitRenderTexture(2, 0);
        InitRenderTexture(2, 1);
        InitRenderTexture(3, 0);
        InitRenderTexture(3, 1);
        _currentIndex_0 = -1;
        _currentIndex_1 = -1;
        _confirmButton = root.Q<Button>("ConfirmButton");
        _confirmButton.RegisterCallback<ClickEvent>(evt => OnConfirmButtonClick());
    }

    private void OnConfirmButtonClick()
    {
        if (_isAcquired)
        {
            if (_gameData.currentCompanionPromoteTech[_currentCompanionIndex] != _currentTech)
            {
                PlayerBroker.OnCompanionAppearanceChange(_currentCompanionIndex, _currentTechData.appearanceData);
                _gameData.currentCompanionPromoteTech[_currentCompanionIndex] = _currentTech;
                StartBroker.SaveLocal();
            }
        }
        else
        {
            BattleBroker.SwitchToCompanionBattle(_currentCompanionIndex, _currentTech);
        }
        UIBroker.InactiveCurrentUI();
    }

    private void InitRenderTexture(int index_0, int index_1)
    {
        _renderTextureArr[index_0][index_1] = root.Q<VisualElement>($"CompanionRenderTexture_{index_0}_{index_1}");
        _renderTextureArr[index_0][index_1].style.display = DisplayStyle.None;
    }
    public void ActiveUI(int companionIndex, int techIndex_0, int techIndex_1)
    {
        _currentTech = (techIndex_0, techIndex_1);
        _currentCompanionIndex = companionIndex;
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[companionIndex].companionStatus;
        switch (techIndex_0)
        {
            default:
                _currentTechData = companionStatus.companionTechData_0;
                break;
            case 1:
                if (techIndex_1 == 0)
                    _currentTechData = companionStatus.companionTechData_1_0;
                else
                    _currentTechData = companionStatus.companionTechData_1_1;
                break;
            case 2:
                if (techIndex_1 == 0)
                    _currentTechData = companionStatus.companionTechData_2_0;
                else
                    _currentTechData = companionStatus.companionTechData_2_1;
                break;
            case 3:
                if (techIndex_1 == 0)
                    _currentTechData = companionStatus.companionTechData_3_0;
                else
                    _currentTechData = companionStatus.companionTechData_3_1;
                break;
        }
        int techDataIndex = _gameData.companionPromoteTech[companionIndex][techIndex_1];
        _isAcquired = techDataIndex >= techIndex_0;
        if (_isAcquired)
        {
            _nameLabel.text = $"{_currentTechData.techName} (È¹µæÇÔ)";
            _nameLabel.style.color = _acquireColor;
            _confirmButton.Q<Label>().text = "Á÷¾÷ º¯°æ";
        }
        else
        {
            _nameLabel.text = $"{_currentTechData.techName} (¹ÌÈ¹µæ)";
            _nameLabel.style.color = _unacquireColor;
            _confirmButton.Q<Label>().text  = "ÀüÅõÇÏ±â";
        }
        if (_currentIndex_0 != -1)
        {
            _renderTextureArr[_currentIndex_0][_currentIndex_1].style.display = DisplayStyle.None;
        }
        _renderTextureArr[techIndex_0][techIndex_1].style.display = DisplayStyle.Flex;
        _currentIndex_0 = techIndex_0;
        _currentIndex_1 = techIndex_1;
    }
}
