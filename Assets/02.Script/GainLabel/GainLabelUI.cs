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
                isActive = false,
                _goldTexture = _goldTexture,
                _expTexture = _expTexture,
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


}

