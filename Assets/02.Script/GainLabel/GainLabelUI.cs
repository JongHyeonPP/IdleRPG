using EnumCollection;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GainLabelUI : MonoBehaviour
{
    private GainLabelPiece[] pieceArr = new GainLabelPiece[5];
    public VisualElement root { private get; set; }
    [SerializeField] Texture2D _goldTexture;
    [SerializeField] Texture2D _expTexture;
    private readonly float _hideWait = 1f;
    private readonly float _hideDuration = 1f;
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var parent = root.Q<VisualElement>("Vertical");
        for (int i = 0; i < pieceArr.Length; i++)
        {
            VisualElement pieceRoot = parent.hierarchy.ElementAt(i);
            var iconElement = pieceRoot.Q<VisualElement>("IconElement");
            var valueLabel = pieceRoot.Q<Label>("ValueLabel");
            pieceRoot.style.opacity = 0f;
            pieceArr[i] = new GainLabelPiece(pieceRoot, iconElement, valueLabel)
            {
                isActive = false
            };
        }
        BattleBroker.OnCurrencyInBattle += OnCurrencyInBattle;
    }
    private IEnumerator GraduallyHideAlpha()
    {
        yield return new WaitForSeconds(_hideWait);
        for (int i = pieceArr.Length - 1; i >= 0; i--)
        {
            if (!pieceArr[i].isActive)
                continue;
            GainLabelPiece x = pieceArr[i];
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(x.GraduallyHidePiece());
        }
    }
    private void OnCurrencyInBattle(DropType dropType, int value)
    {
        StopAllCoroutines();
        foreach (var x in pieceArr)
        {
            x.root.style.opacity = 0f;
        }
        for (int i = pieceArr.Length - 1; i >= 1; i--)
        {
            if (!pieceArr[i - 1].isActive)
                continue;
            pieceArr[i - 1].GetValue(out DropType newType, out int newValue);
            pieceArr[i].SetValue(newType, newValue);
        }
        pieceArr[0].SetValue(dropType, value);
        StartCoroutine(GraduallyHideAlpha());
    }
    internal class GainLabelPiece
    {
        // Perm Value
        public bool isActive;
        public VisualElement root;
        private VisualElement _iconElement;
        private Label _valueLabel;
        // Temp Value
        private DropType _dropType;
        private int _value;
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

}

