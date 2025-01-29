using UnityEngine;
using EnumCollection;
[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;
    public Rarity weaponRarity;
    public int power;
    public int criticalDamage;
    public int critical;
    public Sprite weaponSprite;
    public Vector2 textureSize;
    public string uid;
    public string weaponName;
}
