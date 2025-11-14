using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryUIHandler : MonoBehaviour
{
    [SerializeField] private float _typeSpeed = 0.04f;

    private VisualElement _root;
    private VisualElement _rootChild;
    private Label _nameLabel;
    private Label _textLabel;
    private VisualElement _fadePanel;

    private bool _isTyping;
    private bool _isWaitingClick;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _rootChild = _root.Q<VisualElement>("StoryUI");
        _nameLabel = _root.Q<Label>("NameLabel");
        _textLabel = _root.Q<Label>("TextLabel");
        _fadePanel = _root.Q<VisualElement>("FadePanel");

        _fadePanel.style.opacity = 0f;
        _fadePanel.style.display = DisplayStyle.None;
        _nameLabel.text = "";
        _textLabel.text = "";

        _rootChild.RegisterCallback<ClickEvent>(OnClick);
        BattleBroker.SwitchToStory += (index) => { _root.style.display = DisplayStyle.Flex; };
        BattleBroker.SwitchToBattle += () => { _root.style.display = DisplayStyle.None; };
    }

    private void OnClick(ClickEvent evt)
    {
        if (_isWaitingClick)
            _isWaitingClick = false;
    }

    public IEnumerator ShowDialogueCoroutine(string talker, string text)
    {
        yield return TypeText(talker, text);
        yield return WaitForNextClick();
    }

    private IEnumerator TypeText(string talker, string text)
    {
        _isTyping = true;
        _nameLabel.text = talker;
        _textLabel.text = "";

        foreach (char c in text)
        {
            _textLabel.text += c;
            yield return new WaitForSeconds(_typeSpeed);
        }

        _isTyping = false;
    }

    private IEnumerator WaitForNextClick()
    {
        _isWaitingClick = true;
        yield return new WaitUntil(() => !_isWaitingClick);
    }

    // ===============================
    // FadeInOut: ¾îµÎ¿öÁü ¡æ À¯Áö ¡æ ¹à¾ÆÁü
    // ===============================
    public IEnumerator FadeInOut(float fadeOutTime = 1.5f, float holdTime = 0.5f, float fadeInTime = 1.5f)
    {
        _fadePanel.style.display = DisplayStyle.Flex;
        yield return Fade(0f, 1f, fadeOutTime); // ¾îµÎ¿öÁü
        yield return new WaitForSeconds(holdTime); // À¯Áö
        yield return Fade(1f, 0f, fadeInTime); // ¹à¾ÆÁü
        _fadePanel.style.display = DisplayStyle.None;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        _fadePanel.style.display = DisplayStyle.Flex;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _fadePanel.style.opacity = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _fadePanel.style.opacity = to;
    }

    public IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public void ShowDialogue(string talker, string text)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(talker, text));
    }
}
