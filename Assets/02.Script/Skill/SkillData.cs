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
    public List<float> value;//���� �� ���
    public SkillType type = SkillType.Damage;
    public SkillTarget target = SkillTarget.Opponent;//������ ������� ����
    public int targetNum = 1;
    //�ִϸ��̼� �ð��� ����ؼ� Delay�� �ּ� �ð��� �����ؾ� �Ѵ�.
    public float preDelay = 0.2f;//���� ����
    public float postDelay = 0.2f;//���� �ĵ�
    public bool isAnim = true;//�ִϸ��̼��� ��������
    public GameObject visualEffectPrefab;//���� ����Ʈ ������
}