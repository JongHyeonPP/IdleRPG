using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionUI : MonoBehaviour
{
    //UI Element
    public VisualElement root { get; private set;  }
    private VisualElement _companionPanel;
    private VisualElement _jobPanel;
    private Button _companionButton;
    private Button _jobButton;
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        InitUI();
    }
    private void InitUI()
    {
        _companionPanel = root.Q<VisualElement>("CompanionPanel");
        _jobPanel = root.Q<VisualElement>("JobPanel");
        _companionButton = root.Q<Button>("CompanionButton");
        _jobButton = root.Q<Button>("JobButton");
        _companionButton.RegisterCallback<ClickEvent>(evt => OnCompanionButtonClicked());
        _jobButton.RegisterCallback<ClickEvent>(evt => OnJobButtonClicked());
        OnCompanionButtonClicked();
    }
    private void OnCompanionButtonClicked()
    {
        _companionButton.style.unityBackgroundImageTintColor = _companionButton.style.color = activeColor;
        _jobButton.style.unityBackgroundImageTintColor = _jobButton.style.color = inactiveColor;
        _companionPanel.style.display = DisplayStyle.Flex;
        _jobPanel.style.display = DisplayStyle.None;
    }
    private void OnJobButtonClicked()
    {
        _companionButton.style.unityBackgroundImageTintColor = _companionButton.style.color = inactiveColor;
        _jobButton.style.unityBackgroundImageTintColor = _jobButton.style.color = activeColor;
        _companionPanel.style.display = DisplayStyle.None;
        _jobPanel.style.display = DisplayStyle.Flex;
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