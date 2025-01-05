using EnumCollection;
using System;
using UnityEngine;

public class SkillInBattle
{
    public SkillData skillData;
    public float currentCoolTime;
    public int currentCoolAttack;
    public int level;

    // IsSkillAble ������Ƽ�� ��ȯ
    public bool IsSkillAble
    {
        get
        {
            switch (skillData.skillCoolType)
            {
                case SkillCoolType.ByAtt:
                    return currentCoolAttack == 0;
                case SkillCoolType.ByTime:
                    return currentCoolTime == 0;
                default:
                    return false;
            }
        }
    }
    public SkillInBattle(SkillData skillData)
    {
        this.skillData = skillData;
        currentCoolTime = 0f;
        currentCoolAttack = 0;
        if (!StartBroker.GetGameData().skillLevel.TryGetValue(skillData.name, out int level))
        {
            Debug.Log("�����ߴµ� ��ų�ε� ���� ��ų�̷�");
        }
        level = StartBroker.GetGameData().skillLevel[skillData.name];
        BattleBroker.OnSkillLevelChange += OnSkillLevelChange;
    }

    private void OnSkillLevelChange(string skillId, int skillLevel)
    {
        if (skillData.name == skillId)
        {
            level = skillLevel;
        }
    }
}