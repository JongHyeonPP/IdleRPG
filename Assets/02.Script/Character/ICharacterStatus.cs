using UnityEngine;

public interface ICharacterStatus
{
    int MaxHp { get; set; }
    int Power { get; set; }
    int HpRecover { get; set; }
    int Critical { get; set; }
    int CriticalDamage { get; set; }
    int Mana { get; set; }
    int ManaRecover { get; set; }
    int Accuracy { get; set; }
    int Evasion { get; set; }
}
