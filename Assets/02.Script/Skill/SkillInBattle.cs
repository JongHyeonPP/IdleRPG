using EnumCollection;
using System;
using UnityEngine;

public class SkillInBattle
{
    public SkillData skillData;
    public float currentCoolTime;
    public int currentCoolAttack;
    public int level;

    // IsSkillAble 프로퍼티로 변환
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
            Debug.Log("장착했는데 스킬인데 없는 스킬이래");
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