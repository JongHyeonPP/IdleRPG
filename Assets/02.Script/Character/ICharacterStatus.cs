using System.Collections.Generic;
using System.Numerics;

public interface ICharacterStatus
{
    BigInteger MaxHp { get; }
    BigInteger Power { get; }
    BigInteger HpRecover { get; }
    float Critical { get; }
    float CriticalDamage { get; }
    float Resist { get; }
    float Penetration { get; }
    List<SkillInBattle> Skills { get; }
}