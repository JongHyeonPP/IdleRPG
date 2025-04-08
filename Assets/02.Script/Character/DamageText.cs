using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageText : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private VisualElement _rootChild;
    private Label _damageLabel;

    public Action<DamageText> returnAction;

    public int index;//Debug를 위함
    private Coroutine coroutine;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = root.Q<VisualElement>("DamageText");
        root.style.height = Length.Percent(100);
        root.style.justifyContent = Justify.Center;
        _damageLabel = root.Q<Label>();
        root.style.alignContent = Align.Center;
        root.style.alignItems = Align.Center;
    }

    public void SetActive(bool isActive)
    {
        root.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void StartShowText(Vector3 screenPos, string text)
    {
        _damageLabel.text = text;
        StartCoroutine(DelayAndSetPosition(screenPos));

        if (coroutine != null)
        {
            Debug.LogError("Error");
        }
        coroutine =  StartCoroutine(AnimateTextSequence());
    }
    private IEnumerator DelayAndSetPosition(Vector2 screenPos)
    {
        // 텍스트가 실제 레이아웃을 가진 상태가 될 때까지 대기
        while (_rootChild.resolvedStyle.width == 0 ||
               _rootChild.resolvedStyle.height == 0 ||
               float.IsNaN(_rootChild.resolvedStyle.width) ||
               float.IsNaN(_rootChild.resolvedStyle.height))
        {
            yield return null; // 한 프레임 대기
        }


        // 텍스트의 실제 너비/높이 가져오기
        float width = _rootChild.resolvedStyle.width;

        // 중심 맞춤: 좌표에서 절반만큼 빼줌
        float adjustedX = screenPos.x - (width * 0.5f);
        float adjustedY = screenPos.y + 120f;

        _rootChild.style.left = adjustedX;
        _rootChild.style.bottom = adjustedY;
        SetOpacity(1f);
    }


    private IEnumerator AnimateTextSequence()
    {
        yield return StartCoroutine(AnimateScale(2f, 0.15f, 0.05f));
        yield return StartCoroutine(AnimateMoveAndFade(300f, 0.4f, 0.5f));

        SetActive(false);
        ResetStyle(); // 스타일 초기화
        coroutine = null;
        returnAction(this);
    }
    private void ResetStyle()
    {
        root.style.opacity = 1f;
        _damageLabel.style.bottom = StyleKeyword.Null; // 초기값 제거 (또는 0f)
        _damageLabel.style.scale = Vector2.one;
        _rootChild.style.left = StyleKeyword.Null;
        _damageLabel.text = "";
    }

    private IEnumerator AnimateScale(float peakScale, float growDuration, float shrinkDuration)
    {
        float elapsed = 0f;

        // 1단계: 커지기
        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            float currentScale = Mathf.Lerp(1f, peakScale, t);
            _damageLabel.style.scale = new Vector2(currentScale, currentScale);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _damageLabel.style.scale = new Vector2(peakScale, peakScale);

        elapsed = 0f;

        // 2단계: 작아지기
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            float currentScale = Mathf.Lerp(peakScale, 1f, t);
            _damageLabel.style.scale = new Vector2(currentScale, currentScale);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _damageLabel.style.scale = Vector2.one;
    }

    private IEnumerator AnimateMoveAndFade(float moveDistance, float duration, float fadeStartRatio)
    {
        float elapsed = 0f;

        float startY = root.resolvedStyle.bottom;
        float endY = startY + moveDistance;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Ease-In 적용: 느리게 시작해서 점점 빨라짐
            float easedT = Mathf.Pow(t, 3f);

            // 위치 이동 (가속 효과)
            float currentY = Mathf.Lerp(startY, endY, easedT);;
            _damageLabel.style.bottom = currentY;

            // 흐려지기 (t는 원래 시간 비율 기준이어야 정확하므로 여긴 그대로 사용)
            if (t > fadeStartRatio)
            {
                float fadeT = (t - fadeStartRatio) / (1f - fadeStartRatio);
                float currentOpacity = Mathf.Lerp(1f, 0f, fadeT);
                root.style.opacity = currentOpacity;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        root.style.bottom = endY;
    }

    public void SetOpacity(float opacity)
    {
        root.style.opacity = opacity;
    }
}
