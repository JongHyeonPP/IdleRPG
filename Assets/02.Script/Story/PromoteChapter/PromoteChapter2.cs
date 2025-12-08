using EnumCollection;
using UnityEngine;

public class PromoteChapter2 : StoryChapter
{
    [Header("Mage Supervisor Talker")]
    [SerializeField] private StoryTalker supervisorTalker;

    [Header("Mage Supervisor Model")]
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

        runner.AddAction(runner.Dialogue("이번엔… 마력이 느껴지는 듯한데?"));
        runner.AddAction(runner.Dialogue("피부가 살짝 저릿할 정도야."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("딩동댕~ 정답입니다."));
        runner.AddAction(runner.Dialogue("이 구역은 마법 방어 능력을 평가하죠."));
        runner.AddAction(runner.Dialogue("정확히 말하자면, 마법에 대한 집중력과 타이밍을 보는 시험입니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("쉽진 않겠지만, 마나 흐름만 잘 읽으면…"));
        runner.AddAction(runner.Dialogue("막아낼 수 있을 거야. 아마도."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("생각보다 훨씬 빠릅니다."));
        runner.AddAction(runner.Dialogue("그리고 생각보다 훨씬 아프고요."));
        runner.AddAction(runner.Dialogue("실전 상황이라고 생각하세요."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("살벌하네…"));
        runner.AddAction(runner.Dialogue("괜히 방심했다간 그대로 나가떨어지겠는데."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("맞습니다."));
        runner.AddAction(runner.Dialogue("작은 미세한 마력의 움직임도 놓치면 안 돼요."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("…좋아, 머리는 차갑게. 감각은 예민하게."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("이제 시작하겠습니다."));
        runner.AddAction(runner.Dialogue("자세 잡아주세요."));
    }
}
