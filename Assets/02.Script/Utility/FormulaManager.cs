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
        //���ȸ��� ����� �޶����.
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
        //���ȸ��� ����� �޶����.
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
        //�ؽ�Ʈ�� ��� ��� ����
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
        //�ؽ�Ʈ�� ��� ��� ����
        switch (stat)
        {
            case StatusType.Power:
                return $"���ݷ� +{currentValue} -> +{nextValue}";
            case StatusType.MaxHp:
                return $"ü�� +{currentValue} -> +{nextValue}";
            case StatusType.HpRecover:
                return $"ü�� ȸ���� +{currentValue} -> +{nextValue}";
            case StatusType.CriticalDamage:
                return $"ġ��Ÿ ���ݷ� +{currentValue * 100f}% -> +{nextValue * 100f}%";
            case StatusType.GoldAscend:
                return $"��� ȹ�淮 +{currentValue * 100f:F1}% -> +{nextValue * 100f:F1}%";
            default:
                return "N/A";
        }
    }
}
