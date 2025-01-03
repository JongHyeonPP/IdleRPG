using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;
    [SerializeField] PriceInfo skillPriceInfo;
    private void Awake()
    {
        instance = this;
    }
    public int GetRequireEmerald_Skill(WeaponRarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case WeaponRarity.Common:
                return skillPriceInfo._commonSkillEmerald[level];
            case WeaponRarity.Uncommon:
                return skillPriceInfo._uncommonSkillEmerald[level];
            case WeaponRarity.Rare:
                return skillPriceInfo._rareSkillEmerald[level];
            case WeaponRarity.Unique:
                return skillPriceInfo._uniqueSkillEmerald[level];
            case WeaponRarity.Legendary:
                return skillPriceInfo._legendarySkillEmerald[level];
            case WeaponRarity.Mythic:
                return skillPriceInfo._mythicSkillEmerald[level];
        }
        return int.MaxValue;
    }
    public int GetRequirePiece_Skill(WeaponRarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case WeaponRarity.Common:
                return skillPriceInfo._commonSkillPiece[level];
            case WeaponRarity.Uncommon:
                return skillPriceInfo._uncommonSkillPiece[level];
            case WeaponRarity.Rare:
                return skillPriceInfo._rareSkillPiece[level];
            case WeaponRarity.Unique:
                return skillPriceInfo._uniqueSkillPiece[level];
            case WeaponRarity.Legendary:
                return skillPriceInfo._legendarySkillPiece[level];
            case WeaponRarity.Mythic:
                return skillPriceInfo._mythicSkillPiece[level];
        }
        return int.MaxValue;
    }
}

