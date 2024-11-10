using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    public string id;
    public string skillName;
    public List<float> value;
    public bool byTime;//or by attack
    public float cooltime;
    public int coolAttack;
    public SkillType type;
    public Sprite icon;
    public int range;
}