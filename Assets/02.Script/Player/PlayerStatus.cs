using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, ICharacterStatus
{

    [SerializeField] private BigInteger _maxHp;
    [SerializeField] private BigInteger _power;
    [SerializeField] private BigInteger _hpRecover;
    [SerializeField] private float _critical;
    [SerializeField] private float _criticalDamage;
    [SerializeField] private int _mana;
    [SerializeField] private int _manaRecover;
    [SerializeField] private float _accuracy;
    [SerializeField] private float _evasion;
    [SerializeField] private float _goldAscend;
    [SerializeField] private float _expAscend;
    [SerializeField] private List<Skill> _skills = new();

    public BigInteger MaxHp { get => _maxHp; set => _maxHp = value; }
    public BigInteger Power { get => _power; set => _power = value; }
    public BigInteger HpRecover { get => _hpRecover; set => _hpRecover = value; }
    public float Critical { get => _critical; set => _critical = value; }
    public float CriticalDamage { get => _criticalDamage; set => _criticalDamage = value; }
    public int Mana { get => _mana; set => _mana = value; }
    public int ManaRecover { get => _manaRecover; set => _manaRecover = value; }
    public float Accuracy { get => _accuracy; set => _accuracy = value; }
    public float Evasion { get => _evasion; set => _evasion = value; }
    public float GoldAscend { get => _goldAscend; set => _goldAscend = value; }
    public float ExpAscend { get => _expAscend; set => _expAscend = value; }
    public List<Skill> Skills { get => _skills; set => _skills = value; }

}
