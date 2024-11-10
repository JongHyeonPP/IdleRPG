using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UtilityManager : MonoBehaviour
{
    public static bool CalculateProbability(float _probability)
    {
        return Random.Range(0f, 1f) <= Mathf.Clamp(_probability, 0f, 1f);
    }
    public static IEnumerator FadeUi(MaskableGraphic _ui, float _duration, bool _isFadeIn, float _targetAlpha = -1)
    {
        _ui.gameObject.SetActive(true);
        Color originalColor = _ui.color;

        // 초기 알파 값 설정
        float startAlpha = _isFadeIn ? 0f : 1f;

        // 최종 알파 값 결정
        float endAlpha;
        if (_targetAlpha != -1)
        {
            // targetAlpha가 설정된 경우
            endAlpha = Mathf.Clamp01(_targetAlpha);
        }
        else
        {
            // targetAlpha가 설정되지 않은 경우 기본 동작
            endAlpha = _isFadeIn ? 1f : 0f;
        }

        originalColor.a = startAlpha;
        _ui.color = originalColor;

        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / _duration);
            _ui.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 확실히 alpha가 설정된 값으로 되도록 보장
        originalColor.a = endAlpha;
        _ui.color = originalColor;

        // 페이드 아웃이 끝났을 때 UI를 비활성화
        if (!_isFadeIn && endAlpha == 0f)
        {
            _ui.gameObject.SetActive(false);
        }
    }
}
