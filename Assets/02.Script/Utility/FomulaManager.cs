using EnumCollection;
using UnityEngine;

public static class FomulaManager
{
    private const float A = 0.0001f;
    private const float B = 2.5f;
    private const float C = 0.01f;
    private const float D = 1f;

    public static int GetGoldRequired(int level)
    {

        if (level <= 1000)
        {
            return Mathf.CeilToInt(
                0.0003f * Mathf.Pow(level, 3)
                - 0.15f * Mathf.Pow(level, 2)
                + 14f * level + 1f
            );
        }
        else
        {
            return Mathf.CeilToInt(
                0.0000000008f * Mathf.Pow(level, 4.2f)
                + 1000f * level
            );
        }
    }
    public static int GetBasicStatRise(int level,StatusType stat)
    {
        if(stat==StatusType.Power||stat==StatusType.MaxHp||stat==StatusType.HpRecover)
        {
            if (level <= 1000)
            {
                return 1;
            }
            else if (level > 1000 && level <= 5000)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        else if (stat == StatusType.Critical)
        {
            return 10;
        }
        else
        {
            return 1;
        }
        
    }
    public static string GetStatRiseText(int level, StatusType stat)
    {
        int nextLevel = level + 1;
        int statRise = GetBasicStatRise(level, stat);
        if (stat == StatusType.Power || stat == StatusType.MaxHp || stat == StatusType.HpRecover)
        {
            return $"{level} -> {level+statRise}";
        }
        else if (stat == StatusType.CriticalDamage) 
        {
            return $"{level}% -> {nextLevel}%";
        }
        else if (stat == StatusType.Critical) 
        {
            float currentCritical = level * 0.1f; 
            float nextCritical = nextLevel * 0.1f;
            return $"{currentCritical:F1}% -> {nextCritical:F1}%";
        }
        else
        {
            return "N/A";
        }
    }
}
