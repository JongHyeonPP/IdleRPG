using UnityEngine;
using EnumCollection;
[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private Rarity _weaponRarity;
    [SerializeField] private int _power;
    [SerializeField] private int _criticalDamage;
    [SerializeField] private int _critical;
    [SerializeField] private Sprite _weaponSprite;
    [SerializeField] private Vector2 _textureSize = new(1, 1);
    [SerializeField] private float _textureScale;
    [SerializeField] string _uID;
    [SerializeField] private string _weaponName;
    public WeaponType WeaponType => _weaponType;
    public Rarity WeaponRarity => _weaponRarity;
    public int Power => _power;
    public int CriticalDamage => _criticalDamage;
    public int Critical => _critical;
    public Sprite WeaponSprite => _weaponSprite;
    public Vector2 TextureSize =>  _textureSize;
    public float TextureScale =>  _textureScale;
    public string UID => _uID;

    public string WeaponName => _weaponName;
    public void SetReinforcedStats(float powerIncrease, float critDamageIncrease, float critIncrease)
    {
        _power += (int)powerIncrease;
        _criticalDamage += (int)critDamageIncrease;
        _critical += (int)critIncrease;
    }

}
