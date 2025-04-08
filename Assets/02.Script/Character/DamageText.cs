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

    public int index;//Debug�� ����
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
        // �ؽ�Ʈ�� ���� ���̾ƿ��� ���� ���°� �� ������ ���
        while (_rootChild.resolvedStyle.width == 0 ||
               _rootChild.resolvedStyle.height == 0 ||
               float.IsNaN(_rootChild.resolvedStyle.width) ||
               float.IsNaN(_rootChild.resolvedStyle.height))
        {
            yield return null; // �� ������ ���
        }


        // �ؽ�Ʈ�� ���� �ʺ�/���� ��������
        float width = _rootChild.resolvedStyle.width;

        // �߽� ����: ��ǥ���� ���ݸ�ŭ ����
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
        ResetStyle(); // ��Ÿ�� �ʱ�ȭ
        coroutine = null;
        returnAction(this);
    }
    private void ResetStyle()
    {
        root.style.opacity = 1f;
        _damageLabel.style.bottom = StyleKeyword.Null; // �ʱⰪ ���� (�Ǵ� 0f)
        _damageLabel.style.scale = Vector2.one;
        _rootChild.style.left = StyleKeyword.Null;
        _damageLabel.text = "";
    }

    private IEnumerator AnimateScale(float peakScale, float growDuration, float shrinkDuration)
    {
        float elapsed = 0f;

        // 1�ܰ�: Ŀ����
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

        // 2�ܰ�: �۾�����
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

            // Ease-In ����: ������ �����ؼ� ���� ������
            float easedT = Mathf.Pow(t, 3f);

            // ��ġ �̵� (���� ȿ��)
            float currentY = Mathf.Lerp(startY, endY, easedT);;
            _damageLabel.style.bottom = currentY;

            // ������� (t�� ���� �ð� ���� �����̾�� ��Ȯ�ϹǷ� ���� �״�� ���)
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
