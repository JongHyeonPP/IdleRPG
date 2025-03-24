using UnityEngine;
using EnumCollection;
using System;
using System.Numerics;

[CreateAssetMenu(fileName = "StageInfo", menuName = "ScriptableObjects/StageInfo")]
public class StageInfo : ScriptableObject, IListViewItem
{
    [Header("Stage Info")]
    public int stageNum;
    public string stageName;
    public Background background;
    //[Header("Drop Info")]
    //public EquipGrade equipGrade;
    //public EquipType equipType;
    public float dropProb;
    [Header("Enemy Info")]
    public EnemyStatus enemy_0;
    public EnemyStatus enemy_1;
    public EnemyStatus boss;
    public int enemyNum;
    public EnemyStatusFromStage enemyStatusFromStage;
    public BossStatusFromStage bossStatusFromStage;
    public CompanionTechIndex companionTechIndex;
    public string GetDropInfo()
    {
        return $"Currently Undefined";
    }
    [Serializable]
    public class EnemyStatusFromStage
    {
        //일반몹과 보스몹이 갖는 스탯
        public string maxHp;
        public float resist;

        //보스몹에게만 의미있는 스탯
        //public string power;
        //public float critical;
        //public float criticalDamage;
        //public float penetration;
        //public int mana;
        //public int manaRecover;
    }
    [Serializable]
    public class BossStatusFromStage
    {
        //일반몹과 보스몹이 갖는 스탯
        public string maxHp;
        public float resist;

        //보스몹에게만 의미있는 스탯
        public string power;
        public float penetration;
    }
    [Serializable]
    public class CompanionTechIndex
    {
        public int companionNum;
        public int techIndex_0;//행
        public int techIndex_1;//열
    }
}
