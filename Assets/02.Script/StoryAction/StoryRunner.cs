using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private StoryUIHandler _uiHandler;

    private readonly Queue<IEnumerator> _actions = new();

    public void AddAction(IEnumerator action)
    {
        _actions.Enqueue(action);
    }

    public void Run()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        while (_actions.Count > 0)
        {
            yield return StartCoroutine(_actions.Dequeue());
        }

        _uiHandler.ShowDialogue("시스템", "스토리가 종료되었습니다.");
        Debug.Log("스토리 종료");
    }

    // =============================
    // 실제 연출 동작
    // =============================

    public IEnumerator Dialogue(string talker, string text)
    {
        yield return _uiHandler.ShowDialogueCoroutine(talker, text);
    }

    public IEnumerator Move(GameObject target, Vector3 direction, float speed, float duration)
    {
        if (target == null)
        {
            Debug.LogWarning("[이동 실패] 대상 오브젝트가 null입니다.");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.transform.position += direction.normalized * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // =============================
    // 페이드 통합 (FadeInOut)
    // =============================

    public IEnumerator FadeInOut(float fadeOutTime = 1.5f, float holdTime = 0.5f, float fadeInTime = 1.5f)
    {
        yield return _uiHandler.FadeInOut(fadeOutTime, holdTime, fadeInTime);
    }

    // =============================
    // 단순 대기
    // =============================

    public IEnumerator Delay(float seconds)
    {
        yield return _uiHandler.Wait(seconds);
    }
}
