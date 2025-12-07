using EnumCollection;
using UnityEngine;

public class Chapter14 : StoryChapter
{
    [Header("NPC Models")]
    [SerializeField] private GameObject boar;

    public override StoryTalker[] LocalTalkers => null;

    public override GameObject[] LocalModels => new GameObject[]
    {
        boar
    };

    public override StoryRenderType[] GetRequiredRenderTypes()
    {
        return new StoryRenderType[]
        {
            StoryRenderType.Player,
            StoryRenderType.Companion0,
            StoryRenderType.Companion1,
            StoryRenderType.Companion2
        };
    }

    public override void BuildActions(StoryRunner runner)
    {
        StoryTalker player = StoryManager.instance.GetTalker(StoryRenderType.Player);
        StoryTalker archer = StoryManager.instance.GetTalker(StoryRenderType.Companion0);
        StoryTalker warrior = StoryManager.instance.GetTalker(StoryRenderType.Companion1);
        StoryTalker mage = StoryManager.instance.GetTalker(StoryRenderType.Companion2);

        GameObject p = StoryManager.instance.GetModel(StoryRenderType.Player);
        GameObject a = StoryManager.instance.GetModel(StoryRenderType.Companion0);
        GameObject w = StoryManager.instance.GetModel(StoryRenderType.Companion1);
        GameObject m = StoryManager.instance.GetModel(StoryRenderType.Companion2);

        Vector3 oP = p.transform.position;
        Vector3 oA = a.transform.position;
        Vector3 oW = w.transform.position;
        Vector3 oM = m.transform.position;

        Vector3 offset = new Vector3(-2.5f, 0f, 0f);

        p.transform.position = oP + offset;
        a.transform.position = oA + offset;
        w.transform.position = oW + offset;
        m.transform.position = oM + offset;

        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        runner.AddAction(runner.MoveWithAnim(p, oP, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.7f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("아까 말한 오디션 기억 있지."));
        runner.AddAction(runner.Dialogue("그때 심사위원들이 너 보고 난리 났다 했잖아."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("응."));
        runner.AddAction(runner.Dialogue("노래보다 얼굴 얘기를 더 많이 들었던 것 같긴 한데."));

        // 전사 대사
        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("뭐, 이해는 된다."));
        runner.AddAction(runner.Dialogue("나였으면 더 시끄러웠을 거다."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("본인 자랑 아니면 말을 못 하네."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("그 순간이야."));
        runner.AddAction(runner.Dialogue("사람들 시선이 너한테 한꺼번에 몰렸을 때, 요미 흐름도 같이 흔들렸을 거야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("사람들이 나를 바라보는 걸, 이 세계도 같이 본 거라고."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("그래."));
        runner.AddAction(runner.Dialogue("저쪽 세계에서 널 비추던 시선이 이 세계까지 닿은 거야."));
        runner.AddAction(runner.Dialogue("여기는 그걸 보고 너를 데려오기로 한 거고."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("정리하면, 심사위원들이 난리 치는 바람에 세계까지 같이 꽂혀서 납치한 거네."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("귀엽다는 소리 듣다가 세계까지 넘어온 사람은 처음 본다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("뭔가 거창한 운명 같은 건 줄 알았는데."));
        runner.AddAction(runner.Dialogue("결국 이유는 귀여움이야."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름이 움직이기엔 그 정도 이유면 충분해."));
        runner.AddAction(runner.Dialogue("결과가 지금 우리 네 명이 같이 걷고 있는 거고."));

        // 맷돼지 몬스터 등장 연출
        Vector3 boarOrigin = boar.transform.position;
        boar.transform.position = boarOrigin + new Vector3(3f, 0f, 0f);

        runner.AddAction(runner.MoveWithAnim(boar, boarOrigin, 0.5f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("앞에서 뭔가 달려온다."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("됐다. 이제 생각 그만하고 눈앞부터 처리하자."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("그래. 정리도 했겠다, 몸도 좀 풀어볼까."));
    }
}
