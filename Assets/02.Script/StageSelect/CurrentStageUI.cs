using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentStageUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label _stageNumLabel;
    private Label _stageNameLabel;
    [SerializeField] StageSelectUI stageSelectUI;
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _stageNumLabel = root.Q<Label>("StageNumLabel");
        _stageNameLabel = root.Q<Label>("StageNameLabel");
        VisualElement stageSelectEnter = root.Q<VisualElement>("StageSelectEnter");
        stageSelectEnter.RegisterCallback<ClickEvent>(OnClickUI);
        VisualElement bossEnter = root.Q<VisualElement>("BossEnter");
        bossEnter.RegisterCallback<ClickEvent>(OnClickBossEnter);
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.OnStageEnter += OnStageEnter;
    }
    private void OnClickBossEnter(ClickEvent evt)
    {
        root.style.display = DisplayStyle.None;
        BattleBroker.OnBossEnter();
    }

    private void OnClickUI(ClickEvent evt)
    {
        stageSelectUI.ToggleUi(true);
    }

    private void OnStageChange(int stageNum)
    {
        StageInfo info = StageManager.instance.GetStageInfo(stageNum);
        _stageNameLabel.text = info.stageName;
        _stageNumLabel.text = $"Stage {info.stageNum}";
    }
    private void OnStageEnter()
    {
        root.style.display = DisplayStyle.Flex;
    }
}
