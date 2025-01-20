using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class TranslucentBackground : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private VisualElement _currentUI;
    private bool _isDisplay;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        UIBroker.ActiveTranslucent += OnActiveTraslucent;
    }
    private void Start()
    {
        root.Q<VisualElement>("Background").RegisterCallback<ClickEvent>(click=>InvokeBroker());
        UIBroker.InactiveCurrentUI += () => { root.style.visibility = Visibility.Hidden; };
    }

    private void InvokeBroker()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    private void OnActiveTraslucent(VisualElement currentUI, bool isDisplay/*or visibility*/)
    {
        _isDisplay = isDisplay;
        _currentUI = currentUI;
        root.style.visibility = Visibility.Visible;
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
