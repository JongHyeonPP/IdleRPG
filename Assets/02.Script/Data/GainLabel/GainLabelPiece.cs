using EnumCollection;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;
using System;

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
    private string _id;
    internal Texture2D _goldTexture;
    internal Texture2D _expTexture;
    private readonly float _hideDuration = 1f;

    internal GainLabelPiece(VisualElement root, VisualElement iconElement, Label valueLabel)
    {
        this.root = root;
        _iconElement = iconElement;
        _valueLabel = valueLabel;
    }

    internal void SetValue(DropType dropType, int value, string id)
    {
        _dropType = dropType;
        _value = value;
        _valueLabel.text = $"+{value:N0}";
        isActive = true;
        root.style.opacity = 1f;
        if (id != null)
            _id = id;
        switch (dropType)
        {
            case DropType.Gold:
                _iconElement.style.backgroundImage = _goldTexture;
                _iconElement.style.scale = Vector2.one;
                break;
            case DropType.Exp:
                _iconElement.style.backgroundImage = _expTexture;
                _iconElement.style.scale = Vector2.one;
                break;
            case DropType.Fragment:
                Rarity rarity = Enum.Parse<Rarity>(id);
                _iconElement.style.backgroundImage = new(CurrencyManager.instance.fragmentSprites[(int)rarity]);
                _iconElement.style.scale = Vector2.one;
                break;
            case DropType.Weapon:
                WeaponManager.instance.SetWeaponIconToVe(WeaponManager.instance.weaponDict[id], _iconElement);
                break;
        }
    }

    internal void GetValue(out DropType dropType, out int value, out string id)
    {
        dropType = _dropType;
        value = _value;
        id = _id;
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