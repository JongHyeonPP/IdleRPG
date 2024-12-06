using System;
using UnityEngine;
using UnityEngine.UIElements;
public class BossTimerUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        BattleBroker.OnBossEnter += OnBossEnter;
        BattleBroker.OnStageEnter += OnStageEnter;
    }

    private void OnStageEnter()
    {
        root.style.display = DisplayStyle.None;
    }

    private void OnBossEnter()
    {
        root.style.display = DisplayStyle.Flex;
    }
}