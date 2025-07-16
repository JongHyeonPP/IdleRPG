using System;
using UnityEngine;
using UnityEngine.UIElements;

public class InvalidActUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;
        StartBroker.OnDetectInvalidAct += OnDetectInvalidAct;
    }

    private void OnDetectInvalidAct()
    {
        Debug.LogError("Invalid Act");
        root.style.display = DisplayStyle.Flex;
    }
}