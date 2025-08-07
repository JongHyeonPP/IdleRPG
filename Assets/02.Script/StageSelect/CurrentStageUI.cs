using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentStageUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    private Label _stageNumLabel;
    private Label _stageNameLabel;
    [SerializeField] StageSelectUI stageSelectUI;
    private GameData _gameData;
    void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        _stageNumLabel = root.Q<Label>("StageNumLabel");
        _stageNameLabel = root.Q<Label>("StageNameLabel");
        VisualElement stageSelectEnter = root.Q<VisualElement>("StageSelectEnter");
        stageSelectEnter.RegisterCallback<ClickEvent>(OnClickUI);
        VisualElement bossEnter = root.Q<VisualElement>("BossEnter");
        bossEnter.RegisterCallback<ClickEvent>(OnClickBossEnter);
        BattleBroker.OnStageChange += OnStageChange;
    }
    private void OnClickBossEnter(ClickEvent evt)
    {
        root.style.display = DisplayStyle.None;
        UIBroker.ChangeMenu(0);
        BattleBroker.SwitchToBoss();
    }

    private void OnClickUI(ClickEvent evt)
    {
        Debug.Log("Click CurrentStage Ui");
        stageSelectUI.ToggleUi(true);
    }

    private void OnStageChange()
    {
        StageInfo info = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        _stageNameLabel.text = info.stageName;
        _stageNumLabel.text = $"Stage {info.stageNum}";
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
        root.style.display = DisplayStyle.None;
    }
}
