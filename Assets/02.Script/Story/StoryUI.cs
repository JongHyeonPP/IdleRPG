using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryUI : MonoBehaviour
{
    private VisualElement _root;
    private VisualElement _main;
    private Label _label;
    private Button _skipButton;
    private VisualElement _fadeElement;
    private float _fadeDuration = 2f;

    public System.Action OnSkip;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _main = _root.Q<VisualElement>("Main");
        _label = _root.Q<Label>("TextLabel");
        _skipButton = _root.Q<Button>("SkipButton");
        _fadeElement = _root.Q<VisualElement>("FadeElement");

        _skipButton.clickable.clicked += () => OnSkip?.Invoke();
    }

    public void ShowStoryUI()
    {
        _main.style.display = DisplayStyle.Flex;
    }

    public void HideStoryUI()
    {
        _main.style.display = DisplayStyle.None;
    }

    public void UpdateText(string talker, string text, Color color)
    {
        _label.style.color = color;
        _label.text = $"{talker}: {text}";
    }

    public IEnumerator FadeEffect(bool fadeIn)
    {
        float startOpacity = fadeIn ? 1 : 0;
        float endOpacity = fadeIn ? 0 : 1;
        float elapsedTime = 0f;

        _fadeElement.style.display = DisplayStyle.Flex;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float opacity = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / _fadeDuration);
            _fadeElement.style.opacity = opacity;
            yield return null;
        }

        _fadeElement.style.opacity = endOpacity;
        if (!fadeIn) _fadeElement.style.display = DisplayStyle.None;
    }
    public void SetStoryText(string talker, string text, Color color)
    {
        _label.text = $"{talker}: {text}";
        _label.style.color = color;
    }
}
