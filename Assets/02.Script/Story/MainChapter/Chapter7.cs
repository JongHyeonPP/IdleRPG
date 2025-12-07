using EnumCollection;
using UnityEngine;

public class Chapter7 : StoryChapter
{
    [Header("NPC Talkers")]
    [SerializeField] private StoryTalker wolfTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject wolf;
    [SerializeField] private GameObject wolfBoss;

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
        wolfTalker
    };

    public override GameObject[] LocalModels => new GameObject[]
    {
        wolf,
        wolfBoss
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

        // 네 명 전부 같은 offset 뒤에서 등장
        Vector3 offset = new Vector3(-3f, 0f, 0f);

        p.transform.position = oP + offset;
        a.transform.position = oA + offset;
        w.transform.position = oW + offset;
        m.transform.position = oM + offset;

        // 등장 + 페이드
        runner.AddAction(runner.FadeInOut(0f, 0f, 1.2f));

        // 4명 함께 이동
        runner.AddAction(runner.MoveWithAnim(p, oP, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(a, oA, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(w, oW, 0.8f, false));
        runner.AddAction(runner.MoveWithAnim(m, oM, 0.8f, true));

        // ------------------------------
        // 스토리 본문
        // ------------------------------
        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름의 끊김이 이 근처에서도 계속 이어지고 있어."));
        runner.AddAction(runner.Dialogue("특히 여긴 흔적이 진하게 남아."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("아까 말한 요미 에너지라는 게 정확히 뭐야. 계속 흐름이라고만 하니까 감이 안 와."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("아주 단순하게 말하면, 생명들이 왜 살아 있는지를 설명하는 힘."));
        runner.AddAction(runner.Dialogue("이유를 잃어버리면, 몸도 마음도 같이 틀어져."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("밥 같은 거구나."));
        runner.AddAction(runner.Dialogue("밥줄 끊기면 다 이상해지는 거고."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("정확해."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("그럼 지금 이 세계는 밥이 새고 있는 상태라는 거네."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("혹은 누군가가 몰래 걷어 가고 있거나."));
        runner.AddAction(runner.Dialogue("자연스럽게 흐르는 모습은 아니야."));

        // 늑대 등장
        Vector3 wolfOrigin = wolf.transform.position;
        Vector3 wolfStart = wolfOrigin + new Vector3(3f, 0f, 0f);
        wolf.transform.position = wolfStart;
        runner.AddAction(runner.MoveWithAnim(wolf, wolfOrigin, 0.5f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("얘 표정 너무 멍한데."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("굶은 늑대 같기도 하고. 힘이 남은 느낌은 아닌데."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("싸우고 싶은 게 아니라 버티는 중이야."));
        runner.AddAction(runner.Dialogue("흐름이 비어 가는데, 본능만 남은 거지."));

        // 보스 늑대 등장
        Vector3 bossOrigin = wolfBoss.transform.position;
        Vector3 bossStart = bossOrigin + new Vector3(4f, 0f, 0f);
        wolfBoss.transform.position = bossStart;

        runner.AddAction(runner.MoveWithAnim(wolfBoss, bossOrigin, 0.7f, true));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("얘는 더 안 좋아 보이는데."));
        runner.AddAction(runner.Dialogue("눈빛이 완전히 헤매고 있어."));

        runner.AddAction(runner.SetTalkerAction(wolfTalker));
        runner.AddAction(runner.Dialogue("그으… 어지러워…"));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("우두머리까지 이 정도라면 꽤 힘든 상태야."));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("우리 쪽을 확실히 적대하긴 해. 피해서 지나가긴 틀렸다."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("좋아. 길 열자"));
    }
}
