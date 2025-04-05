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
    public ChestStatusFromStage chestStatusFromStage;
    public CompanionTechInfo companionTechInfo;
    public string GetDropInfo()
    {
        return $"Currently Undefined";
    }
    [Serializable]
    public class EnemyStatusFromStage
    {
        [Header("Status")]
        //ÀÏ¹Ý¸÷°ú º¸½º¸÷ÀÌ °®´Â ½ºÅÈ
        public string maxHp;
        public float resist;
        [Header("Reward")]
        public int gold;
        public int exp;
    }
    [Serializable]
    public class ChestStatusFromStage
    {
        [Header("Status")]
        public string maxHp;
        public float resist;
        [Header("Reward")]
        public int gold;
        public int exp;
    }
    [Serializable]
    public class BossStatusFromStage
    {
        //CompanionÀº Boss Status¸¦ Àû¿ëÇÑ´Ù.
        [Header("Status")]
        //ÀÏ¹Ý¸÷°ú º¸½º¸÷ÀÌ °®´Â ½ºÅÈ
        public string maxHp;
        public float resist;

        //º¸½º¸÷¿¡°Ô¸¸ ÀÇ¹ÌÀÖ´Â ½ºÅÈ
        public string power;
        public float penetration;
        [Header("Reward")]
        public int gold;
        public int exp;
    }
    [Serializable]
    public class CompanionTechInfo
    {
        [Header("Info")]
        public int companionNum;
        public int techIndex_0;//Çà
        public int techIndex_1;//¿­
        public int recommendLevel;
    }
}
