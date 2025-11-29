using EnumCollection;
using UnityEngine;
using System.Collections.Generic;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance;

    [SerializeField] private StoryRunner runner;
    [SerializeField] private StoryChapter[] chapters;

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

    // ★ 원래 위치 저장
    private Dictionary<GameObject, Vector3> originalPositions = new();


    private void Awake()
    {
        instance = this;
        if (mainCamera == null)
            mainCamera = Camera.main;

        InitMaps();

        // 모든 Chapter를 활성화시키되 본문은 꺼둔다
        foreach (var c in chapters)
        {
            if (c != null)
            {
                c.gameObject.SetActive(true);
                c.SetChapterActive(false);
            }
        }

        // Local talker 전부 OFF
        DisableLocalTalkersOfAllChapters();

        // ★ 원래 위치 저장 (동료 + 플레이어 + 챕터별 local 모델)
        SaveOriginalPositions();

        PlayerBroker.OnPlayerAppearanceChange += ad =>
        {
            protagonistModel.GetComponentInChildren<AppearanceController>()
                ?.SetAppearance(ad);
        };

        BattleBroker.SwitchToStory += RunStory;
        BattleBroker.SwitchToBattle += SwitchToBattle;
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

    private void SaveOriginalPositions()
    {
        // 플레이어 + 동료
        foreach (var kv in modelMap)
        {
            if (kv.Value != null)
                originalPositions[kv.Value] = kv.Value.transform.position;
        }

        // ★ 각 chapter의 local models도 저장
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
        foreach (var c in chapters)
        {
            var locals = c?.LocalTalkers;
            if (locals == null) continue;

            foreach (var t in locals)
                if (t != null && t.talkerObject != null)
                    t.talkerObject.SetActive(false);
        }
    }

    private void RunStory(int index)
    {
        if (index < 0 || index >= chapters.Length)
            return;

        DisableLocalTalkersOfAllChapters();

        foreach (var c in chapters)
            c.SetChapterActive(false);

        activeChapter = chapters[index];
        activeChapter.SetChapterActive(true);

        // Local talker ON
        var locals = activeChapter.LocalTalkers;
        if (locals != null)
        {
            foreach (var t in locals)
                if (t != null && t.talkerObject != null)
                    t.talkerObject.SetActive(true);
        }

        // 필요한 모델만 Enable
        foreach (var kv in modelMap)
        {
            bool needed = false;

            foreach (var t in activeChapter.GetRequiredRenderTypes())
                if (t == kv.Key)
                    needed = true;

            kv.Value.SetActive(needed);
        }

        mainCamera.enabled = false;
        storyCamera.enabled = true;

        runner.ResetUI();
        activeChapter.BuildActions(runner);
        runner.Run();
    }

    private void SwitchToBattle()
    {
        // 챕터 종료 → fadeOut은 runner 내부에 있음 → fadeOut 완료 후 호출됨
        ResetAllModelPositionsAfterStory();

        DisableLocalTalkersOfAllChapters();

        if (activeChapter != null)
        {
            activeChapter.SetChapterActive(false);
            activeChapter = null;
        }

        // 모든 파티 모델은 다시 활성화
        foreach (var m in modelMap.Values)
            m.SetActive(true);

        storyCamera.enabled = false;
        mainCamera.enabled = true;
    }

    // ★ 이 함수는 fadeOut 끝난 뒤 실행됨
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
