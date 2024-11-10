using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContoller : Attackable
{
    PlayerStatus status;
    private void Start()
    {
        status = GetComponent<PlayerStatus>();
    }
    public void StartAttack()
    {
        MoveState(false);
        StartCoroutine(AttackCor());
    }
    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {
    
    }
    private IEnumerator AttackCor()
    {
        while (true)
        {
            bool isActiveSkill = false;
            foreach (Skill skill in status.skills)
            {
                if (skill.IsSkillAble())
                {
                    SkillData data = skill.data;
                    ActiveSkill(data.name, data.value[0], data.range, data.range);
                    isActiveSkill = true;
                }
            }
            if (!isActiveSkill)
            {
                if (target)
                    DefaultAttack();
            }
            yield return new WaitForSeconds(attackTerm);
        }
    }
    public void MoveState(bool _isMove)
    {
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    private void ActiveSkill(string _skillName, float _value, float _range, float type)
    {
        Debug.Log("Skill : " + _skillName);
    }
    private void DefaultAttack()
    {
        SkillBehaviour(1, SkillType.Damage, SkillRange.Target);
        Debug.Log("Defualt Attack To " + target.name);
    }

    protected override ICharacterStatus GetStatus()
    {
        return status;
    }
}
