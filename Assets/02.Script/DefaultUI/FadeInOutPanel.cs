using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class FadeInOutPanel : MonoBehaviour
{
    private VisualElement _background;
    private Coroutine _fadeCoroutine;

    private void Start()
    {
        UIBroker.FadeInOut += FadeInOut;
        _background = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Background");
        _background.pickingMode = PickingMode.Ignore;
        // 초기값: 완전 투명한 검정
        _background.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
        _background.style.display = DisplayStyle.None;
    }

    private void FadeInOut(float fadeInDuration, float stayDuration, float fadeOutDuration)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(fadeInDuration, stayDuration, fadeOutDuration));
    }

    private IEnumerator FadeRoutine(float fadeInDuration, float stayDuration, float fadeOutDuration)
    {
        float elapsed = 0f;
        _background.style.display = DisplayStyle.Flex;

        // Fade In
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            _background.style.backgroundColor = new StyleColor(new Color(0, 0, 0, t));
            yield return null;
        }

        _background.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 1f));

        // Stay
        yield return new WaitForSeconds(stayDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            _background.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 1f - t));
            yield return null;
        }

        _background.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
        _background.style.display = DisplayStyle.None;
        _fadeCoroutine = null;
    }
}
