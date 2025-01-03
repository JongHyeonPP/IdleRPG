using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, ICharacterStatus
{
    #region Field
    //상속받음
    [SerializeField] private BigInteger _maxHp;
    [SerializeField] private BigInteger _power;
    [SerializeField] private BigInteger _hpRecover;
    [SerializeField] private float _critical;
    [SerializeField] private float _criticalDamage;
    [SerializeField] private float _resist;
    [SerializeField] private float _penetration;
    [SerializeField] private List<SkillInBattle> _skills = new();
    //상속받지 않음
    [SerializeField] private float _maxMp;
    [SerializeField] private float _manaRecover;
    [SerializeField] private float _goldAscend;
    [SerializeField] private float _expAscend;
    #endregion
    #region Property
    public BigInteger MaxHp { get => _maxHp; set => _maxHp = value; }
    public BigInteger Power { get => _power; set => _power = value; }
    public BigInteger HpRecover { get => _hpRecover; set => _hpRecover = value; }
    public float Critical { get => _critical; set => _critical = value; }
    public float CriticalDamage { get => _criticalDamage; set => _criticalDamage = value; }
    public float Resist { get => _resist; set => _resist = value; }
    public float Penetration { get => _penetration; set => _penetration = value; }
    public List<SkillInBattle> Skills { get => _skills; set => _skills = value; }
    public float MaxMp { get => _maxMp; set => _maxMp = value; }
    public float MpRecover { get => _manaRecover; set => _manaRecover = value; }
    public float GoldAscend { get => _goldAscend; set => _goldAscend = value; }
    public float ExpAscend { get => _expAscend; set => _expAscend = value; }
    #endregion
}
