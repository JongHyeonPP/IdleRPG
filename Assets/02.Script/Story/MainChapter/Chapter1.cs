using EnumCollection;
using UnityEngine;

public class Chapter1 : StoryChapter
{
    [Header("NPC Talker")]
    [SerializeField] private StoryTalker ratTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject bigRat;
    [SerializeField] private GameObject[] rats;

    public override StoryTalker[] LocalTalkers => new StoryTalker[] { ratTalker };

    public override GameObject[] LocalModels
    {
        get
        {
            GameObject[] arr = new GameObject[1 + rats.Length];
            arr[0] = bigRat;
            for (int i = 0; i < rats.Length; i++)
                arr[i + 1] = rats[i];
            return arr;
        }
    }

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
        GameObject protagonistModel = StoryManager.instance.GetModel(StoryRenderType.Player);

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.5f));

        Vector3 origin = protagonistModel.transform.position;
        Vector3 startPos = origin + new Vector3(-3f, 0f, 0f);
        protagonistModel.transform.position = startPos;

        runner.AddAction(runner.MoveWithAnim(protagonistModel, origin, 1f, true));

        runner.AddAction(runner.Dialogue("여기도 풀밭인데 느낌이 좀 다르네."));
        runner.AddAction(runner.Dialogue("잔디가 여기저기 파여 있어."));
        runner.AddAction(runner.Dialogue("누가 땅을 파고 다닌 건가."));
        runner.AddAction(runner.Dialogue("이쯤 되면 몬스터가 하나쯤 튀어나오는 게 정상이지."));

        Vector3 offset = new Vector3(5f, 0f, 0f);

        Vector3 bigOrigin = bigRat.transform.position;
        bigRat.transform.position = bigOrigin + offset;

        Vector3[] ratOrigins = new Vector3[rats.Length];
        for (int i = 0; i < rats.Length; i++)
        {
            ratOrigins[i] = rats[i].transform.position;
            rats[i].transform.position = ratOrigins[i] + offset;
        }

        runner.AddAction(runner.MoveWithAnim(bigRat, bigOrigin, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(ratTalker));
        runner.AddAction(runner.Dialogue("찍찍."));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("어… 의외로 귀엽게 생겼는데."));
        runner.AddAction(runner.Dialogue("근데 나 보는 눈빛이 귀엽지가 않다."));

        for (int i = 0; i < rats.Length; i++)
            runner.AddAction(runner.MoveWithAnim(rats[i], ratOrigins[i], 0.9f, false));

        runner.AddAction(runner.Dialogue("잠깐만, 한 마리가 아니었네."));
        runner.AddAction(runner.Dialogue("왜 이렇게 많이 나오는 거야."));

        runner.AddAction(runner.SetTalkerAction(ratTalker));
        runner.AddAction(runner.Dialogue("찍찍찍."));

        runner.AddAction(runner.Attack(bigRat));

        runner.AddAction(runner.SetTalkerAction(protagonist));
        runner.AddAction(runner.Dialogue("이건 그냥 땅 파는 정도가 아니라 화난 수준인데."));
        runner.AddAction(runner.Dialogue("좋아. 여기까지 왔으니 잔디 정리 좀 해보자."));
    }
}
