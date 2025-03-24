using EnumCollection;
using Mono.Cecil;
using System;
using System.Numerics;
using UnityEngine;
public static class EnemyBroker
{
    //Enemy
    public static Func<BigInteger> GetEnemyMaxHp;
    public static Func<float> GetEnemyResist;
    //Boss
    public static Func<BigInteger> GetBossMaxHp;
    public static Func<float> GetBossResist;
    public static Func<BigInteger> GetBossPower;
    public static Func<float> GetBossPenetration;
}