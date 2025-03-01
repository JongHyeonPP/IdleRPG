using EnumCollection;
using UnityEngine;

public static class FormulaManager
{
    public static int GetGoldRequired(int level)
    {
        return 10;
    }
    public static int GetGoldStatus(int level, StatusType statusType)
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
    public static int GetStatPointStatus(int level, StatusType statusType)
    {
        //스탯마다 밸류가 달라야함.
        switch (statusType)
        {
            case StatusType.MaxHp:
                return 0;
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
            case StatusType.GoldAscend:
                return 0;
        }
        return 0;
    }
    public static string GetGoldStatRiseText(int currentValue, int nextValue, StatusType stat)
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
    public static string GetStatPointStatRiseText(int currentValue, int nextValue, StatusType stat)
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
}
