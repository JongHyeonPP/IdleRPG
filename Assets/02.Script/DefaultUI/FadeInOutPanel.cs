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
        _background.style.opacity = 0; // 초기값
    }

    private void FadeInOut(int fadeInDuration, int fadeOutDuration)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(fadeInDuration, fadeOutDuration));
    }

    private IEnumerator FadeRoutine(int fadeInDuration, int fadeOutDuration)
    {
        // Fade In
        float elapsed = 0f;
        float fadeInSeconds = fadeInDuration;
        while (elapsed < fadeInSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInSeconds);
            _background.style.opacity = t;
            yield return null;
        }

        _background.style.opacity = 1f;

        // Optional: 잠시 멈추고 싶으면 yield return new WaitForSeconds(holdTime);

        // Fade Out
        elapsed = 0f;
        float fadeOutSeconds = fadeOutDuration;
        while (elapsed < fadeOutSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutSeconds);
            _background.style.opacity = 1f - t;
            yield return null;
        }

        _background.style.opacity = 0f;
        _fadeCoroutine = null;
    }
}
