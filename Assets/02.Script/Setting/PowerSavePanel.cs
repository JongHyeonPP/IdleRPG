using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerSavePanel : MonoBehaviour
{
    public VisualElement root { private set; get; }
    private GameData _gameData;
    private Label _timeLabel;
    private Label _stageNameLabel;
    private Label _stageNumLabel;
    private Coroutine _timeCoroutine;

    void Start()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;

        _timeLabel = root.Q<Label>("TimeLabel");
        _stageNameLabel = root.Q<Label>("StageNameLabel");
        _stageNumLabel = root.Q<Label>("StageNumLabel");
    }

    public void ActivePowerSavePanel()
    {
        root.style.display = DisplayStyle.Flex;
        StageInfo stage = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        _stageNameLabel.text = stage.stageName;
        _stageNumLabel.text = $"Stage {_gameData.currentStageNum}";

        // 시간 갱신 코루틴 시작
        if (_timeCoroutine != null)
            StopCoroutine(_timeCoroutine);

        _timeCoroutine = StartCoroutine(UpdateTimeCoroutine());
    }

    public void InactivePowerSavePanel()
    {
        root.style.display = DisplayStyle.None;

        // 코루틴 중단
        if (_timeCoroutine != null)
        {
            StopCoroutine(_timeCoroutine);
            _timeCoroutine = null;
        }
    }

    private IEnumerator UpdateTimeCoroutine()
    {
        while (true)
        {
            _timeLabel.text = System.DateTime.Now.ToString("h:mm tt");
            yield return new WaitForSeconds(1f);
        }
    }
}
