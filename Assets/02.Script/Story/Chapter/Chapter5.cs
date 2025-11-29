using EnumCollection;
using UnityEngine;

public class Chapter5 : StoryChapter
{
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
        runner.AddAction(runner.Dialogue("여기도 해안인데 공기가 묘하다."));
        runner.AddAction(runner.Dialogue("시원하긴 한데, 마력이 살짝 끼어 있는 느낌."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("마법 흔적이 남아 있어."));
        runner.AddAction(runner.Dialogue("파도가 아니라 마력이 흔들리는 패턴이 계속 이어져."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("오리들도 멍하다. 기운에 치인 거다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("조용한데 오히려 더 불안해."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("저기 봐. 방금 바닷물 위에서 빛이 번진 것 같지 않았어."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("봤어."));
        runner.AddAction(runner.Dialogue("저건 사람 쪽이야."));
        runner.AddAction(runner.Dialogue("몬스터는 저렇게 정교한 흔적 못 남겨."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("강한 놈이면 좋다."));
        runner.AddAction(runner.Dialogue("약하면 우리가 지킨다."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("아무튼 누가 있는 건 확실하다는 거지."));

        runner.AddAction(runner.SetTalkerAction(archerTalker));
        runner.AddAction(runner.Dialogue("응. 직접 확인하러 가보자."));

        runner.AddAction(runner.SetTalkerAction(warriorTalker));
        runner.AddAction(runner.Dialogue("좋다. 가자."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("그래. 눈앞에서 확인해 보자."));
    }
}
