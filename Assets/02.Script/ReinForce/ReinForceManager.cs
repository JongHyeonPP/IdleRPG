using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Services.RemoteConfig;



public class ReinforceManager : MonoBehaviour
{
    private Dictionary<StatusType, ReinforceRule> reinforcePriceGold;
    private Dictionary<StatusType, ReinforceRule> reinforceValueGold;
    private Dictionary<StatusType, ReinforceRule> reinforceValueStatus;

    private GameData _gameData;
    public static ReinforceManager instance;

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

    // 가격 증가 계산
    public int GetReinforcePriceGold(StatusType type, int level)
    {
        if (!reinforcePriceGold.TryGetValue(type, out var rule))
        {
            Debug.LogWarning("ReinforcePriceGold 데이터 없음 " + type);
            return 0;
        }

        float total = rule.startValue;
        float inc = rule.baseInc;

        for (int i = 1; i <= level; i++)
        {
            total += inc;

            if (i % rule.step == 0)
            {
                inc += rule.stepInc;
            }
        }

        return Mathf.RoundToInt(total);
    }

    // 골드 강화값 증가 계산
    public float GetReinforceValueGold(StatusType type, int level)
    {
        if (!reinforceValueGold.TryGetValue(type, out var rule))
        {
            Debug.LogWarning("ReinforceValueGold 데이터 없음 " + type);
            return 0;
        }

        float total = rule.startValue;
        float inc = rule.baseInc;

        for (int i = 1; i <= level; i++)
        {
            total += inc;

            if (i % rule.step == 0)
            {
                inc += rule.stepInc;
            }
        }

        return total;
    }

    // 스탯 강화값 증가 계산
    public float GetReinforceValueStatus(StatusType type, int level)
    {
        if (!reinforceValueStatus.TryGetValue(type, out var rule))
        {
            Debug.LogWarning("ReinforceValueStatus 데이터 없음 " + type);
            return 0;
        }

        float total = rule.startValue;
        float inc = rule.baseInc;

        for (int i = 1; i <= level; i++)
        {
            total += inc;

            if (i % rule.step == 0)
            {
                inc += rule.stepInc;
            }
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

        Debug.Log("[" + testStatusType + "] Lv." + testValue +
                  " 가격 " + price +
                  " 골드강화수치 " + valueGold +
                  " 스탯강화수치 " + valueStatus);
    }
}
