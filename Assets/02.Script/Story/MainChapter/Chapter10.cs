using EnumCollection;
using UnityEngine;

public class Chapter10 : StoryChapter
{

    [Header("NPC Models")]
    [SerializeField] private GameObject flowerMonster_0;
    [SerializeField] private GameObject flowerMonster_1;

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
        flowerMonster_0,
        flowerMonster_1
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

        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("여긴 흐름이 심하게 뒤틀렸어."));
        runner.AddAction(runner.Dialogue("공기부터 이상하다. 너무 조용한데, 안쪽에서만 요동치고 있어."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("저 꽃들 봐."));
        runner.AddAction(runner.Dialogue("바람도 안 부는데 혼자 흔들려. 일부러 우리를 향해 흔드는 것 같은데."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("숲이 우리를 내보내려고 발악하는 거 아니냐."));



        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("지금도 일부러 피한 것 같지 않아. 타이밍이 딱 맞아."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("네 주변에서만 흐름이 잠깐 잠잠해져."));
        runner.AddAction(runner.Dialogue("아주 특이한 반응이야."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("그래서 묻는 거야."));
        runner.AddAction(runner.Dialogue("너는 어디서 온 거지."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("눈 떴을 때 이미 여기였어."));
        runner.AddAction(runner.Dialogue("전에 뭐 했는지도 흐릿해. 잡으려 하면 도망가는 느낌이야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("기억도 안 난다는 거네."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름이 너한테서 다르게 움직여."));
        runner.AddAction(runner.Dialogue("이 세계의 규칙에서 살짝 비켜 서 있는 느낌이야."));
        runner.AddAction(runner.Dialogue("아직 뭐라 단정할 순 없으니 조금 더 관찰해 볼게."));

        Vector3 f0 = flowerMonster_0.transform.position;
        flowerMonster_0.transform.position = f0 + new Vector3(1f, 0f, 0f);
        runner.AddAction(runner.MoveWithAnim(flowerMonster_0, f0, 0.35f, false));
        Vector3 f2 = flowerMonster_1.transform.position;
        flowerMonster_1.transform.position = f2 + new Vector3(1f, 0f, 0f);
        runner.AddAction(runner.MoveWithAnim(flowerMonster_1, f2, 0.3f, false));
        runner.AddAction(runner.Attack(flowerMonster_1));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("뭐지. 맞고 싶은 건가. 앞으로 달려 나오는 것 봐라."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("정령정원에 도착하면 실마리를 찾을 수 있을 거야."));
        runner.AddAction(runner.Dialogue("네가 왜 여기 있는지도 같이 드러나겠지."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("내가 왜 여기 왔는지도."));
        runner.AddAction(runner.Dialogue("그때쯤이면 나도 알고 싶다."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("아무튼 길부터 뚫자."));
        runner.AddAction(runner.Dialogue("서 있으면 또 뭐가 튀어나온다."));
    }
}
