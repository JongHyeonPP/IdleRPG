using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PopUpInBattle : MonoBehaviour
{
    private VisualElement _rootChlid;
    private Label _label;
    private Coroutine showCoroutine;

    [SerializeField] private Color _originalBgColor;
    [SerializeField] private Color _originalTextColor;

    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _rootChlid = root.Q<VisualElement>("PopUpInBattleUI");
        _label = root.Q<Label>();

        UIBroker.ShowPopUpInBattle += StartShowPopUpInBattle;

        _rootChlid.style.display = DisplayStyle.None;
        _label.style.display = DisplayStyle.None;
    }

    private void StartShowPopUpInBattle(string text)
    {
        _label.text = text;
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        _rootChlid.style.display = DisplayStyle.Flex;
        _label.style.display = DisplayStyle.Flex;

        // 원래 색상으로 초기화
        _rootChlid.style.backgroundColor = _originalBgColor;
        _label.style.color = _originalTextColor;

        _label.transform.scale = Vector3.one;

        showCoroutine = StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        yield return EmphasizeText();
        yield return new WaitForSeconds(0.7f);
        yield return FadeOutAlpha();
    }

    private IEnumerator EmphasizeText()
    {
        float scaleUpDuration = 0.1f;
        float scaleDownDuration = 0.1f;
        float maxScale = 1.05f;
        float normalScale = 1.0f;

        float elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleUpDuration;
            float scale = Mathf.Lerp(normalScale, maxScale, t);
            _label.transform.scale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < scaleDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDownDuration;
            float scale = Mathf.Lerp(maxScale, normalScale, t);
            _label.transform.scale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        _label.transform.scale = new Vector3(normalScale, normalScale, 1f);
    }

    private IEnumerator FadeOutAlpha()
    {
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float newAlphaBg = Mathf.Lerp(_originalBgColor.a, 0f, t);
            float newAlphaText = Mathf.Lerp(_originalTextColor.a, 0f, t);

            _rootChlid.style.backgroundColor = new Color(_originalBgColor.r, _originalBgColor.g, _originalBgColor.b, newAlphaBg);
            _label.style.color = new Color(_originalTextColor.r, _originalTextColor.g, _originalTextColor.b, newAlphaText);

            yield return null;
        }

        _rootChlid.style.backgroundColor = new Color(_originalBgColor.r, _originalBgColor.g, _originalBgColor.b, 0f);
        _label.style.color = new Color(_originalTextColor.r, _originalTextColor.g, _originalTextColor.b, 0f);

        _rootChlid.style.display = DisplayStyle.None;
        _label.style.display = DisplayStyle.None;
    }
}
