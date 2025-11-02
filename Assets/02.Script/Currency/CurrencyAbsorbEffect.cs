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
    public void PlayEffect(Resource resource, int amount, Vector3? startScreenPos = null)
    {
        if (CurrencyManager.instance == null) return;

        Sprite sprite = CurrencyManager.instance.GetResourceSprite(resource);
        if (sprite == null) return;

        // 시작 좌표: 없으면 화면 중앙
        Vector3 startScreen = startScreenPos ?? new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector2 startPos = RuntimePanelUtils.ScreenToPanel(root.panel, startScreen);

        // 리소스 종류별 도착 좌표 선택
        Vector2 targetScreenPos = resource switch
        {
            Resource.Gold => goldPos,
            Resource.Dia => diaPos,
            Resource.Clover => cloverPos,
            _ => new Vector2(Screen.width * 0.5f, 100f)
        };
        Vector2 targetPos = RuntimePanelUtils.ScreenToPanel(root.panel, targetScreenPos);

        // 여러 개 생성
        int spawnCount = Mathf.Clamp(amount, 1, 8);
        for (int i = 0; i < spawnCount; i++)
        {
            VisualElement coin = GetFromPool();
            coin.style.display = DisplayStyle.Flex;
            coin.style.backgroundImage = new StyleBackground(sprite);

            // 퍼지는 방향 계산
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float spreadDist = Random.Range(scatterRadius * 0.7f, scatterRadius * 1.5f);
            Vector2 spreadPos = startPos + randomDir * spreadDist;
            coin.style.translate = new Translate(startPos.x, startPos.y);

            float delay = i * 0.05f + Random.Range(0f, 0.1f);

            // 시퀀스: 퍼짐 → 정지 → 흡수
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);

            seq.Append(DOTween.To(() => 0f, t =>
            {
                Vector2 cur = Vector2.Lerp(startPos, spreadPos, t);
                coin.style.translate = new Translate(cur.x, cur.y);
            }, 1f, 0.25f).SetEase(Ease.OutBack));

            seq.AppendInterval(0.1f);

            seq.Append(DOTween.To(() => 0f, t =>
            {
                Vector2 cur = Vector2.Lerp(spreadPos, targetPos, t);
                coin.style.translate = new Translate(cur.x, cur.y);
            }, 1f, flyDuration).SetEase(Ease.InOutCubic));

            seq.OnComplete(() => ReturnToPool(coin));
        }
    }

    [ContextMenu("Test Absorb Effect")]
    private void TestAbsorbEffect()
    {
        // 테스트용: 중앙에서 골드 8개가 위로 빨려감
        PlayEffect(Resource.Gold, 20);
    }
}
