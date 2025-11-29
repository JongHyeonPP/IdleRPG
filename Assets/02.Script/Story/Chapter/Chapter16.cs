using EnumCollection;
using UnityEngine;

public class Chapter16 : StoryChapter
{
    [Header("Spring Talker")]
    [SerializeField] private StoryTalker springTalker;

    [Header("Spring Object")]
    [SerializeField] private GameObject worldSpring;
    
    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
        springTalker
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
        worldSpring
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

        // 샘 앞 상황
        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("여기가 샘이야."));
        runner.AddAction(runner.Dialogue("공기부터 아까까지랑 완전히 달라."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("응."));
        runner.AddAction(runner.Dialogue("이 세계의 샘."));
        runner.AddAction(runner.Dialogue("모든 요미 흐름이 시작되는 자리야."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("조용한데 괜히 등골이 서늘하다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("누가 바로 옆에서 보고 있는 느낌이야."));

        // 샘이 말 걸기 시작
        runner.AddAction(runner.SetTalkerAction(springTalker));
        runner.AddAction(runner.Dialogue("보았다."));
        runner.AddAction(runner.Dialogue("저 너머에서 모두가 널 바라보던 그 순간도."));
        runner.AddAction(runner.Dialogue("그때 흐름이 흔들렸고, 나는 너를 원했다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("…샘이 직접 말하는 거네."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("샘의 의지가 잠깐 목소리를 낸 거야."));
        runner.AddAction(runner.Dialogue("이제 결론을 들려주려는 거지."));

        // 결정 전달
        runner.AddAction(runner.SetTalkerAction(springTalker));
        runner.AddAction(runner.Dialogue("그래서 데려왔다."));
        runner.AddAction(runner.Dialogue("귀여워서."));
        runner.AddAction(runner.Dialogue("눈을 뗄 수 없어서."));
        runner.AddAction(runner.Dialogue("그 순간 나의 규칙도 함께 틀어졌다."));
        runner.AddAction(runner.Dialogue("이제 결정했다."));
        runner.AddAction(runner.Dialogue("너를 돌려보내지 않는다."));
        runner.AddAction(runner.Dialogue("쫓아내지도 않는다."));
        runner.AddAction(runner.Dialogue("너를 이 세계의 흐름 안에 둔다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("잠깐만."));
        runner.AddAction(runner.Dialogue("그 말은 내가 여기서 계속 산다는 거지."));
        runner.AddAction(runner.Dialogue("돌아갈 방법은 없는 거야."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("널 되돌리려면 흐름을 다시 크게 비틀어야 해."));
        runner.AddAction(runner.Dialogue("이번보다 더 큰 금이 갈지도 몰라."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("나 한 사람 돌려보겠다고 또 누가 이렇게 되는 건 싫어."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("그래서 남겠다는 거네."));
        runner.AddAction(runner.Dialogue("네가 고른 쪽이야."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("게다가 혼자가 아니잖아."));
        runner.AddAction(runner.Dialogue("넘어지면 잡아 줄 사람 셋이나 있는데."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("샘이 널 받아들였으니까 이제부터는 책임도 같이 나눠 가지는 거야."));

        // 앞으로 전투가 계속되는 이유
        runner.AddAction(runner.SetTalkerAction(springTalker));
        runner.AddAction(runner.Dialogue("한 번 흔들린 흐름의 자국은 남는다."));
        runner.AddAction(runner.Dialogue("곳곳에서 작은 파동이 계속 일어날 것이다."));
        runner.AddAction(runner.Dialogue("그 파동이, 네가 앞으로 마주칠 요란한 것들의 이유가 된다."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("큰 붕괴는 막았지만 찌꺼기는 남아 있어."));
        runner.AddAction(runner.Dialogue("정리할 일은 아직 많을 거야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("결국 몬스터는 계속 나온다는 말이네."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("좋지."));
        runner.AddAction(runner.Dialogue("심심할 틈은 없겠다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("흐름을 흔든 장본인이라면, 끝까지 같이 정리하는 게 맞겠지."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("그럼 앞으로도 잘 부탁해."));
        runner.AddAction(runner.Dialogue("어디로 가든 따라갈게."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("다음 몬스터는 어디에 있나."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("좋아."));
        runner.AddAction(runner.Dialogue("어차피 여기까지 와 버렸으니까, 끝까지 같이 해보자."));


    }
}
