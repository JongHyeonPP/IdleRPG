using EnumCollection;
using System;
using UnityEngine;

public class EquipedSkill
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
    public void SetCoolMax()
    {
        switch (skillData.skillCoolType)
        {
            case SkillCoolType.ByAtt:
                currentCoolAttack = skillData.coolAttack;
                    break;
            case SkillCoolType.ByTime:
                currentCoolTime = skillData.cooltime;
                break;
        }
    }
    public EquipedSkill(SkillData skillData)
    {
        this.skillData = skillData;
        currentCoolTime = 0f;
        currentCoolAttack = 0;
        if (!StartBroker.GetGameData().skillLevel.TryGetValue(skillData.name, out int level))
        {
            Debug.Log($"{skillData.name} - {level}");
        }
        GameData gameData = StartBroker.GetGameData();
        PlayerBroker.OnSkillLevelSet += OnSkillLevelChange;
        this.level = level;
    }
    public EquipedSkill()//�⺻ ����
    {
        skillData = SkillManager.instance.defaultAttackData;
    }
    private void OnSkillLevelChange(string skillId, int skillLevel)
    {
        if (skillData.name == skillId)
        {
            level = skillLevel;
        }
    }

}