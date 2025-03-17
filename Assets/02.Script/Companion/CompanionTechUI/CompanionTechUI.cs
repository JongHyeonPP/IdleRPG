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
    }
    private void InitRenderTexture(int index_0, int index_1)
    {
        _renderTextureArr[index_0][index_1] = root.Q<VisualElement>($"CompanionRenderTexture_{index_0}_{index_1}");
        _renderTextureArr[index_0][index_1].style.display = DisplayStyle.None;
    }
    public void ActiveUI(int companionIndex, int techIndex_0, int techIndex_1)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[companionIndex].companionStatus;
        CompanionTechData companionTechData;
        switch (techIndex_0)
        {
            default:
                companionTechData = companionStatus.companionTechData_0;
                break;
            case 1:
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_1_0;
                else
                    companionTechData = companionStatus.companionTechData_1_1;
                break;
            case 2:
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_2_0;
                else
                    companionTechData = companionStatus.companionTechData_2_1;
                break;
            case 3:
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_3_0;
                else
                    companionTechData = companionStatus.companionTechData_3_1;
                break;
        }
        int techDataIndex = _gameData.companionPromoteTech[companionIndex][techIndex_1];
        bool isAcquired = techDataIndex >= techIndex_0;
        if (isAcquired)
        {
            _nameLabel.text = companionTechData.techName;
            _nameLabel.style.color = _acquireColor;
        }
        else
        {
            _nameLabel.text = $"{companionTechData.techName} (πÃ»πµÊ)";
            _nameLabel.style.color = _unacquireColor;
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
