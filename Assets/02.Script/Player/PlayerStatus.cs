using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, ICharacterStatus
{
    public List<Skill> skills = new();

    [SerializeField] private int maxHp;
    [SerializeField] private int power;
    [SerializeField] private int hpRecover;
    [SerializeField] private int critical;
    [SerializeField] private int criticalDamage;
    [SerializeField] private int mana;
    [SerializeField] private int manaRecover;
    [SerializeField] private int accuracy;
    [SerializeField] private int evasion;

    [SerializeField] private float goldAscend;
    [SerializeField] private float expAscend;

    public int MaxHp { get => maxHp; set => maxHp = value; }
    public int Power { get => power; set => power = value; }
    public int HpRecover { get => hpRecover; set => hpRecover = value; }
    public int Critical { get => critical; set => critical = value; }
    public int CriticalDamage { get => criticalDamage; set => criticalDamage = value; }
    public int Mana { get => mana; set => mana = value; }
    public int ManaRecover { get => manaRecover; set => manaRecover = value; }
    public int Accuracy { get => accuracy; set => accuracy = value; }
    public int Evasion { get => evasion; set => evasion = value; }

    public float GoldAscend => goldAscend;
    public float ExpAscend => expAscend;
}
