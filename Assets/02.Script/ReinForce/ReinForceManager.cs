using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Services.RemoteConfig;
using System.Data;

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
    }

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        LoadFormulas();
    }

    // --------------------------------------------------------
    // 스탯 계산
    // --------------------------------------------------------
    public float GetGoldStatus(int level, StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.MaxHp:
                {
                    int baseInc = 4;
                    int step = 100;
                    int total = 100;
                    for (int i = 1; i <= level; i++)
                    {
                        int inc = baseInc + (i / step);
                        total += inc;
                    }
                    return total;
                }

            case StatusType.Power:
                {
                    int baseInc = 1;
                    int step = 100;
                    int total = 10;
                    for (int i = 1; i <= level; i++)
                    {
                        int inc = baseInc + (i / step);
                        total += inc;
                    }
                    return total;
                }

            case StatusType.HpRecover:
                {
                    float baseInc = 0.2f;
                    int step = 200;
                    float total = 1f;
                    for (int i = 1; i <= level; i++)
                    {
                        float inc = baseInc + ((i / step) * 0.1f);
                        total += inc;
                    }
                    return total;
                }

            case StatusType.Critical:
                {
                    float baseInc = 0.001f;
                    float total = 0f;
                    for (int i = 1; i <= level; i++)
                    {
                        total += baseInc;
                    }
                    return total;
                }

            case StatusType.CriticalDamage:
                {
                    float baseInc = 0.01f;
                    int step = 100;
                    float total = 1.2f;
                    for (int i = 1; i <= level; i++)
                    {
                        float inc = baseInc + ((i / step) * 0.01f);
                        total += inc;
                    }
                    return total;
                }
        }

        return 0f;
    }

    // --------------------------------------------------------
    // 스탯 포인트 기반 성장 계산
    // --------------------------------------------------------
    public float GetStatPointStatus(int level, StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.MaxHp:
                {
                    int baseInc = 5;
                    int step = 200;
                    int stepInc = 2;
                    int total = 0;
                    for (int i = 1; i <= level; i++)
                    {
                        int inc = baseInc + (i / step) * stepInc;
                        total += inc;
                    }
                    return total;
                }

            case StatusType.Power:
                {
                    int baseInc = 1;
                    int step = 100;
                    int stepInc = 1;
                    int total = 0;
                    for (int i = 1; i <= level; i++)
                    {
                        int inc = baseInc + (i / step) * stepInc;
                        total += inc;
                    }
                    return total;
                }

            case StatusType.HpRecover:
                {
                    int baseInc = 1;
                    int step = 300;
                    int stepInc = 1;
                    int total = 0;
                    for (int i = 1; i <= level; i++)
                    {
                        int inc = baseInc + (i / step) * stepInc;
                        total += inc;
                    }
                    return total;
                }

            case StatusType.Critical:
                {
                    int baseInc = 1;
                    return baseInc * level;
                }

            case StatusType.CriticalDamage:
                {
                    float baseInc = 0.01f;
                    int step = 100;
                    float total = 0f;
                    for (int i = 1; i <= level; i++)
                    {
                        float inc = baseInc + ((i / step) * 0.01f);
                        total += inc;
                    }
                    return total;
                }

            case StatusType.GoldAscend:
                {
                    float baseInc = 0.01f;
                    float total = 0f;
                    for (int i = 1; i <= level; i++)
                    {
                        total += baseInc;
                    }
                    return total;
                }
        }

        return 0;
    }

    // --------------------------------------------------------
    // 강화 가격 / 강화 수치 계산
    // --------------------------------------------------------
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

    // --------------------------------------------------------
    // ✅ Remote Config에서 JSON 로드
    // --------------------------------------------------------
    private void LoadFormulas()
    {
        string priceStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_PRICE_GOLD", "None");
        string valueGoldStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_GOLD", "None");
        string valueStatusStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_STATUS", "None");

        reinforcePriceGold = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(priceStr);
        reinforceValueGold = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(valueGoldStr);
        reinforceValueStatus = JsonConvert.DeserializeObject<Dictionary<StatusType, ReinforceRule>>(valueStatusStr);
    }

    // --------------------------------------------------------
    // ✅ 테스트
    // --------------------------------------------------------
    public StatusType testStatusType;
    public int testValue;

    [ContextMenu("Test")]
    public void Test()
    {
        float price = GetReinforcePriceGold(testStatusType, testValue);
        float value = GetReinforceValueStatus(testStatusType, testValue);
        Debug.Log($"[{testStatusType}] Lv.{testValue} → 가격: {price}, 수치: {value}");
    }
}
