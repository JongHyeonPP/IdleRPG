using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int power = 10;
    [SerializeField] private int hpRecover = 0;
    [SerializeField] private int critical = 0;
    [SerializeField] private int criticalDamage = 0;
    [SerializeField] private int mana = 0;
    [SerializeField] private int manaRecover = 0;
    [SerializeField] private int accuracy = 0;
    [SerializeField] private int evasion = 0;
    public GameObject prefab;

    public int MaxHp { get => maxHp; set => maxHp = value; }
    public int Power { get => power; set => power = value; }
    public int HpRecover { get => hpRecover; set => hpRecover = value; }
    public int Critical { get => critical; set => critical = value; }
    public int CriticalDamage { get => criticalDamage; set => criticalDamage = value; }
    public int Mana { get => mana; set => mana = value; }
    public int ManaRecover { get => manaRecover; set => manaRecover = value; }
    public int Accuracy { get => accuracy; set => accuracy = value; }
    public int Evasion { get => evasion; set => evasion = value; }

    [SerializeField] private float gold;
    [SerializeField] private float exp;
    [SerializeField] private List<string> skillNames;

    public float Gold=> gold;
    public float Exp => exp;
    public List<string> SkillNames => skillNames;
}