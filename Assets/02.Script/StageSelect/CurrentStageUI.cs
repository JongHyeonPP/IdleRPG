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
        
    }
    private void Start()
    {
        _stageNumLabel = root.Q<Label>("StageNumLabel");
        _stageNameLabel = root.Q<Label>("StageNameLabel");
        BattleBroker.OnMainStageChange += OnMainStageChange;
        VisualElement rootChild = root.Q<VisualElement>("CurrentStageUI");
        rootChild.RegisterCallback<ClickEvent>(OnClickUI);
    }
    private void OnClickUI(ClickEvent evt)
    {
        stageSelectUI.ToggleUi(true);
    }

    private void OnMainStageChange(int stageNum)
    {
        StageInfo info = StageManager.instance.GetStageInfo(stageNum);
        _stageNameLabel.text = info.stageName;
        _stageNumLabel.text = $"Stage {info.stageNum}";
    }
}
