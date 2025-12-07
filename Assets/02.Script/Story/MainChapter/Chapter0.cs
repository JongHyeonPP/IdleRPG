using EnumCollection;
using UnityEngine;

public class Chapter0 : StoryChapter
{
    [Header("NPC Talker")]
    [SerializeField] private StoryTalker pigTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject bigPig;
    [SerializeField] private GameObject[] pigs;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { pigTalker };

    public override GameObject[] LocalModels
    {
        get
        {
            GameObject[] arr = new GameObject[1 + pigs.Length];
            arr[0] = bigPig;
            for (int i = 0; i < pigs.Length; i++)
                arr[i + 1] = pigs[i];
            return arr;
        }
    }

    public override StoryRenderType[] GetRequiredRenderTypes()
    {
        return new StoryRenderType[]
        {
            StoryRenderType.Player
        };
    }

    public override void BuildActions(StoryRunner runner)
    {
        StoryTalker protagonist = StoryManager.instance.GetTalker(StoryRenderType.Player);

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.5f));

        runner.AddAction(runner.Dialogue("어우… 머리 아파."));
        runner.AddAction(runner.Dialogue("여긴… 풀만 가득한 평원인데?"));
        runner.AddAction(runner.Dialogue("방금 전까지는 분명…"));
        runner.AddAction(runner.Dialogue("…생각하려니까 머리만 더 아프네."));
        runner.AddAction(runner.Dialogue("일단 눈앞부터 정리하자."));
        runner.AddAction(runner.Dialogue("하늘 맑고, 바람 솔솔 불고…"));
        runner.AddAction(runner.Dialogue("그리고… 저기 핑크색 덩어리들 뭐지."));

        Vector3 offset = new Vector3(5f, 0f, 0f);

        Vector3 bigO = bigPig.transform.position;
        bigPig.transform.position = bigO + offset;

        Vector3[] o = new Vector3[pigs.Length];
        for (int i = 0; i < pigs.Length; i++)
        {
            o[i] = pigs[i].transform.position;
            pigs[i].transform.position = o[i] + offset;
        }

        runner.AddAction(runner.MoveWithAnim(bigPig, bigO, 1.0f, true));

        for (int i = 0; i < pigs.Length; i++)
            runner.AddAction(runner.MoveWithAnim(pigs[i], o[i], 1.0f, false));

        runner.AddAction(runner.SetTalkerAction(pigTalker));
        runner.AddAction(runner.Dialogue("꾸웨엑…"));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("와, 움직이네. 잠깐만, 생각보다 많은데?"));

        runner.AddAction(runner.SetTalkerAction(pigTalker));
        runner.AddAction(runner.Dialogue("너. 처음 맡는 냄새다."));
        runner.AddAction(runner.Dialogue("여기 풀밭, 내 밥터다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("밥터가 뭔데. 해석이 전혀 안 되는데."));
        runner.AddAction(runner.Dialogue("설마 나를 메뉴에 넣을 생각은 아니겠지."));

        runner.AddAction(runner.SetTalkerAction(pigTalker));
        runner.AddAction(runner.Dialogue("시험 삼아 한 입 정도는."));

        runner.AddAction(runner.Attack(bigPig));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("진짜로 먹을 생각이었어?"));
        runner.AddAction(runner.Dialogue("좋아. 일단 저 핑크들부터 정리하고 생각하자."));
    }
}
