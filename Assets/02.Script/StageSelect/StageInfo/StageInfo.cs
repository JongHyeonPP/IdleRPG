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
    public int recommendLevel;
    [Header("Enemy Info")]
    public EnemyStatus enemy_0;
    public EnemyStatus enemy_1;
    public EnemyStatus boss;
    public int enemyNum;
    public EnemyStatusFromStage enemyStatusFromStage;
    public BossStatusFromStage bossStatusFromStage;
    public ChestStatusFromStage chestStatusFromStage;
    public CompanionTechInfo companionTechInfo;
    public AdventureInfo adventrueInfo;
    public string GetDropInfo()
    {
        return "";
    }
    [Serializable]
    public class EnemyStatusFromStage
    {
        //ÀÏ¹Ý¸÷°ú º¸½º¸÷ÀÌ °®´Â ½ºÅÈ
        public string maxHp;
        public float resist;
    }
    [Serializable]
    public class ChestStatusFromStage
    {
        public string maxHp;
        public float resist;
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
    }
    [Serializable]
    public class CompanionTechInfo
    {
        public int companionNum;
        public int techIndex_0;//Çà
        public int techIndex_1;//¿­
        public int recommendLevel;
    }
    [Serializable]
    public class AdventureInfo
    {
        public int adventureIndex_0;
        public int adventureIndex_1;

        public float imageLeft;
        public float imageScale = 1f;
    }


    [ContextMenu("SetAdventureIndex")]
    public void SetAdventureIndex()
    {
        string[] splitted = name.Split('_');
        int index_0 = int.Parse(splitted[1]);
        int index_1 = int.Parse(splitted[2]);
        adventrueInfo.adventureIndex_0 = index_0;
        adventrueInfo.adventureIndex_1 = index_1;
    }
}
