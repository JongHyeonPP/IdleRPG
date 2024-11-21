using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, ICharacterStatus
{
    public List<Skill> skills = new();

    [SerializeField] private int _maxHp;
    [SerializeField] private int _power;
    [SerializeField] private int _hpRecover;
    [SerializeField] private int _critical;
    [SerializeField] private int _criticalDamage;
    [SerializeField] private int _mana;
    [SerializeField] private int _manaRecover;
    [SerializeField] private int _accuracy;
    [SerializeField] private int _evasion;

    [SerializeField] private int _goldAscend;
    [SerializeField] private int _expAscend;

    public int MaxHp { get => _maxHp; set => _maxHp = value; }
    public int Power { get => _power; set => _power = value; }
    public int HpRecover { get => _hpRecover; set => _hpRecover = value; }
    public int Critical { get => _critical; set => _critical = value; }
    public int CriticalDamage { get => _criticalDamage; set => _criticalDamage = value; }
    public int Mana { get => _mana; set => _mana = value; }
    public int ManaRecover { get => _manaRecover; set => _manaRecover = value; }
    public int Accuracy { get => _accuracy; set => _accuracy = value; }
    public int Evasion { get => _evasion; set => _evasion = value; }

    public int GoldAscend => _goldAscend;
    public int ExpAscend => _expAscend;
}
