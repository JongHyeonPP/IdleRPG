using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBarUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    private ProgressBar _hpBar;
    private ProgressBar _delayedBar;
    private ProgressBar _mpBar;
    private Coroutine _delayedHpCoroutine;
    void Awake()
    {
        InitElement();
        InitEvent();
        UIBroker.SetPlayerBarPosition += SetBarPosition;
        root.style.display = DisplayStyle.None;
    }

    private void InitElement()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _hpBar = root.Q<ProgressBar>("HpBar");
        _hpBar.Q<VisualElement>(className: "unity-progress-bar__progress").style.backgroundColor = new Color(1f, 0.22f, 0.22f);
        _delayedBar = root.Q<ProgressBar>("DelayedBar");
        _mpBar = root.Q<ProgressBar>("MpBar");
    }

    private void SetBarPosition()
    {
        VisualElement vertical = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Vertical");
        MonoBehaviour controller = (MonoBehaviour)BattleBroker.GetPlayerController();

        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(controller.transform.position);

        float left = screenPoint.x - 100f;
        float bottom = screenPoint.y - 105f;
        vertical.style.left = left;
        vertical.style.bottom = bottom;  // Y축 반전 제거
        root.style.display = DisplayStyle.Flex;
    }

    private void OnStageEnter()
    {
        _hpBar.value = 1f;
        _delayedBar.value = 1f;
    }
    private void InitEvent()
    {
        PlayerBroker.OnPlayerHpChanged += OnPlayerHpChanged;
        BattleBroker.SwitchToBattle += OnStageEnter;
        PlayerBroker.OnPlayerMpChanged += OnPlayerMpChanged;
    }

    private void OnPlayerMpChanged(float ratio)
    {
        _mpBar.value = ratio;
    }



    private void OnPlayerHpChanged(float ratio)
    {
        _hpBar.value = ratio;
        _delayedBar.value = ratio;
        //if (_delayedHpCoroutine != null)
        //    StopCoroutine(_delayedHpCoroutine);

        //_delayedHpCoroutine = StartCoroutine(AnimateDelayedHpBar(ratio));
    }

    private IEnumerator AnimateDelayedHpBar(float targetRatio)
    {
        float current = _delayedBar.value;
        float speed = 2f; // 초당 1.0 비율만큼 감소 (속도 조절 가능)

        while (_delayedBar.value > targetRatio)
        {
            current -= Time.deltaTime * speed;
            _delayedBar.value = Mathf.Max(current, targetRatio);
            yield return null;
        }
        _delayedBar.value = targetRatio;
    }
    public void OnBattle()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
}