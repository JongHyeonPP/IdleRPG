using EnumCollection;
using UnityEngine;

public class PromoteChapter1 : StoryChapter
{
    [Header("Archer Supervisor Talker")]
    [SerializeField] private StoryTalker supervisorTalker;

    [Header("Archer Supervisor Model")]
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

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.0f));

        runner.AddAction(runner.Dialogue("바닥에 꽂힌 화살들… 이번 시험과 관련있는건가?"));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("눈치가 빠르군요."));
        runner.AddAction(runner.Dialogue("이곳에선 회피 능력을 평가합니다."));
        runner.AddAction(runner.Dialogue("압박 속에서도 멈추지 않고, 앞으로 나아갈 수 있는지."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("피하는 건 자신 있어."));
        runner.AddAction(runner.Dialogue("뒤로만 안 물러나면 되는 거지?"));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("정확합니다."));
        runner.AddAction(runner.Dialogue("뒤돌아 도망치는 순간, 거기서 시험은 끝입니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("좋아. 제대로 조준하기도 전에 그쪽에 도달해주지."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("좋은 기세입니다. 그럼 시험 시작합니다."));
    }
}
