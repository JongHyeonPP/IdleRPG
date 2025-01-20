using UnityEngine;

[CreateAssetMenu(fileName = "PriceInfo", menuName = "ScriptableObjects/PriceInfo")]
public class PriceInfo : ScriptableObject
{
    [Header("Skill_Piece")]
    public int[] _commonSkillPiece;
    public int[] _uncommonSkillPiece;
    public int[] _rareSkillPiece;
    public int[] _uniqueSkillPiece;
    public int[] _legendarySkillPiece;
    public int[] _mythicSkillPiece;
    [Header("Weapon")]
    public int[] _commonWeaponPrice;
    public int[] _uncommonSWeaponPrice;
    public int[] _rareWeaponPrice;
    public int[] _uniqueWeaponPrice;
    public int[] _legendaryWeaponPrice;
    public int[] _mythicWeaponPrice;
    [Header("Companion")]
    public int[] _companionPrice;
}
