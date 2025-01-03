using EnumCollection;
using UnityEngine;

public class SkillInBattle : MonoBehaviour // ����� �� �ش� ������ ���
{
    public SkillData data;
    public float currentCooltime;
    public int currentCoolAttack;
    public int level;
    public bool IsSkillAble()
    {
        switch (data.skillCoolType)
        {
            case SkillCoolType.ByAtt:
                return currentCoolAttack == 0;
            case SkillCoolType.ByTime:
                return currentCooltime == 0;
        }
        return false;
    }
}
