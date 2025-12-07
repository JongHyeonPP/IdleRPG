using EnumCollection;
using UnityEngine;
using System.Collections.Generic;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance;

    [SerializeField] private StoryRunner runner;
    [SerializeField] private StoryChapter[] mainChapters;

    [SerializeField] private StoryChapter[] promoteChapters;

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera storyCamera;

    [Header("Talkers")]
    [SerializeField] private StoryTalker protagonistTalker;
    [SerializeField] private StoryTalker archerTalker;
    [SerializeField] private StoryTalker warriorTalker;
    [SerializeField] private StoryTalker mageTalker;

    [Header("Models")]
    [SerializeField] private GameObject protagonistModel;
    [SerializeField] private GameObject archerModel;
    [SerializeField] private GameObject warriorModel;
    [SerializeField] private GameObject mageModel;

    private StoryChapter activeChapter;

    private Dictionary<StoryRenderType, StoryTalker> talkerMap;
    private Dictionary<StoryRenderType, GameObject> modelMap;

    // 원래 위치 저장
    private Dictionary<GameObject, Vector3> originalPositions = new();


    private void Awake()
    {
        instance = this;
        if (mainCamera == null)
            mainCamera = Camera.main;

        InitMaps();

        // 모든 Chapter 를 활성화시키되 본문은 꺼둔다
        InitChapters(mainChapters);
        InitChapters(promoteChapters);

        // Local talker 전부 OFF
        DisableLocalTalkersOfAllChapters();

        // 원래 위치 저장 동료 플레이어 챕터별 local 모델
        SaveOriginalPositions();

        PlayerBroker.OnPlayerAppearanceChange += ad =>
        {
            protagonistModel.GetComponentInChildren<AppearanceController>()
                ?.SetAppearance(ad);
        };

        BattleBroker.SwitchToStory += RunStory;
        BattleBroker.SwitchToAdventure += (_,_)=>SwitchToBattle();
        BattleBroker.SwitchToBattle += SwitchToBattle;
        BattleBroker.SwitchToCompanionBattle += (_,_) => SwitchToBattle();
        BattleBroker.SwitchToDungeon += (_,_) => SwitchToBattle();
        BattleBroker.SwitchToPromoteBattle += (_) => SwitchToBattle();
    }

    private void InitMaps()
    {
        talkerMap = new();
        modelMap = new();

        talkerMap[StoryRenderType.Player] = protagonistTalker;
        talkerMap[StoryRenderType.Companion0] = archerTalker;
        talkerMap[StoryRenderType.Companion1] = warriorTalker;
        talkerMap[StoryRenderType.Companion2] = mageTalker;

        modelMap[StoryRenderType.Player] = protagonistModel;
        modelMap[StoryRenderType.Companion0] = archerModel;
        modelMap[StoryRenderType.Companion1] = warriorModel;
        modelMap[StoryRenderType.Companion2] = mageModel;
    }

    private void InitChapters(StoryChapter[] chapters)
    {
        if (chapters == null) return;

        foreach (var c in chapters)
        {
            if (c == null) continue;

            c.gameObject.SetActive(true);
            c.SetChapterActive(false);
        }
    }

    private void SaveOriginalPositions()
    {
        // 플레이어 동료
        foreach (var kv in modelMap)
        {
            if (kv.Value != null)
                originalPositions[kv.Value] = kv.Value.transform.position;
        }

        // 각 chapter 의 local models 도 저장
        SaveLocalModelsFor(mainChapters);
        SaveLocalModelsFor(promoteChapters);
    }

    private void SaveLocalModelsFor(StoryChapter[] chapters)
    {
        if (chapters == null) return;

        foreach (var c in chapters)
        {
            if (c == null) continue;

            var locals = c.LocalModels;
            if (locals == null) continue;

            foreach (var m in locals)
            {
                if (m != null)
                    originalPositions[m] = m.transform.position;
            }
        }
    }

    private void DisableLocalTalkersOfAllChapters()
    {
        DisableLocalTalkersFor(mainChapters);
        DisableLocalTalkersFor(promoteChapters);
    }

    private void DisableLocalTalkersFor(StoryChapter[] chapters)
    {
        if (chapters == null) return;

        foreach (var c in chapters)
        {
            var locals = c?.LocalTalkers;
            if (locals == null) continue;

            foreach (var t in locals)
            {
                if (t != null && t.talkerObject != null)
                    t.talkerObject.SetActive(false);
            }
        }
    }

    private void RunStory(BattleType storyType, int[] index)
    {
        if (index == null || index.Length == 0)
        {
            Debug.LogError("RunStory 에 전달된 index 배열이 비어 있음");
            return;
        }

        switch (storyType)
        {
            case BattleType.Default:
                activeChapter = mainChapters[index[0]];
                break;

            case BattleType.Promote:
                activeChapter = promoteChapters[index[0]];
                break;
            default:
                Debug.LogError("알 수 없는 StoryType");
                return;
        }


        DisableLocalTalkersOfAllChapters();

        // 모든 챕터 비활성화
        SetAllChaptersActive(false);

        
        activeChapter.SetChapterActive(true);

        // Local talker ON
        var locals = activeChapter.LocalTalkers;
        if (locals != null)
        {
            foreach (var t in locals)
            {
                if (t != null && t.talkerObject != null)
                    t.talkerObject.SetActive(true);
            }
        }

        // 필요한 모델만 Enable
        var requiredTypes = activeChapter.GetRequiredRenderTypes();

        foreach (var kv in modelMap)
        {
            bool needed = false;

            if (requiredTypes != null)
            {
                foreach (var t in requiredTypes)
                {
                    if (t == kv.Key)
                    {
                        needed = true;
                        break;
                    }
                }
            }

            if (kv.Value != null)
                kv.Value.SetActive(needed);
        }

        mainCamera.enabled = false;
        storyCamera.enabled = true;

        runner.ResetUI();
        activeChapter.BuildActions(runner);
        runner.Run(activeChapter.nextStage);
    }

    private void SetAllChaptersActive(bool active)
    {
        SetChaptersActive(mainChapters, active);
        SetChaptersActive(promoteChapters, active);
    }

    private void SetChaptersActive(StoryChapter[] chapters, bool active)
    {
        if (chapters == null) return;

        foreach (var c in chapters)
        {
            if (c != null)
                c.SetChapterActive(active);
        }
    }

    private void SwitchToBattle()
    {
        // 챕터 종료 fadeOut 은 runner 내부에서 처리 후 호출됨
        ResetAllModelPositionsAfterStory();

        DisableLocalTalkersOfAllChapters();

        if (activeChapter != null)
        {
            activeChapter.SetChapterActive(false);
            activeChapter = null;
        }

        // 모든 파티 모델은 다시 활성화
        foreach (var m in modelMap.Values)
        {
            if (m != null)
                m.SetActive(true);
        }

        storyCamera.enabled = false;
        mainCamera.enabled = true;
    }

    // 이 함수는 fadeOut 끝난 뒤 실행됨
    public void ResetAllModelPositionsAfterStory()
    {
        foreach (var kv in originalPositions)
        {
            GameObject model = kv.Key;
            Vector3 pos = kv.Value;

            if (model != null)
                model.transform.position = pos;
        }
    }

    public StoryTalker GetTalker(StoryRenderType type)
    {
        if (talkerMap.TryGetValue(type, out var talker))
            return talker;

        return null;
    }

    public GameObject GetModel(StoryRenderType type)
    {
        if (modelMap.TryGetValue(type, out var model))
            return model;

        return null;
    }
}
