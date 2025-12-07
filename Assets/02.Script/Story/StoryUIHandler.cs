using EnumCollection;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryUIHandler : MonoBehaviour, IGeneralUI
{
    [SerializeField] private float typeSpeed = 0.04f;

    private VisualElement root;
    private VisualElement rootChild;
    private Label nameLabel;
    private Label textLabel;

    private VisualElement playerRenderElement;
    private VisualElement[] companionRenderElement;
    private VisualElement otherRenderElement;

    private VisualElement fadePanel;

    private bool isTyping;
    private bool isWaitingClick;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        rootChild = root.Q<VisualElement>("StoryUI");
        nameLabel = root.Q<Label>("NameLabel");
        textLabel = root.Q<Label>("TextLabel");
        fadePanel = root.Q<VisualElement>("FadePanel");

        playerRenderElement = root.Q<VisualElement>("PlayerRender");
        otherRenderElement = root.Q<VisualElement>("OtherRender");

        companionRenderElement = new VisualElement[3];
        companionRenderElement[0] = root.Q<VisualElement>("CompanionRender_0");
        companionRenderElement[1] = root.Q<VisualElement>("CompanionRender_1");
        companionRenderElement[2] = root.Q<VisualElement>("CompanionRender_2");

        ResetUI();

        fadePanel.style.display = DisplayStyle.Flex;
        fadePanel.style.opacity = 1f;

        rootChild.RegisterCallback<ClickEvent>(OnClick);
    }

    // ============================================================
    //   CLICK 핸들러
    // ============================================================
    private void OnClick(ClickEvent evt)
    {
        // 타이핑 중이면 즉시 종료 플래그
        if (isTyping)
        {
            isTyping = false;   // → TypeText coroutine이 즉시 종료됨
            return;
        }

        // 타이핑이 끝난 뒤 클릭했을 때만 다음으로 넘어감
        if (isWaitingClick)
            isWaitingClick = false;
    }

    // ============================================================
    //   화자 설정
    // ============================================================
    public void SetTalker(StoryTalker talker)
    {
        nameLabel.text = talker.name;
        HideAllRenders();

        switch (talker.type)
        {
            case StoryRenderType.Player:
                playerRenderElement.style.display = DisplayStyle.Flex;
                break;

            case StoryRenderType.Companion0:
                companionRenderElement[0].style.display = DisplayStyle.Flex;
                break;

            case StoryRenderType.Companion1:
                companionRenderElement[1].style.display = DisplayStyle.Flex;
                break;

            case StoryRenderType.Companion2:
                companionRenderElement[2].style.display = DisplayStyle.Flex;
                break;

            case StoryRenderType.Other:
                otherRenderElement.style.display = DisplayStyle.Flex;
                break;
        }
    }

    // ============================================================
    //   대사 출력
    // ============================================================
    public IEnumerator ShowDialogueCoroutine(string text)
    {
        yield return TypeText(text);
        yield return WaitForNextClick();
    }

    // ============================================================
    //   타이핑 효과 + 클릭 시 즉시 전체 출력
    // ============================================================
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        textLabel.text = "";

        int length = text.Length;
        int index = 0;

        while (index < length)
        {
            // 클릭 시 즉시 전체 출력
            if (!isTyping)
            {
                textLabel.text = text; // 남은 텍스트 전체 출력
                break;
            }

            textLabel.text += text[index];
            index++;

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    private IEnumerator WaitForNextClick()
    {
        isWaitingClick = true;
        yield return new WaitUntil(() => !isWaitingClick);
    }

    // ============================================================
    //   페이드
    // ============================================================
    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        fadePanel.style.display = DisplayStyle.Flex;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanel.style.opacity = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        fadePanel.style.opacity = to;
    }

    public IEnumerator FadeInOut(float fadeInTime = 1.5f, float holdTime = 0.5f, float fadeOutTime = 1.5f)
    {
        fadePanel.style.display = DisplayStyle.Flex;

        if (fadeInTime > 0f)
            yield return Fade(0f, 1f, fadeInTime);

        yield return new WaitForSeconds(holdTime);

        if (fadeOutTime > 0f)
            yield return Fade(1f, 0f, fadeOutTime);

        fadePanel.style.display = DisplayStyle.None;
    }

    public IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    // ============================================================
    //   UI Reset
    // ============================================================
    public void ResetUI()
    {
        nameLabel.text = "";
        textLabel.text = "";

        HideAllRenders();

        isTyping = false;
        isWaitingClick = false;

        fadePanel.style.display = DisplayStyle.Flex;
        fadePanel.style.opacity = 1f;
    }

    // ============================================================
    //   모든 렌더 비활성화
    // ============================================================
    private void HideAllRenders()
    {
        playerRenderElement.style.display = DisplayStyle.None;
        otherRenderElement.style.display = DisplayStyle.None;

        for (int i = 0; i < companionRenderElement.Length; i++)
            companionRenderElement[i].style.display = DisplayStyle.None;
    }

    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnBoss()
    {
        root.style.display = DisplayStyle.None;
    }
}
