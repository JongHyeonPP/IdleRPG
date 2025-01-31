using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;
    [SerializeField] PriceInfo priceInfo;
    public const int MAXSKILLLEVEL = 10;
    public const int MAXWEAPONLEVEL = 20;
    private void Awake()
    {
        instance = this;
    }
    public int GetRequireFragment_Skill(Rarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case Rarity.Common:
                return priceInfo.commonSkillPrice[level];
            case Rarity.Uncommon:
                return priceInfo.uncommonSkillPrice[level];
            case Rarity.Rare:
                return priceInfo.rareSkillPrice[level];
            case Rarity.Unique:
                return priceInfo.uniqueSkillPrice[level];
            case Rarity.Legendary:
                return priceInfo.legendarySkillPrice[level];
            case Rarity.Mythic:
                return priceInfo.mythicSkillPrice[level];
        }
        return int.MaxValue;
    }
    public int GetRequireWeaponCount(Rarity weaponRarity, int level)
    {
        Debug.Log(level);
        switch (weaponRarity)
        {
            case Rarity.Common:
                return priceInfo.commonWeaponPrice[level];
            case Rarity.Uncommon:
                return priceInfo.uncommonWeaponPrice[level];
            case Rarity.Rare:
                return priceInfo.rareWeaponPrice[level];
            case Rarity.Unique:
                return priceInfo.uniqueWeaponPrice[level];
            case Rarity.Legendary:
                return priceInfo.legendaryWeaponPrice[level];
            case Rarity.Mythic:
                return priceInfo.mythicWeaponPrice[level];
        }
        return int.MaxValue;
    }
}

