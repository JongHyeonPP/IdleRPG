using UnityEngine;
using EnumCollection;
[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private WeaponRarity _weaponRarity;
    [SerializeField] private int _power;
    [SerializeField] private int _criticalDamage;
    [SerializeField] private int _critical;
    [SerializeField] private Sprite _weaponSprite;
    [SerializeField] private Vector2 _textureSize = new Vector2(1, 1);
    [SerializeField] private int _uID;
    [SerializeField] private string _weaponName;
    public WeaponType WeaponType => _weaponType;
    public WeaponRarity WeaponRarity => _weaponRarity;
    public int Power => _power;
    public int CriticalDamage => _criticalDamage;
    public int Critical => _critical;
    public Sprite WeaponSprite => _weaponSprite;
    public Vector2 TextureSize => _textureSize == Vector2.zero ? new Vector2(1, 1) : _textureSize;
    public int UID => _uID;

    public string WeaponName => _weaponName;
}
