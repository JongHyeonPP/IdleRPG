using EnumCollection;
using System;
using System.Numerics;
public static class EnemyBroker
{
    //Enemy
    public static Func<EnemyType, BigInteger> GetEnemyMaxHp;
    public static Func<EnemyType, float> GetEnemyResist;
    public static Func<EnemyType, BigInteger> GetEnemyPower;
    public static Func<EnemyType, float> GetEnemyPenetration;
}