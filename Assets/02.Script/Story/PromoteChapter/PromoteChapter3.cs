using EnumCollection;
using UnityEngine;

public class PromoteChapter3 : StoryChapter
{
    [Header("Rogue Supervisor Talker")]
    [SerializeField] private StoryTalker supervisorTalker;

    [Header("Rogue Supervisor Model")]
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

        runner.AddAction(runner.Dialogue("……기운이 이상해."));
        runner.AddAction(runner.Dialogue("공기는 고요한데, 등줄기가 서늘하네."));
        runner.AddAction(runner.Dialogue("누가 숨을 죽이고 날 노리고 있어."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("감각이 예민하군요."));
        runner.AddAction(runner.Dialogue("여긴 기습 대응 시험장입니다."));
        runner.AddAction(runner.Dialogue("보이지 않는 적을 감지하고, 피하거나 받아내는 것이 목표죠."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("정면에서 싸우는 상대는 아니라는 거군."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("도적은 그림자 속에서 움직입니다."));
        runner.AddAction(runner.Dialogue("당신이 한순간이라도 방심하면—"));
        runner.AddAction(runner.Dialogue("칼끝이 등을 노릴 겁니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("…한순간도 긴장을 놓지 말라는 말이군."));
        runner.AddAction(runner.Dialogue("좋아, 감각을 곤두세워야겠어."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("시험은 실전처럼 진행됩니다."));
        runner.AddAction(runner.Dialogue("제가 언제, 어디서, 어떤 방식으로 나올지는 알 수 없습니다."));
        runner.AddAction(runner.Dialogue("기척을 느끼고, 반응하지 못하면 그대로 실격입니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("단순히 기세로 밀어붙일 순 없겠네."));
        runner.AddAction(runner.Dialogue("신중하게 임해야하겠어."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("준비되셨다면 바로 시작하겠습니다."));
    }
}
