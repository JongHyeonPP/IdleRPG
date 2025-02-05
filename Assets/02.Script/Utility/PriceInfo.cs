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
    [Header("Companion")]
    public int[] companionPrice;
}
