using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DuplicateLoginUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        StartBroker.OnDetectDuplicateLogin += OnDetectDuplicateLogin;
    }

    private void OnDetectDuplicateLogin()
    {
        root.style.display = DisplayStyle.Flex;
    }
}