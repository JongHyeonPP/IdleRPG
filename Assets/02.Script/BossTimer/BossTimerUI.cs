using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BossTimerUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    private ProgressBar _timerBar;
    private ProgressBar _hpBar;
    private readonly float _entireTime = 20f;
    private float _currentTime = 0f;
    private Coroutine _timerCoroutine;
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        
        BattleBroker.OnBossHpChanged += OnBossAttack;

        BattleBroker.OnBossClear += StopTimer;
        PlayerBroker.OnPlayerDead += StopTimer;


        _timerBar = root.Q<VisualElement>("TimerBar").Q<ProgressBar>("ProgressBar");
        _hpBar = root.Q<VisualElement>("HpBar").Q<ProgressBar>("ProgressBar");
        root.Q<Button>("RunButton").RegisterCallback<ClickEvent>(evt => 
        {
            StopTimer();
            BattleBroker.SwitchToBattle();
        });
    }


    private IEnumerator TimerLoop()
    {
        _currentTime = _entireTime; // 시작 시간을 전체 시간으로 설정
        _timerBar.value = 1f;   // ProgressBar를 100으로 초기화

        while (_currentTime > 0f)
        {
            _currentTime -= Time.deltaTime;

            // ProgressBar 값 계산 (100에서 0으로 감소)
            _timerBar.value = _currentTime / _entireTime;

            yield return null; // 다음 프레임까지 대기
        }

        _timerBar.value = 0f; // 타이머가 종료되었을 때 ProgressBar를 0으로 설정

        BattleBroker.OnBossTimeLimit();
    }
    private void StopTimer()
    {
        root.style.display = DisplayStyle.None;
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
    }
    private void OnBossAttack(float ratio)
    {
        _hpBar.value = ratio;
    }
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
        StopTimer();
        root.style.display = DisplayStyle.Flex;

        _currentTime = _entireTime; // 타이머 초기화
        _hpBar.value = 1f;
        // 타이머 시작
        _timerCoroutine = StartCoroutine(TimerLoop());

    }
}
