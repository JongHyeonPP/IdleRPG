using UnityEngine;
using EnumCollection;
using System;

[CreateAssetMenu(fileName = "StageInfo", menuName = "ScriptableObjects/StageInfo")]
public class StageInfo : ScriptableObject, IListViewItem
{
    [Header("Stage Info")]
    public int stageNum;
    public string stageName;
    public Background background;
    [Header("Drop Info")]
    //public EquipGrade equipGrade;
    //public EquipType equipType;
    public float dropProb;
    [Header("Enemy Info")]
    public EnemyStatus enemy_0;
    public EnemyStatus enemy_1;
    public int enemyNum;

    public string GetDropInfo()
    {
        return $"Currently Undefined";
    }
}
