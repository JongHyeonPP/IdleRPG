using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBarUI : MonoBehaviour
{
    private VisualElement _root;
    private ProgressBar hpBar;
    private ProgressBar mpBar;
    void Awake()
    {
        InitElement();
        InitEvent();
    }
    void Start()
    {
        InitPosition();
    }

    private void InitElement()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        hpBar = _root.Q<ProgressBar>("HpBar");
        mpBar = _root.Q<ProgressBar>("MpBar");
    }

    private void InitPosition()
    {
        VisualElement vertical = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Vertical");
        var controller = (MonoBehaviour)PlayerBroker.GetPlayerController();
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(controller.transform.position);
        vertical.style.left = screenPoint.x;
        vertical.style.top = Screen.height - screenPoint.y;
    }

    private void OnStageEnter()
    {
        hpBar.value = 1f;
    }
    private void InitEvent()
    {
        PlayerBroker.OnPlayerHpChanged += OnPlayerHpChanged;
        BattleBroker.OnStageEnter += OnStageEnter;
        PlayerBroker.OnPlayerMpChanged += OnPlayerMpChanged;
    }

    private void OnPlayerMpChanged(float ratio)
    {
        mpBar.value = ratio;
    }

    private void OnPlayerHpChanged(float ratio)
    {
        hpBar.value = ratio;
    }
}