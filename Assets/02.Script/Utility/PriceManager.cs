using EnumCollection;
using System.Collections.Generic;
using UnityEngine;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;
    [SerializeField] PriceInfo skillPriceInfo;
    public const int MAXSKILLLEVEL = 7;
    private void Awake()
    {
        instance = this;
    }
    public int GetRequireFragment_Skill(Rarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case Rarity.Common:
                return skillPriceInfo._commonSkillPiece[level];
            case Rarity.Uncommon:
                return skillPriceInfo._uncommonSkillPiece[level];
            case Rarity.Rare:
                return skillPriceInfo._rareSkillPiece[level];
            case Rarity.Unique:
                return skillPriceInfo._uniqueSkillPiece[level];
            case Rarity.Legendary:
                return skillPriceInfo._legendarySkillPiece[level];
            case Rarity.Mythic:
                return skillPriceInfo._mythicSkillPiece[level];
        }
        return int.MaxValue;
    }
}

