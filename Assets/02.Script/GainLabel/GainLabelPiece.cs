using EnumCollection;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;

class GainLabelPiece
{
    // Perm Value
    public bool isActive;
    public VisualElement root;
    private VisualElement _iconElement;
    private Label _valueLabel;
    // Temp Value
    private DropType _dropType;
    private int _value;
    internal Texture2D _goldTexture;
    internal Texture2D _expTexture;
    private readonly float _hideDuration = 1f;

    internal GainLabelPiece(VisualElement root, VisualElement iconElement, Label valueLabel)
    {
        this.root = root;
        _iconElement = iconElement;
        _valueLabel = valueLabel;
    }

    internal void SetValue(DropType dropType, int value)
    {
        _dropType = dropType;
        _value = value;
        _valueLabel.text = value.ToString("N0");
        isActive = true;
        root.style.opacity = 1f;
        switch (dropType)
        {
            case DropType.Gold:
                _iconElement.style.backgroundImage = _goldTexture;
                break;
            case DropType.Exp:
                _iconElement.style.backgroundImage = _expTexture;
                break;
        }
    }

    internal void GetValue(out DropType dropType, out int value)
    {
        dropType = _dropType;
        value = _value;
    }

    internal IEnumerator GraduallyHidePiece()
    {
        if (!isActive)
            yield break;
        isActive = false;
        float elapsedTime = 0f;
        float initialOpacity = root.style.opacity.value;

        while (elapsedTime < _hideDuration)
        {
            elapsedTime += Time.deltaTime;
            float newOpacity = Mathf.Lerp(initialOpacity, 0f, elapsedTime / _hideDuration);
            root.style.opacity = newOpacity;
            yield return null;
        }
        root.style.opacity = 0f;

    }
}