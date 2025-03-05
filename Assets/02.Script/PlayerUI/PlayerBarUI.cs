using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBarUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private ProgressBar hpBar;
    private ProgressBar mpBar;
    void Awake()
    {
        InitElement();
        InitEvent();
        UIBroker.SetBarPosition += SetBarPosition;
    }

    private void InitElement()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        hpBar = root.Q<ProgressBar>("HpBar");
        mpBar = root.Q<ProgressBar>("MpBar");
    }

    private void SetBarPosition(Camera currentCamera)
    {
        VisualElement vertical = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Vertical");
        var controller = (MonoBehaviour)PlayerBroker.GetPlayerController();

        // ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
        Vector3 screenPoint = currentCamera.WorldToScreenPoint(controller.transform.position);

        float left = screenPoint.x - 100;
        float bottom = screenPoint.y - 105;
        vertical.style.left = left;
        vertical.style.bottom = bottom;  // Y�� ���� ����
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