using DG.Tweening;
using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrencyAbsorbEffect : MonoBehaviour
{
    public static CurrencyAbsorbEffect instance;

    private UIDocument uiDoc;
    private VisualElement root;
    private Queue<VisualElement> pool = new();

    [SerializeField] private int poolSize = 10;
    [SerializeField] private Vector2 iconSize = new(64, 64);
    [SerializeField] private float flyDuration = 0.8f;
    [SerializeField] private float scatterRadius = 60f;

    // 자원 아이콘 스크린 좌표
    [SerializeField] private Vector2 goldPos = new(800f, 200f);
    [SerializeField] private Vector2 diaPos = new(1000f, 200f);
    [SerializeField] private Vector2 cloverPos = new(900f, 250f);

    private void Awake()
    {
        instance = this;
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        // 패널 스케일 모드 고정 (비율 보정)
        var panelSettings = uiDoc.panelSettings;
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = new Vector2Int(1080, 2400);
        panelSettings.match = 1f;

        InitPool();
    }

    private void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            VisualElement v = new VisualElement();
            v.style.position = Position.Absolute;
            v.style.width = iconSize.x;
            v.style.height = iconSize.y;
            v.style.display = DisplayStyle.None;
            root.Add(v);
            pool.Enqueue(v);
        }
    }

    private VisualElement GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        VisualElement v = new VisualElement();
        v.style.position = Position.Absolute;
        v.style.width = iconSize.x;
        v.style.height = iconSize.y;
        root.Add(v);
        return v;
    }

    private void ReturnToPool(VisualElement v)
    {
        v.style.display = DisplayStyle.None;
        pool.Enqueue(v);
    }

    // startScreenPos를 null로 두면 중앙에서 시작
    // startScreenPos를 null로 두면 중앙에서 시작
    public void PlayEffect(Resource resource, int amount, Vector3? startScreenPos = null)
    {
        if (CurrencyManager.instance == null) return;

        Sprite sprite = CurrencyManager.instance.GetResourceSprite(resource);
        if (sprite == null) return;

        Vector3 startScreen = startScreenPos ?? new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector2 startPos = ScreenToPanelFixed(root, startScreen); // 변환 필요

        // 이미 1080x2400 기준 좌표는 변환하지 않음
        Vector2 targetPos = resource switch
        {
            Resource.Gold => goldPos,
            Resource.Dia => diaPos,
            Resource.Clover => cloverPos,
            _ => new Vector2(540f, 100f)
        };

        int spawnCount = Mathf.Clamp(amount, 1, 8);
        for (int i = 0; i < spawnCount; i++)
        {
            VisualElement coin = GetFromPool();
            coin.style.display = DisplayStyle.Flex;
            coin.style.backgroundImage = new StyleBackground(sprite);
            coin.style.opacity = 1f; // 투명도 초기화

            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float spreadDist = Random.Range(scatterRadius * 0.7f, scatterRadius * 1.5f);
            Vector2 spreadPos = startPos + randomDir * spreadDist;
            coin.style.translate = new Translate(startPos.x, startPos.y);

            float delay = i * 0.05f + Random.Range(0f, 0.1f);

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);

            // 퍼짐
            seq.Append(DOTween.To(() => 0f, t =>
            {
                Vector2 cur = Vector2.Lerp(startPos, spreadPos, t);
                coin.style.translate = new Translate(cur.x, cur.y);
            }, 1f, 0.25f).SetEase(Ease.OutBack));

            seq.AppendInterval(0.1f);

            // 흡수 이동
            seq.Append(DOTween.To(() => 0f, t =>
            {
                Vector2 cur = Vector2.Lerp(spreadPos, targetPos, t);
                coin.style.translate = new Translate(cur.x, cur.y);
            }, 1f, flyDuration).SetEase(Ease.InOutCubic));

            // 도착 후 투명해지기 (0.2초 동안)
            seq.Append(DOTween.To(() => coin.style.opacity.value, x => coin.style.opacity = x, 0f, 0.2f));

            // 완전히 투명해진 후 풀로 반환
            seq.OnComplete(() => ReturnToPool(coin));
        }
    }

    // 핵심: 실제 패널 크기 기준으로 스크린 좌표 변환 (완벽 보정)
    private Vector2 ScreenToPanelFixed(VisualElement root, Vector3 screenPos)
    {
        Rect panelRect = root.worldBound;
        float scaleX = panelRect.width / Screen.width;
        float scaleY = panelRect.height / Screen.height;

        float x = screenPos.x * scaleX;
        float y = (Screen.height - screenPos.y) * scaleY; // 스크린 Y축 보정

        return new Vector2(x, y);
    }

    [ContextMenu("Test Gold Absorb")]
    private void TestGoldAbsorb()
    {
        PlayEffect(Resource.Gold, 20);
    }

    [ContextMenu("Test Dia Absorb")]
    private void TestDiaAbsorb()
    {
        PlayEffect(Resource.Dia, 20);
    }

    [ContextMenu("Test Clover Absorb")]
    private void TestCloverAbsorb()
    {
        PlayEffect(Resource.Clover, 20);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PlayEffect(Resource.Dia, 20);

        if (Input.GetKeyDown(KeyCode.W))
            PlayEffect(Resource.Clover, 20);
    }
}
