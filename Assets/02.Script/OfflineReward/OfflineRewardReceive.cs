using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineRewardReceive : MonoBehaviour
{
    public VisualElement root { get; private set; }
    [SerializeField] OfflineRewardUI _offlineRewardUi;
    private RewardResult _rewardResult;
    private VisualElement[] flares;
    private bool[] isFlickering;

    [SerializeField] private float flickerDuration = 0.5f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float flickerStartInterval = 0.5f;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;
        root.RegisterCallback<ClickEvent>(evt => OnClickUi());

        _rewardResult = (RewardResult)StartBroker.GetOfflineReward();
        if (_rewardResult != null)
        {
            root.style.display = DisplayStyle.Flex;
        }

        NetworkBroker.OnOfflineReward += () => root.style.display = DisplayStyle.None;

        VisualElement rootChild = root.Q<VisualElement>("AutoRewardReceive");
        flares = rootChild.Children().ToArray();
        isFlickering = new bool[flares.Length];
        foreach (var flare in flares)
        {
            flare.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0f));
        }
        StartCoroutine(SpawnFlickersLoop());
    }

    private void OnClickUi()
    {
        _offlineRewardUi.ActiveUi(_rewardResult);
    }

    private IEnumerator PulseAlpha(VisualElement element, int index)
    {
        float t = 0f;
        bool fadeIn = true;
        Color baseColor = new Color(1f, 1f, 1f, 0f);
        isFlickering[index] = true;

        while (true)
        {
            t += Time.deltaTime / flickerDuration;
            float alpha = fadeIn ? Mathf.Lerp(0f, 1f, t) : Mathf.Lerp(1f, 0f, t);
            element.style.unityBackgroundImageTintColor = new StyleColor(new Color(baseColor.r, baseColor.g, baseColor.b, alpha));

            if (t >= 1f)
            {
                t = 0f;
                fadeIn = !fadeIn;

                if (fadeIn)
                    yield return new WaitForSeconds(holdTime);
            }

            yield return null;
        }
    }

    public void StartFlickerEffect(int index)
    {
        if (index < 0 || index >= flares.Length) return;
        if (isFlickering[index]) return;

        StartCoroutine(PulseAlpha(flares[index], index));
    }

    private IEnumerator SpawnFlickersLoop()
    {
        while (true)
        {
            List<int> available = new();

            for (int i = 0; i < flares.Length; i++)
            {
                if (!isFlickering[i])
                    available.Add(i);
            }

            if (available.Count > 0)
            {
                int randIndex = available[Random.Range(0, available.Count)];
                StartFlickerEffect(randIndex);
            }

            yield return new WaitForSeconds(flickerStartInterval);
        }
    }
}
