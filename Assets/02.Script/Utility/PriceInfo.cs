using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PriceInfo", menuName = "ScriptableObjects/PriceInfo")]
public class PriceInfo : ScriptableObject
{
    [Header("Skill_Piece")]
    public int[] commonSkillPrice;
    public int[] uncommonSkillPrice;
    public int[] rareSkillPrice;
    public int[] uniqueSkillPrice;
    public int[] legendarySkillPrice;
    public int[] mythicSkillPrice;
    [Header("Weapon")]
    public int[] commonWeaponPrice;
    public int[] uncommonWeaponPrice;
    public int[] rareWeaponPrice;
    public int[] uniqueWeaponPrice;
    public int[] legendaryWeaponPrice;
    public int[] mythicWeaponPrice;
    [Header("Companion_0_Skill")]
    public CompanionSkillPrice[] companion0_SkillPrice0;
    public CompanionSkillPrice[] companion0_SkillPrice1;
    public CompanionSkillPrice[] companion0_SkillPrice2;
    [Header("Companion_1_Skill")]
    public CompanionSkillPrice[] companion1_SkillPrice0;
    public CompanionSkillPrice[] companion1_SkillPrice1;
    public CompanionSkillPrice[] companion1_SkillPrice2;
    [Header("Companion_2_Skill")]
    public CompanionSkillPrice[] companion2_SkillPrice0;
    public CompanionSkillPrice[] companion2_SkillPrice1;
    public CompanionSkillPrice[] companion2_SkillPrice2;

    [Serializable]
    public struct CompanionSkillPrice
    {
        public int clover;
        public int fragment;
    }
}
