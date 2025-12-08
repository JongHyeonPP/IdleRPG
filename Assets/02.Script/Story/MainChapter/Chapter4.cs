using EnumCollection;
using UnityEngine;

public class Chapter4 : StoryChapter
{
    [Header("NPC Talker")]
    [SerializeField] private StoryTalker crabTalker;

    [Header("NPC Model")]
    [SerializeField] private GameObject crabModel;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { crabTalker };
    public override GameObject[] LocalModels => new GameObject[] { crabModel };

    public override StoryRenderType[] GetRequiredRenderTypes()
    {
        return new StoryRenderType[]
        {
            StoryRenderType.Player,
            StoryRenderType.Companion0,
            StoryRenderType.Companion1
        };
    }

    public override void BuildActions(StoryRunner runner)
    {
        StoryTalker protagonist = StoryManager.instance.GetTalker(StoryRenderType.Player);
        StoryTalker archerTalker = StoryManager.instance.GetTalker(StoryRenderType.Companion0);
        StoryTalker warriorTalker = StoryManager.instance.GetTalker(StoryRenderType.Companion1);

        GameObject protagonistModel = StoryManager.instance.GetModel(StoryRenderType.Player);
        GameObject archerModel = StoryManager.instance.GetModel(StoryRenderType.Companion0);
        GameObject warriorModel = StoryManager.instance.GetModel(StoryRenderType.Companion1);

        Vector3 cOrigin = crabModel.transform.position;
        Vector3 wOrigin = warriorModel.transform.position;

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        protagonistModel.transform.position += new Vector3(-5f, 0f, 0f);
        archerModel.transform.position += new Vector3(-5f, 0f, 0f);

        runner.AddAction(runner.MoveWithAnim(protagonistModel, protagonistModel.transform.position + new Vector3(5f, 0f, 0f), 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(archerModel, archerModel.transform.position + new Vector3(5f, 0f, 0f), 0.7f, true));

        runner.AddAction(runner.Dialogue("여긴… 갑자기 바다네."));
        runner.AddAction(runner.Dialogue("공기가 짠 것도 이상한데, 어디선가 꽉 조이는 느낌도 난다."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("해안 쪽 기척도 요즘 험해."));
        runner.AddAction(runner.Dialogue("숲에서 느끼던 불안한 흐름이 이쪽까지 번진 느낌이야."));

        crabModel.transform.position = cOrigin + new Vector3(6f, 0f, 0f);
        runner.AddAction(runner.MoveWithAnim(crabModel, cOrigin, 0.55f, true));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("잠깐. 저 집게 생각보다 빠른데."));

        Vector3 wStart = wOrigin + new Vector3(-8f, 0f, 0f);
        Vector3 wEnd = wOrigin + new Vector3(2.5f, 0f, 0f);

        warriorModel.transform.position = wStart;

        runner.AddAction(runner.MoveWithAnim(warriorModel, wEnd, 0.35f, true));
        runner.AddAction(runner.Attack(warriorModel));

        runner.AddAction(runner.MoveWithAnim(crabModel, cOrigin + new Vector3(1.1f, 0f, 0f), 0.25f, false));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("지금… 방금 뭐였어. 집게보다 사람이 먼저 튀어나왔는데."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("위험했다. 저 정도면 한 번 물리면 심각하다."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("혹시 여기 지키는 사람인 거야?"));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("맞다. 이 근처 지키고 있었다."));
        runner.AddAction(runner.Dialogue("근데 요즘 몬스터들 다 이상하다."));
        runner.AddAction(runner.Dialogue("눈 뒤집혀서 날뛰기만 한다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("숲에서도 비슷했는데. 여기까지 번진 거네."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("그래서 너희 그냥 두면 곤란하다."));
        runner.AddAction(runner.Dialogue("둘 다 금방 쓰러질 얼굴이다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("그래서 결론은 같이 다니겠단 소리야."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("그래. 너희 쓰러지면 나 혼자 다 막아야 된다."));
        runner.AddAction(runner.Dialogue("그건 싫다. 같이 때리자."));
        runner.AddAction(runner.Dialogue("간다. 앞에서도 또 온다."));
    }
}
