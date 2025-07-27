using UnityEngine;
using EnumCollection;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
public interface IGachaItems
{
    Rarity ItemRarity { get; }
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject, IGachaItems
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private Rarity _weaponRarity;
    [SerializeField] private int _power;
    [SerializeField] private int _maxHp;
    [SerializeField] private int _criticalDamage;
    [SerializeField] private int _critical;
    [SerializeField] private int _resist;
    [SerializeField] private int _penetration;
    [SerializeField] private Sprite _weaponSprite;
    [SerializeField] private Vector2 _textureSize = new(1, 1);
    [SerializeField] private float _textureScale;
    [SerializeField] string _uID;
    [SerializeField] private string _weaponName;
    [SerializeField] private int _powerPerUpgrade;
    [SerializeField] private int _critDmgPerUpgrade;
    [SerializeField] private int _critPerUpgrade;
    public WeaponType WeaponType => _weaponType;
    public Rarity WeaponRarity => _weaponRarity;
    public int Power => _power;
    public int CriticalDamage => _criticalDamage;
    public int Critical => _critical;
    public int Resist => _critical;
    public int Penetration => _critical;
    public Sprite WeaponSprite => _weaponSprite;
    public Vector2 TextureSize =>  _textureSize;
    public float TextureScale =>  _textureScale;
    public string UID => _uID;
    public int PowerPerUpgrade => _powerPerUpgrade;
    public int CritDmgPerUpgrade => _critDmgPerUpgrade;
    public int CritPerUpgrade => _critPerUpgrade;
    public string WeaponName => _weaponName;
    public void SetReinforcedStats(float powerIncrease, float critDamageIncrease, float critIncrease)
    {
        _power += (int)powerIncrease;
        _criticalDamage += (int)critDamageIncrease;
        _critical += (int)critIncrease;
    }
    public (int power, int critDamage, int crit) GetStats(int level)
    {
        int power = _power + (PowerPerUpgrade * level);
        int critDamage = _criticalDamage + (CritDmgPerUpgrade * level);
        int crit = _critical + (CritPerUpgrade * level);
        return (power, critDamage, crit);
    }
    public Rarity ItemRarity => WeaponRarity;
}
