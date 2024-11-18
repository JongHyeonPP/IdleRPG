using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _power = 10;
    [SerializeField] private int _hpRecover = 0;
    [SerializeField] private int _critical = 0;
    [SerializeField] private int _criticalDamage = 0;
    [SerializeField] private int _mana = 0;
    [SerializeField] private int _manaRecover = 0;
    [SerializeField] private int _accuracy = 0;
    [SerializeField] private int _evasion = 0;
    public GameObject prefab;

    public int MaxHp { get => _maxHp; set => _maxHp = value; }
    public int Power { get => _power; set => _power = value; }
    public int HpRecover { get => _hpRecover; set => _hpRecover = value; }
    public int Critical { get => _critical; set => _critical = value; }
    public int CriticalDamage { get => _criticalDamage; set => _criticalDamage = value; }
    public int Mana { get => _mana; set => _mana = value; }
    public int ManaRecover { get => _manaRecover; set => _manaRecover = value; }
    public int Accuracy { get => _accuracy; set => _accuracy = value; }
    public int Evasion { get => _evasion; set => _evasion = value; }

    [SerializeField] private float gold;
    [SerializeField] private float exp;
    [SerializeField] private List<string> skillNames;

    public float Gold=> gold;
    public float Exp => exp;
    public List<string> SkillNames => skillNames;
}