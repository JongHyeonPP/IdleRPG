using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class Attackable : MonoBehaviour
{
    public Attackable target;
    protected float attackTerm = 1f;
    public Animator anim;
    protected int hp;
    //private Dictionary<> tempEffect = new();
    protected void SkillBehaviour(int skillValue, SkillType type, SkillRange range, int targetNum = 1, float preDelay = 0.5f, float postDelay=0.5f)
    {
        StartCoroutine(AnimBehaviour(skillValue, type, range, targetNum, preDelay, postDelay));
        
    }
    private IEnumerator AnimBehaviour(int skillValue, SkillType type, SkillRange range, int targetNum, float preDelay, float postDelay)
    {
        yield return new WaitForSeconds(preDelay);
        switch (type)
        {
            default:
                anim.SetTrigger("Attack");
                break;
        }
        List<Attackable> targets = GetTargets(range, targetNum);
        ActiveSkillToTarget(targets, skillValue, type);
        VisualEffectToTarget(targets);
        yield return new WaitForSeconds(postDelay);
    }
    private void ActiveSkillToTarget(List<Attackable> targets, int skillValue, SkillType skillType)
    {
        ICharacterStatus myStatus = GetStatus();
        int calcedValue = skillValue;
        calcedValue *= myStatus.Power;
        foreach (var target in targets)
        {
            ICharacterStatus targetStatus = target.GetStatus();
            //일련의 계산 진행
            target.ReceiveSkill(calcedValue, skillType);
        }
    }

    private void ReceiveSkill(int calcedValue, SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Damage:
                hp = Mathf.Max(hp-calcedValue, 0);
                Debug.Log(hp);
                break;
            case SkillType.Heal:
                break;
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
    protected abstract ICharacterStatus GetStatus();
}