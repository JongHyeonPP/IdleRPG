using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, ICharacterStatus
{
    #region Field

    public int _maxHp_Gold;
    public int _maxHp_StatPoint;
    public int _maxHp_Promote;
    public int _power_Gold;
    public int _power_StatPoint;
    public int _power_Promote;
    public int _hpRecover_Gold;
    public int _hpRecover_StatPoint;
    
    public float _critical_Gold;

    public float _criticalDamage_Gold;
    public float _criticalDamage_StatPoint;
    public float _criticalDamage_Promote;
    public float _goldAscend_StatPoint;
    [SerializeField] WeaponController playerWeapon;
    #endregion
    #region Property
    public BigInteger MaxHp { get =>  new( _maxHp_Gold + _maxHp_StatPoint); }
    public BigInteger Power { get => new ( _power_Gold + _power_StatPoint); }
    public BigInteger HpRecover { get => new(_hpRecover_Gold+_hpRecover_StatPoint); }
    public float Critical { get => _critical_Gold;  }
    public float CriticalDamage { get => _criticalDamage_Gold+_criticalDamage_StatPoint; }
    public float Resist
    {
        get
        {
            if (playerWeapon == null)
                return 0;
            else
                return playerWeapon.weaponData.Resist;
        }
    }
    public float Penetration
    {
        get
        {
            if (playerWeapon == null)
                return 0;
            else
                return playerWeapon.weaponData.Penetration;
        }
    }
    public float MaxMp { get => 100f;}
    public float MpRecover { get => 10f; }
    public float GoldAscend { get => _goldAscend_StatPoint;}
    public float ExpAscend { get => 0f; }
    #endregion
}
