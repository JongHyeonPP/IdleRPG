using EnumCollection;
using UnityEngine;

public class Chapter12 : StoryChapter
{
    [Header("NPC Models")]
    [SerializeField] private GameObject spider1;
    [SerializeField] private GameObject spider2;

    public override GameObject[] LocalModels => new GameObject[]
    {
        spider1,
        spider2
    };

    public override StoryTalker[] LocalTalkers => null;

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

        Vector3 offset = new Vector3(-2.5f, 0, 0);

        p.transform.position = oP + offset;
        a.transform.position = oA + offset;
        w.transform.position = oW + offset;
        m.transform.position = oM + offset;

        runner.AddAction(runner.FadeInOut(0, 0, 1.2f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("여긴 밟을 때마다 소리가 찔컥거려."));
        runner.AddAction(runner.Dialogue("바닥이 진짜 싫은 타입이다."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("발만 안 빠지면 괜찮다."));
        runner.AddAction(runner.Dialogue("발 빠지면 그때 생각하고."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("공기가 꽤 눅눅한데. 숨이 좀 무거워."));

        Vector3 s1o = spider1.transform.position;
        spider1.transform.position = s1o + new Vector3(3f, 0, 0);
        runner.AddAction(runner.MoveWithAnim(spider1, s1o, 0.5f, true));

        Vector3 s2o = spider2.transform.position;
        spider2.transform.position = s2o + new Vector3(3f, 0, 0);
        runner.AddAction(runner.MoveWithAnim(spider2, s2o, 0.45f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("헉. 갑자기 튀어나오네."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("성격이 급한 놈이다."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("거미가 거미줄에 안 있고 밖을 돌아다니는 건 조금 이상해."));
        runner.AddAction(runner.Dialogue("정상적인 움직임이 아니야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("거미 진짜 싫어!!"));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("정리하고 지나가자."));
        runner.AddAction(runner.Dialogue("이 구간은 오래 머물 자리가 아니다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("응. 빨리 통과하자."));

    }
}
