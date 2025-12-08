using EnumCollection;
using UnityEngine;

public class Chapter13 : StoryChapter
{

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
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

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("잠깐만. 지금 갑자기 생각났다."));

        runner.AddAction(runner.Dialogue("아이돌 오디션 보러 갔었어."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("갑자기 오디션?"));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("하."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("진짜야."));
        runner.AddAction(runner.Dialogue("심사위원들이 귀엽다고 난리 나서 나도 같이 들떴는데…"));
        runner.AddAction(runner.Dialogue("그 순간 바닥이 쿵 하고 꺼졌고, 눈 떠 보니까 여기였어."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("그쪽이 반반하긴 하지"));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("내가 더 잘생겼는데..."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("그때 문이 열린 거구나."));
        runner.AddAction(runner.Dialogue("많은 시선이 너한테 한꺼번에 쏠리던 순간, 요미 흐름이 같이 흔들린 거야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("이 세계가 그 장면을 같이 봤다는 거네."));
        runner.AddAction(runner.Dialogue("그래서 나를 끌어온 거고..."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("기억은 중요한 열쇠야."));
        runner.AddAction(runner.Dialogue("계속 떠올려 봐. 샘 앞에 서게 되면, 그때 완전히 맞춰질 거야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("하하… 좀 부담스럽네."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("농담은 여기까지 하고, 이제 슬슬 가자."));
        runner.AddAction(runner.Dialogue("샘이 얼마 안 남은 것 같아."));

    }
}
