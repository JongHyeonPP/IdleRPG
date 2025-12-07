using EnumCollection;
using UnityEngine;

public class Chapter2 : StoryChapter
{
    [Header("NPC Talker")]
    [SerializeField] private StoryTalker treeTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject bigTree;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { treeTalker };
    public override GameObject[] LocalModels => new GameObject[] { bigTree };

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

        Vector3 origin = protagonistModel.transform.position;
        Vector3 startPos = origin + new Vector3(-5f, 0f, 0f);

        protagonistModel.transform.position = startPos;
        runner.AddAction(runner.MoveWithAnim(protagonistModel, origin, 0.7f, true));

        runner.AddAction(runner.Dialogue("여긴 완전 숲이네."));
        runner.AddAction(runner.Dialogue("나무들 모양이 좀 불길한데. 보기만 해도 찔릴 것 같아."));

        Vector3 offset = new Vector3(5f, 0f, 0f);
        Vector3 treeOrigin = bigTree.transform.position;

        bigTree.transform.position = treeOrigin + offset;
        runner.AddAction(runner.MoveWithAnim(bigTree, treeOrigin, 1.0f, true));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("잠깐. 방금 나무가 걸어 온 거 맞지."));
        runner.AddAction(runner.Dialogue("내가 피곤한 게 아니라 진짜 움직였지."));

        runner.AddAction(runner.SetTalkerAction(treeTalker));
        runner.AddAction(runner.Dialogue("또각."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("눈 마주친 것 같은데. 나무랑 눈을 마주쳤다고 해야 하나."));

        if (archerModel != null)
        {
            Vector3 aOrigin = archerModel.transform.position;
            Vector3 aStart = aOrigin + new Vector3(-5f, 0f, 0f);

            archerModel.transform.position = aStart;
            runner.AddAction(runner.MoveWithAnim(archerModel, aOrigin, 0.7f, true));
        }

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("또 빗나갔네. 미안. 원래 저 나무만 노리려던 거였는데."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("지금 나한테 사과하는 거야, 나무한테 사과하는 거야."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("둘 다."));
        runner.AddAction(runner.Dialogue("요즘 이 숲 애들, 보기만 해도 바로 달려들어."));
        runner.AddAction(runner.Dialogue("원래는 그냥 조용히 서 있던 나무들이었거든."));
        runner.AddAction(runner.Dialogue("며칠 전부터 숲 분위기가 확 변했어."));
        runner.AddAction(runner.Dialogue("마을 사람들은 숲에 금이 갔다고들 하는데, 말 그대로인지는 모르겠어."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("괜히 불안하니까 아무한테나 화내는 중이라는 거네."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("이유도 모르고 휘둘리는 느낌이지. 뭔가 크게 잘못된 건 확실해."));

        runner.AddAction(runner.SetTalkerAction(treeTalker));
        runner.AddAction(runner.Dialogue("또각. 또각."));

        runner.AddAction(runner.Attack(bigTree));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("나무가 화내니까 괜히 더 무섭다."));
        runner.AddAction(runner.Dialogue("좋아. 진정시키고, 여기서 무슨 일이 벌어지는지부터 알아보자."));
    }
}
