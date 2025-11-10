using EnumCollection;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public partial class StatUI
{
    private void InitEnhancePanel()
    {
        foreach (var stat in _statsByGold)
            InitEnhanceElement(stat);
    }

    private void InitEnhanceElement(StatusType stat)
    {
        var element = _categoriPanels[0].Q<VisualElement>($"{stat}Element");
        _goldStatDict[stat] = element;

        var info = _statInfoDict[stat];
        element.Q<Label>("StatName").text = info.name;
        element.Q<VisualElement>("StatIcon").style.backgroundImage = new(info.icon);
        element.Q<VisualElement>("EventVe").RegisterCallback<PointerDownEvent>(_ => OnPointerDown(stat, true));

        if (!_gameData.statLevel_Gold.ContainsKey(stat))
            _gameData.statLevel_Gold[stat] = 0;

        UpdateGoldStatText(stat, _gameData.statLevel_Gold[stat]);
    }

    private void IncreaseGoldStat(StatusType stat)
    {
        int level = _gameData.statLevel_Gold[stat] + 1;
        int cost = ReinForceManager.instance.GetReinforcePriceGold(stat, level);
        if (_gameData.gold < cost) return;

        _gameData.gold -= cost;
        _gameData.statLevel_Gold[stat]++;
        _currentValue++;

        PlayerBroker.OnGoldStatusLevelSet(stat, _gameData.statLevel_Gold[stat]);
        PlayerBroker.OnGoldSet?.Invoke();
    }

    private void UpdateGoldStatText(StatusType stat, int level)
    {
        var element = _goldStatDict[stat];
        element.Q<Label>("StatLevel").text = $"Lv.{level}";

        float current = ReinForceManager.instance.GetReinforceValueGold(stat, level);
        float next = ReinForceManager.instance.GetReinforceValueGold(stat, level + 1);
        element.Q<Label>("StatRise").text = SetGoldStatRiseText(current, next, stat);

        int price = ReinForceManager.instance.GetReinforcePriceGold(stat, level) + 1;
        element.Q<Label>("PriceLabel").text = $"{price}";
    }

    public string SetGoldStatRiseText(float currentValue, float nextValue, StatusType stat)
    {
        switch (stat)
        {
            case StatusType.Power:
            case StatusType.MaxHp:
            case StatusType.HpRecover:
                return $"{currentValue:F0} -> {nextValue:F0}";
            case StatusType.CriticalDamage:
                return $"{currentValue * 100f:F0}% -> {nextValue * 100f:F0}%";
            case StatusType.Critical:
                return $"{currentValue * 100f:F1}% -> {nextValue * 100f:F1}%";
            default:
                return "N/A";
        }
    }
}
