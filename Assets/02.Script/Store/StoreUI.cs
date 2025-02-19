using System;
using UnityEngine;
using UnityEngine.UIElements;

public class StoreUI : MonoBehaviour
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
        if (uiType == 5)
            root.style.display = DisplayStyle.Flex;
        else
            root.style.display = DisplayStyle.None;
    }
    #endregion
}