using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class StageClearPopUp : MonoBehaviour
{
    public VisualElement root { private set; get; }
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.opacity = 0;
        root.style.display = DisplayStyle.None;

        UIBroker.PopUpStageClear += PopUpStageClear;
    }

    private void PopUpStageClear()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeInOutRoutine());
    }

    private IEnumerator FadeInOutRoutine()
    {
        root.style.display = DisplayStyle.Flex;

        // Fade In
        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            root.style.opacity = t;
            yield return null;
        }

        root.style.opacity = 1f;

        // Stay visible for a short time
        yield return new WaitForSeconds(1.5f);

        // Fade Out
        float fadeOutDuration = 0.5f;
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            root.style.opacity = 1f - t;
            yield return null;
        }

        root.style.opacity = 0f;
        root.style.display = DisplayStyle.None;
    }
}
