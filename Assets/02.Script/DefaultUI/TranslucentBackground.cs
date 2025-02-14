using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class TranslucentBackground : MonoBehaviour
{
    private VisualElement _currentUI;
    private bool _isDisplay;
    private VisualElement _background;
    private void Awake()
    {
        
    }
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        UIBroker.ActiveTranslucent += OnActiveTraslucent;
        _background = root.Q<VisualElement>("Background");
        _background.RegisterCallback<ClickEvent>(click=> InvokeBroker());
        _background.style.display = DisplayStyle.None;
        UIBroker.InactiveCurrentUI += () => { _background.style.display = DisplayStyle.None; };
    }

    private void InvokeBroker()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    private void OnActiveTraslucent(VisualElement currentUI, bool isDisplay/*or visibility*/)
    {
        _isDisplay = isDisplay;
        _currentUI = currentUI;
       _background.style.display = DisplayStyle.Flex;
        UIBroker.InactiveCurrentUI += () => CallBackCurrentUI();
    }
    private void CallBackCurrentUI()
    {
        if (_isDisplay)
        {
            _currentUI.style.display = DisplayStyle.None;
        }
        else
        {
            _currentUI.style.visibility = Visibility.Hidden;
        }
        UIBroker.InactiveCurrentUI -= () => CallBackCurrentUI();
    }
}
