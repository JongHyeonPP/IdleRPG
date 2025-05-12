using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Services.RemoteConfig;
using System.Data;
public class ReinForceManager: MonoBehaviour
{
    private Dictionary<StatusType, string> reinforcePriceGold;
    private Dictionary<StatusType, string> reinforceValueGold;
    private Dictionary<StatusType, string> reinforceValueStatus;

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
    public int GetGoldStatus(int level, StatusType statusType)
    {
        //스탯마다 밸류가 달라야함.
        switch (statusType)
        {
            case StatusType.MaxHp:
                return 100;
            case StatusType.Power:
                if (level <= 1000)
                {
                    return level + 10;
                }
                else if (level <= 5000)
                {
                    return 1000 + (level - 1000) * 2 + 10;
                }
                else
                {
                    return 1000 + (5000 - 1000) * 2 + (level - 5000) * 3 + 10;
                }
            case StatusType.HpRecover:
                return 0;
            case StatusType.Critical:
                return 0;
            case StatusType.CriticalDamage:
                return 0;
        }
        return 0;
    }
    public int GetStatPointStatus(int level, StatusType statusType)
    {
        //스탯마다 밸류가 달라야함.
        switch (statusType)
        {
            case StatusType.MaxHp:
                return 0;
            case StatusType.Power:
                if (level <= 1000)
                {
                    return level;
                }
                else if (level <= 5000)
                {
                    return 1000 + (level - 1000) * 2;
                }
                else
                {
                    return 1000 + (5000 - 1000) * 2 + (level - 5000) * 3;
                }
            case StatusType.HpRecover:
                return 0;
            case StatusType.Critical:
                return 0;
            case StatusType.CriticalDamage:
                return 0;
            case StatusType.GoldAscend:
                return 0;
        }
        return 0;
    }
    public string GetGoldStatRiseText(int currentValue, int nextValue, StatusType stat)
    {
        //텍스트를 얻는 기능 수행
        switch (stat)
        {
            case StatusType.Power:
            case StatusType.MaxHp:
            case StatusType.HpRecover:
                return $"{currentValue} -> {nextValue}";
            case StatusType.CriticalDamage:
                return $"{currentValue * 100f}% -> {nextValue * 100f}%";
            case StatusType.Critical:
                return $"{currentValue * 100f:F1}% -> {nextValue * 100f:F1}%";
            default:
                return "N/A";
        }
    }
    public string GetStatPointStatRiseText(int currentValue, int nextValue, StatusType stat)
    {
        //텍스트를 얻는 기능 수행
        switch (stat)
        {
            case StatusType.Power:
                return $"공격력 +{currentValue} -> +{nextValue}";
            case StatusType.MaxHp:
                return $"체력 +{currentValue} -> +{nextValue}";
            case StatusType.HpRecover:
                return $"체력 회복량 +{currentValue} -> +{nextValue}";
            case StatusType.CriticalDamage:
                return $"치명타 공격력 +{currentValue * 100f}% -> +{nextValue * 100f}%";
            case StatusType.GoldAscend:
                return $"골드 획득량 +{currentValue * 100f:F1}% -> +{nextValue * 100f:F1}%";
            default:
                return "N/A";
        }
    }

    public int GetReinforcePriceGold(StatusType type, int level)
    => (int)Math.Round(GetValueFromFormulaDict(reinforcePriceGold, type, level, "ReinforcePriceGold"));
    public float GetReinforceValueGold(StatusType type, int level)
        => GetValueFromFormulaDict(reinforceValueGold, type, level, "ReinforceValueGold");

    public float GetReinforceValueStatus(StatusType type, int level)
        => GetValueFromFormulaDict(reinforceValueStatus, type, level, "ReinforceValueStatus");

    private void LoadFormulas()
    {
        string reinforcePriceGoldStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_PRICE_GOLD", "None");
        string reinforceValueGoldStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_GOLD", "None");
        string reinforceValueStatusStr = RemoteConfigService.Instance.appConfig.GetJson("REINFORCE_VALUE_STATUS", "None");
        reinforcePriceGold = UtilityManager.GetParsedFormularDict<StatusType>(reinforcePriceGoldStr);
        reinforceValueGold = UtilityManager.GetParsedFormularDict<StatusType>(reinforceValueGoldStr);
        reinforceValueStatus = UtilityManager.GetParsedFormularDict<StatusType>(reinforceValueStatusStr);
    }
    private float GetValueFromFormulaDict(Dictionary<StatusType, string> dict, StatusType type, int level, string context)
    {
        if (dict.TryGetValue(type, out var formula))
        {
            string replaced = formula.Replace("{level}", level.ToString());
            return Evaluate(replaced);
        }
        Debug.LogWarning($"[{context}] 공식 없음: {type}");
        return 0f;
    }



    private float Evaluate(string expression)
    {
        try
        {
            var table = new DataTable();
            var result = table.Compute(expression, null);
            return Convert.ToSingle(result);
        }
        catch (Exception e)
        {
            Debug.LogError($"수식 계산 오류: {expression}, 예외: {e.Message}");
            return 0f;
        }
    }
    public StatusType testStatusType;
    public int testValue;
    [ContextMenu("Test")]
    public void Test()
    {
       Debug.Log("Power : " + GetReinforceValueStatus(testStatusType, testValue));
    }
}
