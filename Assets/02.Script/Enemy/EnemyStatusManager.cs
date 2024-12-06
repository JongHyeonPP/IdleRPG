using EnumCollection;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class EnemyStatusManager:MonoBehaviour
{
    public static EnemyStatusManager instance;

    private int _stageNum;//���� �������� ��ȣ
    
    //�Ϲݸ��� �������� ���� ����
    public BigInteger maxHp { get; private set; }
    public float evasion { get; private set; }

    //���������Ը� �ǹ��ִ� ����
    public BigInteger power { get; private set; }
    public float critical { get; private set; }
    public float criticalDamage { get; private set; }
    public float accuracy { get; private set; }

    //���������� ������� �����Ǵ� ����
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
    }
    private void Start()
    {
        BattleBroker.OnStageChange+=OnStageChange;
        BattleBroker.OnStageEnter+=OnStageEnter;
        BattleBroker.OnBossEnter += OnBossEnter;
    }
    private void OnStageChange(int stageNum)
    {
        _stageNum = stageNum;
    }
    private void OnStageEnter()
    {
        maxHp = GetMaxHp_Default();
        evasion = GetEvasion_Default();
        power = hpRecover = mana = manaRecover = 0;
        critical = criticalDamage = accuracy = 0f;
    }
    private void OnBossEnter()
    {
        maxHp = GetMaxHp_Boss();
        evasion = GetEvasion_Boss();
        power = GetPower_Boss();
        critical = GetCritical_Boss();
        criticalDamage = GetCriticalDamage_Boss();
        accuracy = GetAccuracy_Boss();
        mana = 100;
        manaRecover = 10;
        hpRecover = 0;
    }
    private int GetMaxHp_Default()
    {
        return 10;
    }
    private float GetEvasion_Default()
    {
        return 0f;
    }
    private int GetMaxHp_Boss()
    {
        return 100;
    }
    private float GetEvasion_Boss()
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
    private float GetAccuracy_Boss()
    {
        return 0f;
    }
}