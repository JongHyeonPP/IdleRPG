using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public abstract class Attackable : MonoBehaviour
{
    public Attackable target;
    protected float attackTerm = 1f;
    public Animator anim;
    public BigInteger hp;
    //private Dictionary<> tempEffect = new();
    protected Coroutine attackCoroutine;
    public bool isDead;
    protected EquipedSkill[] equipedSkillArr = new EquipedSkill[10];
    private EquipedSkill _defaultAttack;
    protected void SetDefaultAttack()
    {
        _defaultAttack = new();
    }
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackRoop());
    }
    //AttackTerm 간격마다 우선 순위에 있는 스킬 사용
    private IEnumerator AttackRoop()
    {
        while (true)
        {

            EquipedSkill currentSkill = null;
            foreach (EquipedSkill skill in equipedSkillArr)
            {
                if (skill == null)
                    continue;
                if (skill.IsSkillAble)
                {
                    currentSkill = skill;
                    skill.SetCoolMax();
                    break;
                }
            }
            if (currentSkill == null)
            {
                currentSkill = _defaultAttack;
                if (this is PlayerController)
                    ProgressCoolAttack();
            }
            //Debug.Log(currentSkill.skillData.name);
            SkillData skillData = currentSkill.skillData;
            if (skillData.isAnim)
            {
                yield return new WaitForSeconds(skillData.preDelay);
                AnimBehavior(currentSkill, skillData);
                List<Attackable> targets = GetTargets(skillData.target, skillData.targetNum);
                ActiveSkillToTarget(targets, currentSkill);
                VisualEffectToTarget(targets);
                yield return new WaitForSeconds(skillData.postDelay);
            }
            else
            {
                List<Attackable> targets = GetTargets(skillData.target, skillData.targetNum);
                ActiveSkillToTarget(targets, currentSkill);
                VisualEffectToTarget(targets);
            }
            
        }
    }

    private void AnimBehavior(EquipedSkill currentSkill, SkillData skillData)
    {
        if (currentSkill == _defaultAttack)
        {
            if (this is PlayerController)
                anim.SetFloat("AttackState", 0f);
            anim.SetTrigger("Attack");
        }
        else
        {
            switch (skillData.type)
            {
                case SkillType.Damage:
                    anim.SetFloat("AttackState", 1f);
                    anim.SetTrigger("Attack");
                    break;
                case SkillType.Heal:
                case SkillType.AttBuff:
                    anim.SetTrigger("Buff");
                    break;
            }
        }
    }

    private void ProgressCoolAttack()
    {
        foreach (EquipedSkill equipedSkill in equipedSkillArr)
        {
            if (equipedSkill != null)
            {
                if (equipedSkill.skillData.skillCoolType == SkillCoolType.ByAtt)
                    equipedSkill.currentCoolAttack = Mathf.Max(equipedSkill.currentCoolAttack - 1, 0);
            }
        }
    }

    public void StopAttack()
    {
        target = null;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
    private void ActiveSkillToTarget(List<Attackable> targets, EquipedSkill skill)
    {
        ICharacterStatus myStatus = GetStatus();
        SkillData skillData = skill.skillData;
        int skillLevel = skill.level;
        BigInteger calcedValue = new(skillData.value[skillLevel] * 100f);
        calcedValue *= myStatus.Power;
        calcedValue /= 100;
        //100을 곱하고 계산 후 다시 나눠주면 소수점 아래 2자리까지 보존
        foreach (var target in targets)
        {
            //ICharacterStatus targetStatus = target.GetStatus();
            //일련의 계산 진행
            target.ReceiveSkill(calcedValue, skillData.type);
        }
        if (target.hp == 0)
        {
            StartCoroutine(TargetKill());
        }
    }
    private IEnumerator TargetKill()
    {
        StopCoroutine(attackCoroutine);
        yield return new WaitForSeconds(0.5f);
        target = null;
    }
    private void ReceiveSkill(BigInteger calcedValue, SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Damage:
                hp = hp - calcedValue;
                if (hp < 0)
                    hp = 0;
                if (this is EnemyController)
                    anim.SetTrigger("Hit");
                break;
            case SkillType.Heal:
                break;
        }
        OnReceiveSkill();
        if (hp == 0)
        {
            OnDead();
        }
    }
    private void VisualEffectToTarget(List<Attackable> targets)
    {

    }
    private List<Attackable> GetTargets(SkillTarget range, int targetNum)
    {
        List<Attackable> result = new();
        switch (range)
        {
            default:
                result.Add(target);
                break;
        }
        return result;
    }

    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();

}