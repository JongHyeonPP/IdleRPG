using EnumCollection;
using UnityEngine;

public class PromoteChapter4 : StoryChapter
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

        runner.AddAction(runner.Dialogue("…처음부터 마법진이 깔려 있네."));
        runner.AddAction(runner.Dialogue("발 한 번 잘못 디디면 그대로 박살날 것 같아."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("정확한 분석입니다."));
        runner.AddAction(runner.Dialogue("이 구역은 설계된 전장을 돌파할 수 있는지를 평가하는 시험입니다."));
        runner.AddAction(runner.Dialogue("당신이 어디를 밟고, 어떻게 반응할지 전부 계산해두었거든요."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("계산? 내 움직임까지 말이지?"));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("당신의 속도, 반응, 심지어 근력까지."));
        runner.AddAction(runner.Dialogue("전투 설계에 필요한 모든 변수를 실제 상황처럼 환산했습니다."));
        runner.AddAction(runner.Dialogue("그 결과 실패 확률은 91.2%."));
        runner.AddAction(runner.Dialogue("이 계산된 전장을 무너뜨리면 통과입니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("9퍼센트면 넉넉하네."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("계산으로만 보면 무모한 선택이지만"));
        runner.AddAction(runner.Dialogue("전장은 늘 예측 밖에서 무너지는 법이죠."));
        runner.AddAction(runner.Dialogue("그 가능성을 확인하는 것이 이 시험의 목적입니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("좋아. 열심히 계산해봐."));
        runner.AddAction(runner.Dialogue("나는 몸이 먼저 가니까."));

        runner.AddAction(runner.SetTalkerAction(supervisor));
        runner.AddAction(runner.Dialogue("그 자신감이 얼마나 먹힐 수 있을지…"));
        runner.AddAction(runner.Dialogue("시험을 시작합니다."));
    }
}
