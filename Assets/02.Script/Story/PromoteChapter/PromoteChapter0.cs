using EnumCollection;
using UnityEngine;

public class PromoteChapter0 : StoryChapter
{
    [Header("Supervisor Talker")]
    [SerializeField] private StoryTalker supervisorTalker;

    [Header("Supervisor Model")]
    [SerializeField] private GameObject supervisorModel;

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
        supervisorTalker
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
        supervisorModel
    };

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
        StoryTalker supervisor = supervisorTalker;

        // 페이드 인
        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.0f));

        // 플레이어
        runner.AddAction(runner.Dialogue("여기가… 승급 시험장이구나."));
        runner.AddAction(runner.Dialogue("좀 긴장되는데?"));

        // 시험관
        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("제가 이번 시험의 시험관입니다."));
        runner.AddAction(runner.Dialogue("검술의 기본기를 중심으로 평가하겠습니다."));
        runner.AddAction(runner.Dialogue("화려한 기술보다, 자세와 동작의 정확함을 보겠습니다."));

        // 플레이어
        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("기본기라…"));
        runner.AddAction(runner.Dialogue("제일 단순하면서도 제일 어려운 거지."));
        runner.AddAction(runner.Dialogue("어설프게 흉내 내면 바로 드러나니까."));

        // 시험관
        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("맞습니다."));
        runner.AddAction(runner.Dialogue("자세, 시선, 중심. 이 세 가지가 흐트러지지 않고"));
        runner.AddAction(runner.Dialogue("하나로 묶여야 기본이 됩니다."));
        runner.AddAction(runner.Dialogue("당신이 그 균형을 얼마나 잘 유지하는지 지켜보겠습니다."));

        // 플레이어
        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("…좋아."));
        runner.AddAction(runner.Dialogue("말보다 검으로 보여줄 차례군."));
        runner.AddAction(runner.Dialogue("제대로 보여주자."));

        // 시험관
        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("좋습니다."));
        runner.AddAction(runner.Dialogue("검을 들어 주세요."));
        runner.AddAction(runner.Dialogue("지금부터 시험을 시작하겠습니다."));
    }
}
