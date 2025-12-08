using EnumCollection;
using UnityEngine;

public class Chapter6 : StoryChapter
{
    [Header("NPC Model")]
    [SerializeField] private GameObject mageModel;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { };

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
        StoryTalker mageTalker = StoryManager.instance.GetTalker(StoryRenderType.Companion2);

        GameObject protagonistModel = StoryManager.instance.GetModel(StoryRenderType.Player);
        GameObject archerModel = StoryManager.instance.GetModel(StoryRenderType.Companion0);
        GameObject warriorModel = StoryManager.instance.GetModel(StoryRenderType.Companion1);

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.0f));

        Vector3 pOrigin = protagonistModel.transform.position;
        Vector3 pStart = pOrigin + new Vector3(-5f, 0f, 0f);
        protagonistModel.transform.position = pStart;

        Vector3 aOrigin = archerModel.transform.position;
        Vector3 aStart = aOrigin + new Vector3(-5f, 0f, 0f);
        archerModel.transform.position = aStart;

        Vector3 wOrigin = warriorModel.transform.position;
        Vector3 wStart = wOrigin + new Vector3(-5f, 0f, 0f);
        warriorModel.transform.position = wStart;

        runner.AddAction(runner.MoveWithAnim(protagonistModel, pOrigin, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(archerModel, aOrigin, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(warriorModel, wOrigin, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("저기… 해골이야."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("근데 움직임이 너무 자연스럽다."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("야. 뭐 하는 놈이냐."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("정리 중이었습니다."));
        runner.AddAction(runner.Dialogue("이쪽 마력 흐름이 심하게 틀어져서요."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("정리라고 하면…"));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("요미 에너지가 새고 있어요."));
        runner.AddAction(runner.Dialogue("누군가는 막아야 하잖아요."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("근데 왜 해골이냐. 싸우다 죽었냐."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("아니요."));
        runner.AddAction(runner.Dialogue("책을 너무 오래 읽다가 이렇게 됐습니다."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("진짜로."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("저도 믿기진 않지만, 결과가 이렇네요."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("허약하다. 허약해."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("대신 머리는 잘 돌아갑니다. 이쪽 일에는 더 중요해요."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("아까 말한 요미 에너지 누출이, 몬스터들 이상해진 이유야."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("맞아요."));
        runner.AddAction(runner.Dialogue("흐름을 되돌리려면 근원을 찾아야 합니다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("우리는 그걸 어떻게 해야 하는데."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("저를 데려가면 됩니다."));
        runner.AddAction(runner.Dialogue("제가 보고 설명하고 조정하죠."));
        runner.AddAction(runner.Dialogue("대신 뛰는 건 힘들어서 뒤에서 처리하겠습니다."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("좋다. 뒤에서 뭐든 해라."));

        runner.AddAction(runner.SetTalkerAction(mageTalker));
        runner.AddAction(runner.Dialogue("그럼 동행하겠습니다."));
        runner.AddAction(runner.Dialogue("함께 근원을 찾아서 막죠."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("가자. 흐름이 새는 근원 찾으러."));
    }
}
