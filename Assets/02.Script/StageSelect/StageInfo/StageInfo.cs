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

    [Header("Drop Info")]
    public float goldBonusValue;    // 예: 0.2f → +20%
    public float expBonusValue;     // 예: 0.15f → +15%
    public (Rarity rarity, int count)? fragmentDropInfo;
    public string weaponDropId; // null이면 무기 드롭 없음

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

    [Serializable]
    public class EnemyStatusFromStage
    {
        //일반몹과 보스몹이 갖는 스탯
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
        //Companion은 Boss Status를 적용한다.
        [Header("Status")]
        //일반몹과 보스몹이 갖는 스탯
        public string maxHp;
        public float resist;

        //보스몹에게만 의미있는 스탯
        public string power;
        public float penetration;
    }
    [Serializable]
    public class CompanionTechInfo
    {
        public int companionNum;
        public int techIndex_0;//행
        public int techIndex_1;//열
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
