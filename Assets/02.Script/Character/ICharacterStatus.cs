using System.Collections.Generic;
using System.Numerics;

public interface ICharacterStatus
{
    BigInteger MaxHp { get; }
    BigInteger Power { get; }
    float Resist { get; }
    float Penetration { get; }
}