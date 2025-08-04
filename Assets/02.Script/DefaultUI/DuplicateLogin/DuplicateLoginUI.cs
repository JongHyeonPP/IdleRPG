using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DuplicateLoginUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }

    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }

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