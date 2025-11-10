using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillEffectSpawnType
{
    OnTarget,        // 타겟 위치에서 생성
    InFrontOfCaster, // 시전자 앞에서 생성
    Projectile,      // 투사체
    Buff,            // 버프
    EnemyTarget      // 적 타겟
}

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    public Sprite iconSprite;
    public string simple;
    public string complex;
    public Rarity rarity;
    public bool isPlayerSkill;

    [Header("ToActive")]
    public SkillCoolType skillCoolType;  // or by attack
    public float cooltime = 3f;
    public int coolAttack = 3;
    public int requireMp = 15;

    [Header("Effect Behavior")]
    public SkillEffectSpawnType effectSpawnType = SkillEffectSpawnType.InFrontOfCaster;
    public float projectileSpeed = 15f;
    public float effectLifeTime = 5f;

    [Header("Content")]
    public List<float> value; // 레벨별 밸류
    public SkillType type = SkillType.Damage;
    public SkillTarget target = SkillTarget.Opponent; // 대상
    public int targetNum = 1;
    public float preDelay = 0.2f; // 선딜
    public float postDelay = 0.2f; // 후딜
    public bool isAnim = true;
    public GameObject visualEffectPrefab;
    public bool isActiveSkill;

    [Header("Sound Clips")]
    [Tooltip("스킬 시전 시 재생할 사운드")]
    public AudioClip castClip;

    [Tooltip("명중 시 재생할 사운드")]
    public AudioClip hitClip;
}
