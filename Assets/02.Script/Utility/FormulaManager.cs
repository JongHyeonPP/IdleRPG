using EnumCollection;
using UnityEngine;

public static class FormulaManager
{
    public static int GetGoldRequired(int level)
    {

        if (level <= 1000)
        {
            return Mathf.CeilToInt(
                0.000005f * Mathf.Pow(level, 3)  
                + 0.01f * Mathf.Pow(level, 2)   
                + 5f * level                  
                + 100f                          
            );
        }
        else if (level <= 5000)
        {
            return Mathf.CeilToInt(
                0.00000001f * Mathf.Pow(level, 3.5f) 
                + 100f * level                        
            );
        }
        else if (level <= 10000)
        {
            return Mathf.CeilToInt(
                0.0000000001f * Mathf.Pow(level, 4f)
                + 2000f * level
            );
        }
        else
        {
            return Mathf.CeilToInt(
                10000f * level - 90000000f 
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
