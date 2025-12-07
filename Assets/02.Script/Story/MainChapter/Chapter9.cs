using EnumCollection;
using UnityEngine;

public class Chapter9 : StoryChapter
{
    [Header("NPC Models")]
    [SerializeField] private GameObject bushMonster;

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
        bushMonster
    };

    public override StoryRenderType[] GetRequiredRenderTypes()
    {
        return new StoryRenderType[]
        {
            StoryRenderType.Player,
            StoryRenderType.Companion0,
            StoryRenderType.Companion1,
            StoryRenderType.Companion2
        };
    }

    public override void BuildActions(StoryRunner runner)
    {
        StoryTalker player = StoryManager.instance.GetTalker(StoryRenderType.Player);
        StoryTalker archer = StoryManager.instance.GetTalker(StoryRenderType.Companion0);
        StoryTalker warrior = StoryManager.instance.GetTalker(StoryRenderType.Companion1);
        StoryTalker mage = StoryManager.instance.GetTalker(StoryRenderType.Companion2);

        GameObject p = StoryManager.instance.GetModel(StoryRenderType.Player);
        GameObject a = StoryManager.instance.GetModel(StoryRenderType.Companion0);
        GameObject w = StoryManager.instance.GetModel(StoryRenderType.Companion1);
        GameObject m = StoryManager.instance.GetModel(StoryRenderType.Companion2);

        Vector3 oP = p.transform.position;
        Vector3 oA = a.transform.position;
        Vector3 oW = w.transform.position;
        Vector3 oM = m.transform.position;

        Vector3 offset = new Vector3(-2.5f, 0f, 0f);

        p.transform.position = oP + offset;
        a.transform.position = oA + offset;
        w.transform.position = oW + offset;
        m.transform.position = oM + offset;

        // 네 명 같이 등장
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.1f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.8f, true));

        // 대사 시작
        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("잠깐. 뼈다귀를 따라오긴 했는데 결국 어디로 가는 거지."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("정령정원 깊은 곳."));
        runner.AddAction(runner.Dialogue("거기에 요미샘이 있어. 이 세계 흐름이 시작되는 자리야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("요미샘… 지금까지의 이상 현상이 전부 거기에서 새어나온 거구나."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("그럼 처음부터 거기로 가면 됐던 거 아닌가?"));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름이 너무 흔들려서 직선으로 가면 길이 계속 바뀌어."));
        runner.AddAction(runner.Dialogue("흐름이 안내하는 길을 따라가야 도착할 수 있어. 우리가 밟아 온 자리가 그 길이야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("그래서 이 숲부터 통과하는 거구나."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("여긴 흐름이 한 번 꺾였다 다시 이어지는 자리야."));
        runner.AddAction(runner.Dialogue("이 구간을 지나야 방향이 제대로 잡혀."));

        // 덤불 몬스터 등장
        Vector3 bushOrigin = bushMonster.transform.position;
        Vector3 bushStart = bushOrigin + new Vector3(4.0f, 0f, 0f);
        bushMonster.transform.position = bushStart;

        runner.AddAction(runner.MoveWithAnim(bushMonster, bushOrigin, 0.5f, true));
        runner.AddAction(runner.Attack(bushMonster));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("잠깐. 저건 왜 튀어나온 거야."));
        runner.AddAction(runner.Dialogue("움직임이 너무 요란한데."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름이 흔들린 자리에 생명체가 몰리는 건 자연스러운 일이야."));
        runner.AddAction(runner.Dialogue("특별한 존재는 아니지만, 이 구간을 지키는 덩어리 정도는 되겠지."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("결론은 치우고 가자는 거네."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("그래. 빨리 정리하고 계속 가자."));
        runner.AddAction(runner.Dialogue("목적지는 정령정원이니까."));
    }
}
