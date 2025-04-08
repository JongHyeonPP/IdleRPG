using EnumCollection;
using Mono.Cecil;
using System;
using System.Numerics;
using UnityEngine;
public static class EnemyBroker
{
    //Enemy
    public static Func<EnemyType, BigInteger> GetEnemyMaxHp;
    public static Func<EnemyType, float> GetEnemyResist;
    public static Func<EnemyType, BigInteger> GetEnemyPower;
    public static Func<EnemyType, float> GetEnemyPenetration;
}