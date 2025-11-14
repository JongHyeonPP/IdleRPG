using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Services.RemoteConfig;

[Serializable]
public class ReinforceRule
{
    public float baseInc;
    public int step;
    public float stepInc;
    public float startValue;
}

public class ReinForceManager : MonoBehaviour
{
    private Dictionary<StatusType, ReinforceRule> reinforcePriceGold;
    private Dictionary<StatusType, ReinforceRule> reinforceValueGold;
    private Dictionary<StatusType, ReinforceRule> reinforceValueStatus;

    private GameData _gameData;
    public static ReinForceManager instance;

    private void Awake()
    {
        instance = this;
        _gameData = StartBroker.GetGameData();
        LoadFormulas();
    }

    private void LoadFormulas()
    {
        string priceStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_PRICE_GOLD", "None");
        string valueGoldStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_GOLD", "None");
        string valueStatusStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_STATUS", "None");

        reinforcePriceGold = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(priceStr);
        reinforceValueGold = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(valueGoldStr);
        reinforceValueStatus = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(valueStatusStr);
    }

    public int GetReinforcePriceGold(StatusType type, int level)
    {
        if (!reinforcePriceGold.TryGetValue(type, out var rule))
        {
            Debug.LogWarning($"[ReinforcePriceGold] {type} 데이터 없음");
            return 0;
        }

        float total = rule.startValue;
        for (int i = 1; i <= level; i++)
        {
            float inc = rule.baseInc + (i / (float)rule.step) * rule.stepInc;
            total += inc;
        }

        return Mathf.RoundToInt(total);
    }

    public float GetReinforceValueGold(StatusType type, int level)
    {
        if (!reinforceValueGold.TryGetValue(type, out var rule))
        {
            Debug.LogWarning($"[ReinforceValueGold] {type} 데이터 없음");
            return 0;
        }

        float total = rule.startValue;
        for (int i = 1; i <= level; i++)
        {
            float inc = rule.baseInc + (i / (float)rule.step) * rule.stepInc;
            total += inc;
        }

        return total;
    }

    public float GetReinforceValueStatus(StatusType type, int level)
    {
        if (!reinforceValueStatus.TryGetValue(type, out var rule))
        {
            Debug.LogWarning($"[ReinforceValueStatus] {type} 데이터 없음");
            return 0;
        }

        float total = rule.startValue;
        for (int i = 1; i <= level; i++)
        {
            float inc = rule.baseInc + (i / (float)rule.step) * rule.stepInc;
            total += inc;
        }

        return total;
    }

    public StatusType testStatusType;
    public int testValue;

    [ContextMenu("Test")]
    public void Test()
    {
        float price = GetReinforcePriceGold(testStatusType, testValue);
        float valueGold = GetReinforceValueGold(testStatusType, testValue);
        float valueStatus = GetReinforceValueStatus(testStatusType, testValue);

        Debug.Log($"[{testStatusType}] Lv.{testValue} → 가격: {price}, 골드강화수치: {valueGold}, 스탯강화수치: {valueStatus}");
    }
}
