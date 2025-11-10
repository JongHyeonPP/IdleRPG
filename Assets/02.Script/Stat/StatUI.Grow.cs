using EnumCollection;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public partial class StatUI
{
    private void InitGrowPanel()
    {
        _statPointLabel = _categoriPanels[1].Q<Label>("StatPointLabel");
        StatPointSet();

        foreach (var stat in _statsByStatPoint)
            InitGrowElement(stat);
    }

    private void InitGrowElement(StatusType stat)
    {
        var element = _categoriPanels[1].Q<VisualElement>($"{stat}Element");
        _statPointStatDict[stat] = element;

        var info = _statInfoDict[stat];
        element.Q<Label>("StatName").text = info.name;
        element.Q<VisualElement>("StatIcon").style.backgroundImage = new(info.icon);
        element.Q<VisualElement>("EventVe").RegisterCallback<PointerDownEvent>(_ => OnPointerDown(stat, false));

        if (!_gameData.statLevel_StatPoint.ContainsKey(stat))
            _gameData.statLevel_StatPoint[stat] = 0;

        UpdateStatPointStatText(stat, _gameData.statLevel_StatPoint[stat]);
    }

    private void IncreaseStatPointStat(StatusType stat)
    {
        if (_gameData.statPoint <= 0)
            return;

        _gameData.statPoint--;
        _gameData.statLevel_StatPoint[stat]++;
        PlayerBroker.OnStatPointStatusLevelSet(stat, _gameData.statLevel_StatPoint[stat]);
        PlayerBroker.OnStatPointSet?.Invoke();
    }

    private void UpdateStatPointStatText(StatusType stat, int level)
    {
        var element = _statPointStatDict[stat];
        element.Q<Label>("StatLevel").text = $"Lv.{level}";

        float current = ReinForceManager.instance.GetReinforceValueStatus(stat, level);
        float next = ReinForceManager.instance.GetReinforceValueStatus(stat, level + 1);
        element.Q<Label>("StatRise").text = GetStatPointStatRiseText(current, next, stat);
    }

    public string GetStatPointStatRiseText(float currentValue, float nextValue, StatusType stat)
    {
        switch (stat)
        {
            case StatusType.Power:
                return $"공격력 +{currentValue} -> +{nextValue}";
            case StatusType.MaxHp:
                return $"체력 +{currentValue} -> +{nextValue}";
            case StatusType.HpRecover:
                return $"체력 회복량 +{currentValue} -> +{nextValue}";
            case StatusType.CriticalDamage:
                return $"치명타 공격력 +{currentValue * 100f:F0}% -> +{nextValue * 100f:F0}%";
            case StatusType.GoldAscend:
                return $"골드 획득량 +{currentValue * 100f:F1}% -> +{nextValue * 100f:F1}%";
            default:
                return "N/A";
        }
    }

    private void StatPointSet()
    {
        _statPointLabel.text = $"STAT POINT : {_gameData.statPoint}";
    }

    // ==============================
    // 입력 처리 (PointerDown, Up 포함)
    // ==============================





}
