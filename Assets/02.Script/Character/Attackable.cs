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
    protected EquipedSkill[] skillInBattleArr = new EquipedSkill[10];
    private void DefaultAttack()
    {
        SkillBehaviour(1, SkillType.Damage, SkillRange.Target);
    }
    private void ActiveSkill(string _skillName, float _value, float _range, float type)
    {
        Debug.Log("Skill : " + _skillName);
        SkillBehaviour(1, SkillType.Damage, SkillRange.Target);
    }
    protected void SkillBehaviour(int skillValue, SkillType type, SkillRange range, int targetNum = 1, float preDelay = 0.2f, float postDelay = 0.2f)
    {
        StartCoroutine(SkillBehaviorCoroutine(skillValue, type, range, targetNum, preDelay, postDelay));
    }
    private IEnumerator SkillBehaviorCoroutine(int skillValue, SkillType type, SkillRange range, int targetNum, float preDelay, float postDelay)
    {

        switch (type)
        {
            default:
                anim.SetTrigger("Attack");
                break;
        }
        List<Attackable> targets = GetTargets(range, targetNum);
        StartCoroutine(ActiveSkillToTarget(targets, skillValue, type, preDelay));
        VisualEffectToTarget(targets);
        yield return new WaitForSeconds(postDelay);
    }
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackRoop());
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
    private IEnumerator ActiveSkillToTarget(List<Attackable> targets, int skillValue, SkillType skillType, float preDelay)
    {
        yield return new WaitForSeconds(preDelay);
        ICharacterStatus myStatus = GetStatus();
        BigInteger calcedValue = skillValue;
        calcedValue *= myStatus.Power;
        foreach (var target in targets)
        {
            ICharacterStatus targetStatus = target.GetStatus();
            //일련의 계산 진행
            target.ReceiveSkill(calcedValue, skillType);
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
                anim.SetTrigger("Hit");
                break;
            case SkillType.Heal:
                break;
        }
        OnReceiveDamage();
        if (hp == 0)
        {
            OnDead();
        }
    }
    private void VisualEffectToTarget(List<Attackable> targets)
    {

    }
    private List<Attackable> GetTargets(SkillRange range, int targetNum)
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
    //AttackTerm 간격마다 우선 순위에 있는 스킬 사용
    protected IEnumerator AttackRoop()
    {
        ICharacterStatus _status = GetStatus();
        while (true)
        {
            bool isActiveSkill = false;
            foreach (EquipedSkill skill in skillInBattleArr)
            {
                if (skill == null)
                    continue;
                if (skill.IsSkillAble)
                {
                    SkillData data = skill.skillData;
                    ActiveSkill(data.name, data.value[0], data.range, data.range);
                    isActiveSkill = true;
                    break;
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
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveDamage();

}