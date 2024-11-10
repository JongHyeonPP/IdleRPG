using EnumCollection;
using PlasticGui.WorkspaceWindow.PendingChanges;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attackable : MonoBehaviour
{
    public Attackable target;
    protected float attackTerm = 1f;
    public Animator anim;
    private int hp;
    //private Dictionary<> tempEffect = new();
    protected void SkillBehaviour(int value, SkillType type, SkillRange range, int targetNum = 1, float preDelay = 0.5f, float postDelay=0.5f)
    {
        StartCoroutine(AnimBehaviour(type, preDelay, postDelay));
        List<Attackable> targets = GetTargets(range, targetNum);
        ActiveSkillToTarget(targets);
        VisualEffectToTarget(targets);
    }
    private IEnumerator AnimBehaviour(SkillType type, float preDelay, float postDelay)
    {
        yield return new WaitForSeconds(preDelay);
        switch (type)
        {
            default:
                anim.SetTrigger("Attack");
                break;
        }
        yield return new WaitForSeconds(postDelay);
    }
    private void ActiveSkillToTarget(List<Attackable> targets)
    {
        foreach (var x in targets)
        {
            ICharacterStatus myStatus = GetStatus();
            ICharacterStatus targetStatus = x.GetStatus();
            //일련의 계산 진행

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