using System.Collections.Generic;
using System.Numerics;

public interface ICharacterStatus
{
    BigInteger MaxHp { get; }
    BigInteger Power { get; }
    BigInteger HpRecover { get; }
    float Critical { get; }
    float CriticalDamage { get; }
    int Mana { get; }
    int ManaRecover { get; }
    float Accuracy { get; }
    float Evasion { get; }
    List<Skill> Skills { get; }
}