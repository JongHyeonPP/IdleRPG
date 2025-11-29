using EnumCollection;
using UnityEngine;

public class Chapter8 : StoryChapter
{
    [Header("NPC Talkers")]
    [SerializeField] private StoryTalker frogTalker;

    [Header("NPC Models")]
    [SerializeField] private GameObject[] frogModels;

    public override StoryTalker[] LocalTalkers => new StoryTalker[]
    {
        frogTalker
    };

    public override GameObject[] LocalModels => frogModels;

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

        GameObject playerModel = StoryManager.instance.GetModel(StoryRenderType.Player);

        Vector3 origin = playerModel.transform.position;
        Vector3 start = origin + new Vector3(-2.5f, 0f, 0f);
        playerModel.transform.position = start;

        runner.AddAction(runner.FadeInOut(0f, 0f, 1f));
        runner.AddAction(runner.MoveWithAnim(playerModel, origin, 0.8f, true));

        runner.AddAction(runner.SetTalkerAction(archer));
        runner.AddAction(runner.Dialogue("방금 들었지, 저 울음 소리."));
        runner.AddAction(runner.Dialogue("개구리치곤 너무 딱딱 맞춰 울지 않아. 누가 박자라도 찍어 준 것 같아."));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("저러다 진짜 잡아먹히겠다."));
        runner.AddAction(runner.Dialogue("저렇게 티 팍팍 내면, 와서 물어 달라는 거잖아."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("자연스러운 울음은 아니야."));
        runner.AddAction(runner.Dialogue("소리 날 때마다 흐름이 같이 끌려가고 있어."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("주변은 조용한데 소리만 둥둥 떠다니네."));
        runner.AddAction(runner.Dialogue("이대로 두기엔 불안한 느낌이다."));

        Vector3[] frogOrigins = new Vector3[frogModels.Length];
        for (int i = 0; i < frogModels.Length; i++)
        {
            if (frogModels[i] == null)
                continue;

            frogOrigins[i] = frogModels[i].transform.position;
        }

        int rounds = 2;

        for (int r = 0; r < rounds; r++)
        {
            int lastIndex = -1;
            for (int i = 0; i < frogModels.Length; i++)
            {
                if (frogModels[i] != null)
                    lastIndex = i;
            }

            if (lastIndex == -1)
                break;

            // 오른쪽으로 동시에
            for (int i = 0; i < frogModels.Length; i++)
            {
                GameObject frog = frogModels[i];
                if (frog == null)
                    continue;

                Vector3 fOrigin = frogOrigins[i];
                float amp = 0.35f + 0.07f * i;
                bool wait = i == lastIndex;

                runner.AddAction(runner.MoveWithAnim(
                    frog,
                    fOrigin + new Vector3(amp, 0f, 0f),
                    0.1f,
                    wait
                ));
            }

            // 왼쪽으로 동시에
            lastIndex = -1;
            for (int i = 0; i < frogModels.Length; i++)
            {
                if (frogModels[i] != null)
                    lastIndex = i;
            }

            for (int i = 0; i < frogModels.Length; i++)
            {
                GameObject frog = frogModels[i];
                if (frog == null)
                    continue;

                Vector3 fOrigin = frogOrigins[i];
                float amp = 0.35f + 0.07f * i;
                bool wait = i == lastIndex;

                runner.AddAction(runner.MoveWithAnim(
                    frog,
                    fOrigin + new Vector3(-amp, 0f, 0f),
                    0.1f,
                    wait
                ));
            }

            // 다시 살짝 줄여서 오른쪽
            lastIndex = -1;
            for (int i = 0; i < frogModels.Length; i++)
            {
                if (frogModels[i] != null)
                    lastIndex = i;
            }

            for (int i = 0; i < frogModels.Length; i++)
            {
                GameObject frog = frogModels[i];
                if (frog == null)
                    continue;

                Vector3 fOrigin = frogOrigins[i];
                float amp = (0.35f + 0.07f * i) * 0.6f;
                bool wait = i == lastIndex;

                runner.AddAction(runner.MoveWithAnim(
                    frog,
                    fOrigin + new Vector3(amp, 0f, 0f),
                    0.08f,
                    wait
                ));
            }

            // 원위치로 동시에 복귀
            lastIndex = -1;
            for (int i = 0; i < frogModels.Length; i++)
            {
                if (frogModels[i] != null)
                    lastIndex = i;
            }

            for (int i = 0; i < frogModels.Length; i++)
            {
                GameObject frog = frogModels[i];
                if (frog == null)
                    continue;

                Vector3 fOrigin = frogOrigins[i];
                bool wait = i == lastIndex;

                runner.AddAction(runner.MoveWithAnim(
                    frog,
                    fOrigin,
                    0.08f,
                    wait
                ));
            }
        }

        runner.AddAction(runner.SetTalkerAction(frogTalker));
        runner.AddAction(runner.Dialogue("개…굴…"));

        runner.AddAction(runner.SetTalkerAction(warrior));
        runner.AddAction(runner.Dialogue("보는 내가 멀미 나겠다."));

        runner.AddAction(runner.SetTalkerAction(mage));
        runner.AddAction(runner.Dialogue("흐름이 이 정도로 구겨졌다는 뜻이야."));

        runner.AddAction(runner.SetTalkerAction(player));
        runner.AddAction(runner.Dialogue("어쨌든 앞길을 막고 있는 건 맞지."));
        runner.AddAction(runner.Dialogue("정리하고 가자."));

        runner.AddAction(runner.FadeInOut(0f, 0.25f, 1f));
    }
}
