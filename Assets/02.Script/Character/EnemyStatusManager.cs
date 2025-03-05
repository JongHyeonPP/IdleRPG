using EnumCollection;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class EnemyStatusManager:MonoBehaviour
{
    public static EnemyStatusManager instance;

    private int _stageNum;//현재 스테이지 번호
    
    //일반몹과 보스몹이 갖는 스탯
    public BigInteger maxHp { get; private set; }
    public float resist { get; private set; }

    //보스몹에게만 의미있는 스탯
    public BigInteger power { get; private set; }
    public float critical { get; private set; }
    public float criticalDamage { get; private set; }
    public float resistPenetration { get; private set; }

    //스테이지와 상관없이 결정되는 스탯
    public int mana { get; private set; }
    public int manaRecover { get; private set; }
    public BigInteger hpRecover { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.OnStageEnter += OnStageEnter;
        BattleBroker.OnBossEnter += OnBossEnter;
    }
    private void Start()
    {
        
    }
    private void OnStageChange(int stageNum)
    {
        _stageNum = stageNum;
    }
    private void OnStageEnter()
    {
        maxHp = GetMaxHp_Default();
        resist = GetEvasion_Default();
        power = hpRecover = mana = manaRecover = 0;
        critical = criticalDamage = resistPenetration = 0f;
    }
    private void OnBossEnter()
    {
        maxHp = GetMaxHp_Boss();
        resist = GetResist_Boss();
        power = GetPower_Boss();
        critical = GetCritical_Boss();
        criticalDamage = GetCriticalDamage_Boss();
        resistPenetration = GetResistPenetration_Boss();
        mana = 100;
        manaRecover = 10;
        hpRecover = 0;
    }
    private int GetMaxHp_Default()
    {
        return 100;
    }
    private float GetEvasion_Default()
    {
        return 0f;
    }
    private int GetMaxHp_Boss()
    {
        return 1000;
    }
    private float GetResist_Boss()
    {
        return 0f;
    }
    private int GetPower_Boss()
    {
        return 10;
    }
    private float GetCritical_Boss()
    {
        return 0f;
    }
    private float GetCriticalDamage_Boss()
    {
        return 0f;
    }
    private float GetResistPenetration_Boss()
    {
        return 0f;
    }
}