using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        // �ʱ� ���� �� ����
        float startAlpha = _isFadeIn ? 0f : 1f;

        // ���� ���� �� ����
        float endAlpha;
        if (_targetAlpha != -1)
        {
            // targetAlpha�� ������ ���
            endAlpha = Mathf.Clamp01(_targetAlpha);
        }
        else
        {
            // targetAlpha�� �������� ���� ��� �⺻ ����
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

        // Ȯ���� alpha�� ������ ������ �ǵ��� ����
        originalColor.a = endAlpha;
        _ui.color = originalColor;

        // ���̵� �ƿ��� ������ �� UI�� ��Ȱ��ȭ
        if (!_isFadeIn && endAlpha == 0f)
        {
            _ui.gameObject.SetActive(false);
        }
    }
    public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        if (Enum.TryParse(value, true, out TEnum result))
        {
            return result;
        }
        Debug.LogWarning($"Invalid enum value '{value}' for type '{typeof(TEnum)}'. Returning default value.");
        return default;
    }
}