using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerSavePanel : MonoBehaviour
{
    public VisualElement root { private set; get; }
    private GameData _gameData;
    private Label _timeLabel;
    private Label _stageNameLabel;
    private Label _stageNumLabel;
    private Label _batteryLabel;
    private Label _inChargeLabel;
    private VisualElement _unlockButton;
    private VisualElement _fillSize;

    private Coroutine _timeCoroutine;
    private Coroutine _fillCoroutine;
    private Coroutine _idleCoroutine;

    private float _fillDuration = 1f;
    private bool _isHolding = false;

    [SerializeField]private float _idleThreshold = 300f; // 5분 (300초)
    private float _lastInputTime;
    private bool _isPowerSaveActive = false;

    void Start()
    {
        InitElements();
        RegisterButtonEvents();

        _lastInputTime = Time.time;

        // SettingManager의 현재 상태를 바로 반영
        bool initialPowerSave = SettingManager.instance != null && SettingManager.instance.isPowerSaving;
        ActivePowerSaveCount(initialPowerSave);

        // 이후 상태 변경은 UIBroker 이벤트로 감지
        UIBroker.ActivePowerSaveCount += ActivePowerSaveCount;
    }

    private void ActivePowerSaveCount(bool isActive)
    {
        if (isActive)
        {
            // 절전모드 활성화 → 타이머 초기화 후 유휴 감지 코루틴 시작
            _lastInputTime = Time.time;

            if (_idleCoroutine != null)
                StopCoroutine(_idleCoroutine);

            _idleCoroutine = StartCoroutine(IdleCheckCoroutine());
        }
        else
        {
            // 절전모드 비활성화 → 모든 절전 관련 상태 중단
            if (_idleCoroutine != null)
            {
                StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
            }

            if (_isPowerSaveActive)
            {
                _isPowerSaveActive = false;
                InactivePowerSavePanel();
            }
        }
    }

    private void InitElements()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;

        _timeLabel = root.Q<Label>("TimeLabel");
        _stageNameLabel = root.Q<Label>("StageNameLabel");
        _stageNumLabel = root.Q<Label>("StageNumLabel");
        _batteryLabel = root.Q<Label>("BatteryLabel");
        _inChargeLabel = root.Q<Label>("InChargeLabel");
        _unlockButton = root.Q<VisualElement>("UnlockButton");
        _fillSize = root.Q<VisualElement>("FillSize");

        if (_fillSize != null)
            _fillSize.style.width = Length.Percent(0);
    }

    private void RegisterButtonEvents()
    {
        if (_unlockButton == null) return;

        _unlockButton.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button == 0) StartHold();
        });

        _unlockButton.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.button == 0) StopHold();
        });

        _unlockButton.RegisterCallback<PointerLeaveEvent>(evt => StopHold());
    }

    private IEnumerator IdleCheckCoroutine()
    {
        while (SettingManager.instance.isPowerSaving)
        {
            DetectInput();

            if (!_isPowerSaveActive && Time.time - _lastInputTime >= _idleThreshold)
            {
                _isPowerSaveActive = true;
                ActivePowerSavePanel();
            }

            yield return new WaitForSeconds(1f);
        }

        _idleCoroutine = null;
    }

    private void DetectInput()
    {
        if (_isPowerSaveActive)
            return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            ResetIdleTimer();

        if (Input.touchCount > 0)
            ResetIdleTimer();
    }

    private void ResetIdleTimer()
    {
        _lastInputTime = Time.time;

        if (_isPowerSaveActive)
        {
            _isPowerSaveActive = false;
            InactivePowerSavePanel();
        }
    }

    public void ActivePowerSavePanel()
    {
        root.style.display = DisplayStyle.Flex;
        StageInfo stage = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        _stageNameLabel.text = stage.stageName;
        _stageNumLabel.text = $"Stage {_gameData.currentStageNum}";

        if (_timeCoroutine != null)
            StopCoroutine(_timeCoroutine);
        _timeCoroutine = StartCoroutine(UpdateCoroutine());
    }

    public void InactivePowerSavePanel()
    {
        root.style.display = DisplayStyle.None;

        if (_timeCoroutine != null)
        {
            StopCoroutine(_timeCoroutine);
            _timeCoroutine = null;
        }

        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }

        if (_fillSize != null)
            _fillSize.style.width = Length.Percent(0);

        if (_inChargeLabel != null)
            _inChargeLabel.text = "";
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            _timeLabel.text = now.ToString("tt h:mm", new CultureInfo("ko-KR"));

            float battery = SystemInfo.batteryLevel;
            _batteryLabel.text = battery >= 0f ? $"{Mathf.RoundToInt(battery * 100f)}%" : "N/A";

            BatteryStatus status = SystemInfo.batteryStatus;
            _inChargeLabel.text = status == BatteryStatus.Charging ? "충전 중..." : "";

            yield return new WaitForSeconds(1f);
        }
    }

    private void StartHold()
    {
        if (_isHolding) return;
        _isHolding = true;

        if (_fillCoroutine != null)
            StopCoroutine(_fillCoroutine);
        _fillCoroutine = StartCoroutine(FillProgressCoroutine());
    }

    private void StopHold()
    {
        _isHolding = false;

        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }

        if (_fillSize != null)
            _fillSize.style.width = Length.Percent(0);
    }

    private IEnumerator FillProgressCoroutine()
    {
        float elapsed = 0f;

        while (_isHolding)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / _fillDuration) * 100f;
            _fillSize.style.width = Length.Percent(percent);

            if (percent >= 100f)
            {
                InactivePowerSavePanel();
                yield break;
            }

            yield return null;
        }
    }
}
