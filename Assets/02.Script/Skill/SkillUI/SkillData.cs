using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    public string uid;
    public Sprite iconSprite;
    public string simple;
    public string complex;
    public Rarity rarity;
    public bool isPlayerSkill;

    [Header("ToActive")]
    public SkillCoolType skillCoolType;//or by attack
    public float cooltime = 3f;
    public int coolAttack = 3;
    public int requireMp = 15;

    [Header("Content")]
    public List<float> value;//레벨 별 밸류
    public SkillType type = SkillType.Damage;
    public SkillTarget target = SkillTarget.Opponent;//누구를 대상으로 할지
    public int targetNum = 1;
    //애니메이션 시간을 고려해서 Delay는 최소 시간을 보장해야 한다.
    public float preDelay = 0.2f;//판정 선딜
    public float postDelay = 0.2f;//판정 후딜
    public bool isAnim = true;//애니메이션을 보여줄지
    public GameObject visualEffectPrefab;//공격 이펙트 프리팹
}