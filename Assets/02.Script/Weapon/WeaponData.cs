using UnityEngine;
using EnumCollection;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using System;
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
    [SerializeField] private float _attackSpeed;
    [SerializeField] private Sprite _weaponSprite;
    [SerializeField] private Vector2 _textureSize = new(1, 1);
    [SerializeField] private float _textureScale;
    [SerializeField] private string _weaponName;
    [SerializeField] private int _powerPerUpgrade;
    [SerializeField] private int _critDmgPerUpgrade;
    [SerializeField] private int _critPerUpgrade;
    [SerializeField] private float _attackSpeedPerUpgrade;
    public WeaponEffect[] _weaponEffects;
    public WeaponType WeaponType => _weaponType;
    public Rarity WeaponRarity => _weaponRarity;
    public int Power => _power;
    public int CriticalDamage => _criticalDamage;
    public int Critical => _critical;

    public float AttackSpeed => _attackSpeed;
    public Sprite WeaponSprite => _weaponSprite;
    public Vector2 TextureSize =>  _textureSize;
    public float TextureScale =>  _textureScale;
    public string UID => name;
    public int PowerPerUpgrade => _powerPerUpgrade;
    public int CritDmgPerUpgrade => _critDmgPerUpgrade;
    public int CritPerUpgrade => _critPerUpgrade;
    public float AttackSpeedPerUpgrade => _attackSpeedPerUpgrade;

    public string WeaponName => _weaponName;
    public void SetReinforcedStats(float powerIncrease, float critDamageIncrease, float critIncrease,float speedIncrease)
    {
        _power += (int)powerIncrease;
        _criticalDamage += (int)critDamageIncrease;
        _critical += (int)critIncrease;
        _attackSpeed += (float)speedIncrease;
    }
    public (int power, int critDamage, int crit,float attackSpeed) GetStats(int level)
    {
        int power = _power + (PowerPerUpgrade * level);
        int critDamage = _criticalDamage + (CritDmgPerUpgrade * level);
        int crit = _critical + (CritPerUpgrade * level);
        float attackspeed = _attackSpeed + (AttackSpeedPerUpgrade * level);
        return (power, critDamage, crit,attackspeed);
    }
    public Rarity ItemRarity => WeaponRarity;
    
    [Serializable]
    public class WeaponEffect
    {
        public SkillType type;
        public float value;
    }
}
