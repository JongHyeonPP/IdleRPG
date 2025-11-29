using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private StoryUIHandler uiHandler;

    private readonly Queue<IEnumerator> actions = new();

    public void AddAction(IEnumerator action)
    {
        actions.Enqueue(action);
    }

    public void Run()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        while (actions.Count > 0)
            yield return StartCoroutine(actions.Dequeue());

        // 스토리 → 배틀 전환
        BattleBroker.SwitchToBattle();

        // 페이드 실행 (코루틴 X)
        UIBroker.FadeInOut(0f, 0.5f, 2f);

        // ★ fadeInOut 총 2.5초 직접 기다림
        yield return new WaitForSeconds(0.5f + 2f);

        // ★ 페이드 완전히 끝난 순간 - 모델 위치 원위치 복구
        StoryManager.instance.ResetAllModelPositionsAfterStory();
    }


    public IEnumerator SetTalkerAction(StoryTalker talker)
    {
        uiHandler.SetTalker(talker);
        yield break;
    }

    public IEnumerator Dialogue(string text)
    {
        yield return uiHandler.ShowDialogueCoroutine(text);
    }

    public IEnumerator MoveWithAnim(GameObject target, Vector3 endPos, float duration, bool wait = true)
    {
        if (target == null)
            yield break;

        Animator anim = target.GetComponentInChildren<Animator>();

        if (anim != null)
        {
            anim.SetBool("Run", true);
            anim.SetBool("Idle", false);
        }

        Tween t = target.transform.DOMove(endPos, duration).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (anim != null)
            {
                anim.SetBool("Run", false);
                anim.SetBool("Idle", true);
            }
        });

        if (wait)
            yield return t.WaitForCompletion();
    }

    public IEnumerator Attack(GameObject target)
    {
        if (target == null)
            yield break;

        Animator anim = target.GetComponentInChildren<Animator>();
        if (anim == null)
            yield break;

        anim.ResetTrigger("Attack");
        anim.SetTrigger("Attack");

        float time = anim.GetCurrentAnimatorStateInfo(0).length;
        if (time < 0.1f) time = 0.4f;

        yield return new WaitForSeconds(time);
    }

    public IEnumerator FadeInOut(float fadeIn, float hold, float fadeOut)
    {
        yield return uiHandler.FadeInOut(fadeIn, hold, fadeOut);
    }

    public IEnumerator Delay(float sec)
    {
        yield return new WaitForSeconds(sec);
    }

    public void ResetUI()
    {
        uiHandler.ResetUI();
    }
}
