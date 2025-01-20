using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements;
public class FadeManager : MonoBehaviour
{
    private VisualElement _fadeOverlay;
    private float _fadeDuration = 2f;
    private int _fadeCycles = 2;
    public StoryManager storyManager;
    private void Start()
    {
       
        var root = GetComponent<UIDocument>().rootVisualElement;

        _fadeOverlay = root.Q<VisualElement>("FadeElement");

        StartCoroutine(FadeEffect());
    }
    private IEnumerator FadeEffect()
    {
        for (int i = 0; i < _fadeCycles; i++)
        {
            yield return StartCoroutine(Fade(1, 0.3f));
            yield return StartCoroutine(Fade(0.3f, 1));
        }

        yield return StartCoroutine(Fade(1, 0));
        yield return StartCoroutine(storyManager.FirstStoryStart());
    }
    private IEnumerator Fade(float startOpacity, float endOpacity)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration / 2) 
        {
            elapsedTime += Time.deltaTime;

            float opacity = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / (_fadeDuration / 2));
            _fadeOverlay.style.opacity = opacity;

            yield return null; 
        }

        _fadeOverlay.style.opacity = endOpacity;
    }
}
