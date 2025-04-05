using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BossTimerUI : MonoBehaviour
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

        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => StopTimer();
        BattleBroker.SwitchToBoss += StopTimer;
        BattleBroker.OnBossClear += StopTimer;
        PlayerBroker.OnPlayerDead += StopTimer;

        BattleBroker.SwitchToBoss += OnBossEnter;
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => OnBossEnter();

        _timerBar = root.Q<VisualElement>("TimerBar").Q<ProgressBar>("ProgressBar");
        _hpBar = root.Q<VisualElement>("HpBar").Q<ProgressBar>("ProgressBar");
        root.Q<Button>("RunButton").RegisterCallback<ClickEvent>(evt => 
        {
            StopTimer();
            BattleBroker.SwitchToBattle();
        });
    }


    private void OnBossEnter()
    {
        _currentTime = _entireTime; // Ÿ�̸� �ʱ�ȭ
        _hpBar.value = 1f;
        // Ÿ�̸� ����
        _timerCoroutine =  StartCoroutine(TimerLoop());
    }

    private IEnumerator TimerLoop()
    {
        _currentTime = _entireTime; // ���� �ð��� ��ü �ð����� ����
        _timerBar.value = 1f;   // ProgressBar�� 100���� �ʱ�ȭ

        while (_currentTime > 0f)
        {
            _currentTime -= Time.deltaTime;

            // ProgressBar �� ��� (100���� 0���� ����)
            _timerBar.value = _currentTime / _entireTime;

            yield return null; // ���� �����ӱ��� ���
        }

        _timerBar.value = 0f; // Ÿ�̸Ӱ� ����Ǿ��� �� ProgressBar�� 0���� ����

        BattleBroker.OnBossTimeLimit();
    }
    private void StopTimer()
    {
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
    }
    private void OnBossAttack(float ratio)
    {
        _hpBar.value = ratio;
    }
}
