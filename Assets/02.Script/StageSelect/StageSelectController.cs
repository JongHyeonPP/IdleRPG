using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Collections.Generic;
using EnumCollection;
using Newtonsoft.Json;
using Unity.Services.RemoteConfig;

public class StageSelectController : MonoBehaviour, LVItemController
{
    private GameData _gameData;
    public FlexibleListView draggableLV { get; set; }
    private Dictionary<string, float> _dropProbDict;
    private bool _hasScrolledOnce = false; // 최초 한 번만 스크롤하기 위한 플래그

    private class ItemCache
    {
        public Button infoButton;
        public Button moveButton;
        public Label stageLabel;
        public Label titleLabel;
        public Label infoLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;

        public StageInfo stageInfo;
        public int stageNum;
        public bool isOpen;
        public int index;
    }

    private void Awake()
    {
        string probJson = RemoteConfigService.Instance.appConfig.GetJson("DROP_PROBABILITY", "None");
        if (!string.IsNullOrEmpty(probJson) && probJson != "None")
            _dropProbDict = JsonConvert.DeserializeObject<Dictionary<string, float>>(probJson);
        else
            _dropProbDict = new() { { "Gold", 0f }, { "Exp", 0f }, { "Weapon", 0f }, { "Fragment", 0f } };
    }

    public void BindItem(VisualElement element, int index)
    {
        if (_gameData == null)
            _gameData = StartBroker.GetGameData();

        if (draggableLV == null || draggableLV.items == null || index < 0 || index >= draggableLV.items.Count)
            return;

        if (draggableLV.items[index] is not StageInfo stageInfo)
            return;

        int stageNum = stageInfo.stageNum;

        var cache = new ItemCache
        {
            infoButton = element.Q<Button>("InfoButton"),
            moveButton = element.Q<Button>("MoveButton"),
            stageLabel = element.Q<Label>("StageLabel"),
            titleLabel = element.Q<Label>("TitleLabel"),
            infoLabel = element.Q<Label>("InfoLabel"),
            lockGroup = element.Q<VisualElement>("LockGroup"),
            selectBorder = element.Q<VisualElement>("SelectBorder"),
            stageInfo = stageInfo,
            stageNum = stageNum,
            index = index
        };
        element.userData = cache;

        cache.titleLabel.text = stageInfo.stageName;
        BindOpenState(cache, stageInfo);

        cache.moveButton?.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
        if (cache.moveButton != null)
        {
            cache.moveButton.userData = stageNum;
            cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        }

        cache.infoButton?.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
        if (cache.infoButton != null)
        {
            cache.infoButton.userData = stageNum;
            cache.infoButton.RegisterCallback<ClickEvent>(OnInfoButtonClick);
        }

        element.UnregisterCallback<ClickEvent>(OnElementClick);

        bool isCurrentStage = _gameData.currentStageNum == stageNum;
        SetSelected(cache.selectBorder, isCurrentStage);

        // 현재 스테이지 아이템으로 최초 한 번만 자동 스크롤
        if (isCurrentStage && !_hasScrolledOnce)
        {
            _hasScrolledOnce = true;
            element.schedule.Execute(() =>
            {
                element.schedule.Execute(() =>
                {
                    var scrollView = draggableLV.listView.Q<ScrollView>();
                    if (scrollView == null) return;

                    float itemHeight = element.layout.height > 0 ? element.layout.height : 200f;
                    float visibleHeight = scrollView.layout.height;
                    float targetPos = Mathf.Max(0, itemHeight * index - visibleHeight / 2f);

                    scrollView.scrollOffset = new Vector2(0, targetPos);
                    Debug.Log($"[StageSelectController] Auto-scroll applied once (index={index}, targetPos={targetPos})");
                }).StartingIn(0);
            }).StartingIn(0);
        }
    }

    private void OnElementClick(ClickEvent evt) => evt.StopImmediatePropagation();

    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        int stageNum = stageInfo.stageNum;
        bool isOpen = _gameData.maxStageNum >= stageNum;
        cache.isOpen = isOpen;

        if (!isOpen)
        {
            SetVisible(cache.stageLabel, false);
            SetVisible(cache.infoButton, false);
            SetVisible(cache.infoLabel, false);
            SetVisible(cache.moveButton, false);
            SetVisible(cache.lockGroup, true);
            return;
        }

        SetVisible(cache.stageLabel, true);
        SetVisible(cache.infoButton, true);
        SetVisible(cache.infoLabel, true);
        SetVisible(cache.moveButton, true);
        SetVisible(cache.lockGroup, false);

        cache.stageLabel.text = $"STAGE {stageNum}";

        var cm = CurrencyManager.instance;
        var sm = StageInfoManager.instance;

        (float goldBonus, float expBonus) = sm.GetBonusInfo(stageNum);
        (Rarity rarity, int count) fragVal = cm.GetBaseFragmentValue(stageNum);
        string weaponVal = cm.GetWeaponValue(stageNum);

        List<string> infoList = new();

        if (goldBonus > 0f)
            infoList.Add($"골드 보너스 +{goldBonus * 100f:F0}%");
        if (expBonus > 0f)
            infoList.Add($"경험치 보너스 +{expBonus * 100f:F0}%");

        float totalWeight = 0f;
        foreach (var kvp in _dropProbDict)
        {
            if (kvp.Value > 0f)
                totalWeight += kvp.Value;
        }

        if (!string.IsNullOrEmpty(weaponVal))
            infoList.Add($"무기 드랍 확률 {GetDropPercent("Weapon", totalWeight):F1}%");

        if (fragVal.count > 0)
            infoList.Add($"{fragVal.rarity} 조각 드랍 확률 {GetDropPercent("Fragment", totalWeight):F1}%");

        cache.infoLabel.text = infoList.Count > 0
            ? string.Join("\n", infoList)
            : "보상 없음";
    }

    private float GetDropPercent(string key, float totalWeight)
    {
        if (_dropProbDict.TryGetValue(key, out float value) && totalWeight > 0f)
            return (value / totalWeight) * 100f;
        return 0f;
    }

    private void SetSelected(VisualElement selectBorder, bool selected)
    {
        if (selectBorder == null) return;
        selectBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnMoveButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
        {
            _gameData.currentStageNum = stageNum;
            BattleBroker.OnStageChange();
            NetworkBroker.SaveServerData();
            UIBroker.InactiveCurrentUI?.Invoke();
        }
    }

    private void OnInfoButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
            BattleBroker.ActiveStageInfoUI(stageNum);
    }
}
