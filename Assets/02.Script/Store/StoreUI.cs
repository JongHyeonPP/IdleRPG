using System;
using UnityEngine;
using UnityEngine.UIElements;

public class StoreUI : MonoBehaviour, IMenuUI
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

    void IMenuUI.ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
    }
}