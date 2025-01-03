using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    public Sprite icon;
    public string simple;
    public string complex;
    public WeaponRarity rarity;

    [Header("ToActive")]
    public SkillCoolType skillCoolType;//or by attack
    public float cooltime = 3f;
    public int coolAttack = 3;
    public int requireMp = 15;

    [Header("Composition")]
    public List<float> value;//·¹º§ º° ¹ë·ù
    public SkillType type = SkillType.Damage;
    public int range = 1;
    public int num = 1;
}