using EnumCollection;
using UnityEngine;

public class Chapter3 : StoryChapter
{
    [Header("NPC Talker")]
    [SerializeField] private StoryTalker bearTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject bearModel;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { bearTalker };
    public override GameObject[] LocalModels => new GameObject[] { bearModel };

    public override StoryRenderType[] GetRequiredRenderTypes()
    {
        return new StoryRenderType[]
        {
            StoryRenderType.Player,
            StoryRenderType.Companion0
        };
    }

    public override void BuildActions(StoryRunner runner)
    {
        StoryTalker protagonist = StoryManager.instance.GetTalker(StoryRenderType.Player);
        StoryTalker archerTalker = StoryManager.instance.GetTalker(StoryRenderType.Companion0);

        GameObject protagonistModel = StoryManager.instance.GetModel(StoryRenderType.Player);
        GameObject archerModel = StoryManager.instance.GetModel(StoryRenderType.Companion0);

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        Vector3 pOrigin = protagonistModel.transform.position;
        Vector3 pStart = pOrigin + new Vector3(-5f, 0f, 0f);
        protagonistModel.transform.position = pStart;

        Vector3 aOrigin = archerModel.transform.position;
        Vector3 aStart = aOrigin + new Vector3(-5f, 0f, 0f);
        archerModel.transform.position = aStart;

        runner.AddAction(runner.MoveWithAnim(protagonistModel, pOrigin, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(archerModel, aOrigin, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("숲 안쪽으로 계속 들어오는데도 공기가 더 답답해진다."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("여기 숲은 며칠 전부터 특히 이상해."));
        runner.AddAction(runner.Dialogue("조심해. 여기 근처에 곰이 살아."));
        runner.AddAction(runner.Dialogue("원래도 위험한 녀석인데 요즘은 그냥 지나가도 으르렁댄다고 하더라."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("곰이 있는 건 그렇다 치고…"));
        runner.AddAction(runner.Dialogue("그 곰도 요즘 이상해졌다는 거네."));

        Vector3 bOrigin = bearModel.transform.position;
        Vector3 bStart = bOrigin + new Vector3(6f, 0f, 0f);
        bearModel.transform.position = bStart;

        runner.AddAction(runner.MoveWithAnim(bearModel, bOrigin, 1f, true));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("봐. 표정부터 평범하진 않다."));
        runner.AddAction(runner.Dialogue("진짜 화난 눈빛인데."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("원래도 무섭긴 했지만 사람을 먼저 물진 않았어."));
        runner.AddAction(runner.Dialogue("지금은 위태로운 느낌이야. 숨만 쉬어도 예민한 상태."));

        runner.AddAction(runner.SetTalkerAction(bearTalker));
        runner.AddAction(runner.Dialogue("크르르."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("지금 완전히 공격 준비 중이지."));
        runner.AddAction(runner.Dialogue("말로 풀 상황 같진 않네."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("이것도 숲 흐름이 끊어진 탓일 거야."));
        runner.AddAction(runner.Dialogue("뭔가 잃어버린 것처럼 계속 불안해하는 기운이 느껴져."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("잃어버린 게 뭔지도 모르는데 우리한테 화풀이부터 하는 셈이네."));
        runner.AddAction(runner.Dialogue("일단 달려들기 전에 진정시키자."));

        runner.AddAction(runner.SetTalkerAction(bearTalker));
        runner.AddAction(runner.Dialogue("크르아아."));

        runner.AddAction(runner.Attack(bearModel));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("와… 방금 눈빛, 진짜로 덮칠 뻔했다."));
        runner.AddAction(runner.Dialogue("살고 싶으면 여기서도 버텨야지."));
    }
}
