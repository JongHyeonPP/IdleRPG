using EnumCollection;
using UnityEngine;

public class Chapter15 : StoryChapter
{
    public override StoryTalker[] LocalTalkers => null;

    public override GameObject[] LocalModels => null;

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

        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("공기가 확 달라졌어."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("샘이 가까워질수록 흐름이 가라앉아."));
        runner.AddAction(runner.Dialogue("요동치던 기척이 정리되고 있어."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("이 정도면 거의 문 앞 아니냐."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("거기까지 가면 나는 어떻게 되는 거야."));
        runner.AddAction(runner.Dialogue("돌려보내지는 건지, 여기 남는 건지."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("샘은 이 세계가 자기 규칙을 다시 쓰는 자리야."));
        runner.AddAction(runner.Dialogue("널 쫓아낼지, 받아들일지, 그때 가서 정해지겠지."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("선택지를 나한테도 한 번쯤 물어봐 줬으면 좋겠는데."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("어디에 있든, 네가 사라지만 않는다면 그걸로 되는 거 아냐."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("그래."));
        runner.AddAction(runner.Dialogue("돌아가든 눌러앉든, 나중에 자랑할 썰 하나 생기는 거지."));
        runner.AddAction(runner.Dialogue("귀여워서 세계까지 흔들었다고."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("그 표현은 계속 들을수록 부끄럽네."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("한 가지는 확실해."));
        runner.AddAction(runner.Dialogue("이렇게까지 널 끌고 온 샘이, 이제 와서 버리는 쪽으로 흐름을 쓰진 않을 거야."));
        runner.AddAction(runner.Dialogue("그건 비용이 너무 크니까."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("결론. 샘까지 가서 직접 확인해 본다."));
        runner.AddAction(runner.Dialogue("그게 지금 우리가 할 수 있는 전부."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("맞아."));
        runner.AddAction(runner.Dialogue("거의 도착했으니까, 다들 좀만 더 힘내보자."));

    }
}
