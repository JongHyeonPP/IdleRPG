using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ClickReceiver : MonoBehaviour
{
    [SerializeField] private StartManager _startManager;
    private Label _startLabel;
    private VisualElement _root;

    [Header("Blink Settings")]
    [SerializeField] private float fadeDuration = 1f; // 흐려지거나 밝아지는 데 걸리는 시간
    [SerializeField] private float holdDuration = 0.4f; // 완전히 밝거나 어두워졌을 때 멈추는 시간
    [SerializeField] private float minAlpha = 0.35f;
    [SerializeField] private float maxAlpha = 1f;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _startLabel = _root.Q<Label>("StartLabel");

        // 클릭 이벤트 등록
        _root.RegisterCallback<ClickEvent>(OnClickReceiver);

        // 초기 색상 설정
            _startLabel.style.color = new StyleColor(Color.white);

        StartCoroutine(SmoothBlinkWithPause());
    }

    private IEnumerator SmoothBlinkWithPause()
    {
        while (true)
        {
            yield return StartCoroutine(FadeText(maxAlpha, minAlpha, fadeDuration));
            yield return new WaitForSeconds(holdDuration); // 어두운 상태 유지

            yield return StartCoroutine(FadeText(minAlpha, maxAlpha, fadeDuration));
            yield return new WaitForSeconds(holdDuration); // 밝은 상태 유지
        }
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            Color color = Color.white;
            color.a = alpha;

            _startLabel.style.color = new StyleColor(color);
            yield return null;
        }
    }

    private void OnClickReceiver(ClickEvent evt)
    {
        _startManager.OnClickedStartImage();
        gameObject.SetActive(false);
    }
}
