using EnumCollection;
using UnityEngine;

public class Chapter11 : StoryChapter
{
    [Header("NPC Models")]
    [SerializeField] private GameObject deerMonster;

    public override GameObject[] LocalModels => new GameObject[]
    {
        deerMonster
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

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("여긴 다른 지역보다 흐름이 고요해."));
        runner.AddAction(runner.Dialogue("움직임이 모여드는 자리라서, 네 반응도 더 또렷하게 보일 거야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("근데 뭔가 숨어서 계속 쳐다보는 느낌이야."));
        runner.AddAction(runner.Dialogue("등이 간질간질해."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("긴장 풀지 마."));
        runner.AddAction(runner.Dialogue("금방 뭐 하나 튀어나오겠다."));

        Vector3 d0 = deerMonster.transform.position;
        deerMonster.transform.position = d0 + new Vector3(3f, 0, 0);

        runner.AddAction(runner.MoveWithAnim(deerMonster, d0, 0.6f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("우와. 갑자기 나타났어."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("내가 더 잘생겼는데 왜 쟤한테만 시선이 쏠리냐."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("뭐라는거야…"));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("잠깐만. 방금 뭔가 떠오르려다가 사라졌어."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("기억이 조금씩 돌아오는 거야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("잡으려 하면 흩어지는데, 느낌은 있어."));
        runner.AddAction(runner.Dialogue("아주 중요한 장면 같은데 선이 끊긴 느낌."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("여긴 네 반응이 특히 강하게 움직여."));
        runner.AddAction(runner.Dialogue("조금 더 들어가면 흐름이 다시 흔들릴 거야."));
        runner.AddAction(runner.Dialogue("그때 더 많이 떠오를지도 몰라."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("좋아. 계속 가보자."));
        runner.AddAction(runner.Dialogue("기억이든 뭐든, 이제는 끝까지 봐야겠네."));

    }
}
