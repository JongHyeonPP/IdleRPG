using EnumCollection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PriceInfo;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;
    [SerializeField] PriceInfo _priceInfo;
    public const int MAXPLAYERSKILLLEVEL = 10;
    public const int MAXCOMPANIONSKILLLEVEL = 20;
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
                return _priceInfo.commonSkillPrice[level];
            case Rarity.Uncommon:
                return _priceInfo.uncommonSkillPrice[level];
            case Rarity.Rare:
                return _priceInfo.rareSkillPrice[level];
            case Rarity.Unique:
                return _priceInfo.uniqueSkillPrice[level];
            case Rarity.Legendary:
                return _priceInfo.legendarySkillPrice[level];
            case Rarity.Mythic:
                return _priceInfo.mythicSkillPrice[level];
        }
        return int.MaxValue;
    }
    public int GetRequireWeaponCount(Rarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case Rarity.Common:
                return _priceInfo.commonWeaponPrice[level];
            case Rarity.Uncommon:
                return _priceInfo.uncommonWeaponPrice[level];
            case Rarity.Rare:
                return _priceInfo.rareWeaponPrice[level];
            case Rarity.Unique:
                return _priceInfo.uniqueWeaponPrice[level];
            case Rarity.Legendary:
                return _priceInfo.legendaryWeaponPrice[level];
            case Rarity.Mythic:
                return _priceInfo.mythicWeaponPrice[level];
        }
        return int.MaxValue;
    }
    public (int, int) GetRequireCompanionSkill_CloverFragment(int companionIndex, int skillIndex, int skillLevel)
    {
        CompanionSkillPrice price = new();

        switch (companionIndex)
        {
            case 0:
                switch (skillIndex)
                {
                    case 0:
                        price = _priceInfo.companion0_SkillPrice0[skillLevel];
                        break;
                    case 1:
                        price = _priceInfo.companion0_SkillPrice1[skillLevel];
                        break;
                    case 2:
                        price = _priceInfo.companion0_SkillPrice2[skillLevel];
                        break;
                }
                break;
            case 1:
                switch (skillIndex)
                {
                    case 0:
                        price = _priceInfo.companion1_SkillPrice0[skillLevel];
                        break;
                    case 1:
                        price = _priceInfo.companion1_SkillPrice1[skillLevel];
                        break;
                    case 2:
                        price = _priceInfo.companion1_SkillPrice2[skillLevel];
                        break;
                }
                break;
            case 2:
                switch (skillIndex)
                {
                    case 0:
                        price = _priceInfo.companion2_SkillPrice0[skillLevel];
                        break;
                    case 1:
                        price = _priceInfo.companion2_SkillPrice1[skillLevel];
                        break;
                    case 2:
                        price = _priceInfo.companion2_SkillPrice2[skillLevel];
                        break;
                }
                break;
        }

        return (price.clover, price.fragment);
    }

}


