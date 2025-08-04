using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.RemoteConfig;
using UnityEditor;
using UnityEngine;

public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;
    [Header("Normal Stage")]
    [SerializeField] StageInfo[] _normalStageInfoArr;
    [Header("Companion Tech Stage")]
    [SerializeField] StageInfo[] _companion_0_1;
    [SerializeField] StageInfo[] _companion_0_2;
    [SerializeField] StageInfo[] _companion_0_3;
    [SerializeField] StageInfo[] _companion_1_1;
    [SerializeField] StageInfo[] _companion_1_2;
    [SerializeField] StageInfo[] _companion_1_3;
    [SerializeField] StageInfo[] _companion_2_1;
    [SerializeField] StageInfo[] _companion_2_2;
    [SerializeField] StageInfo[] _companion_2_3;
    [Header("Adventure Stage")]
    [SerializeField] StageInfo[] _adventure_0;
    [SerializeField] StageInfo[] _adventure_1;
    [SerializeField] StageInfo[] _adventure_2;
    [SerializeField] StageInfo[] _adventure_3;
    [SerializeField] StageInfo[] _adventure_4;
    [SerializeField] StageInfo[] _adventure_5;
    [SerializeField] StageInfo[] _adventure_6;
    [SerializeField] StageInfo[] _adventure_7;
    [SerializeField] StageInfo[] _adventure_8;

    [Header("Region")]
    [SerializeField] StageRegion[] _stageRegionArr;
    [Header("AdventureReward")]
    public int adventureDiaIncrease;
    public int adventureCloverIncrease;
    public List<(int, int)> adventureRewardList = new();//dia, clover
    public int adventureEntranceFee;
    [Header("CompanionReward")]
    public List<(int, int, int, int)> companionRewardList = new();//dia, clover, diaIncrease, cloverIncrease

    public StageRegion GetRegionInfo(int index) => _stageRegionArr[index];



    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        SetAdventureReward();
        SetCompanionReward();
        BattleBroker.GetCompanionReward += GetCompanionReward;
        BattleBroker.GetAdventureReward += GetAdventureReward;
    }

    private (int, int) GetCompanionReward(int index_0, int index_1)
    {
        (int, int, int, int) reward = companionRewardList[index_0];
        return new(reward.Item1 + reward.Item3 * index_1, reward.Item2 + reward.Item4 * index_1);
    }
    private (int, int) GetAdventureReward(int index_0, int index_1)
    {
        (int, int) reward = adventureRewardList[index_0];
        return new(reward.Item1 + adventureDiaIncrease*index_1, reward.Item2 + adventureCloverIncrease * index_1);
    }

    private void SetCompanionReward()
    {
        string rewardJson = RemoteConfigService.Instance.appConfig.GetJson("COMPANION_REWARD", "None");
        Dictionary<string, object> rewardDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardJson);
        for (int i = 0; i < 3; i++)
        {
            Dictionary<string, string> adventureRewardDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(rewardDict[$"Companion_{i}"]));
            int dia = int.Parse(adventureRewardDict["Dia"]);
            int clover = int.Parse(adventureRewardDict["Clover"]);
            int diaIncrease = int.Parse(adventureRewardDict["DiaIncrease"]);
            int cloverIncrease = int.Parse(adventureRewardDict["CloverIncrease"]);
            companionRewardList.Add(new(dia, clover, diaIncrease, cloverIncrease));
        }
    }

    private void SetAdventureReward()
    {
        string rewardJson = RemoteConfigService.Instance.appConfig.GetJson("ADVENTURE_REWARD", "None");
        Dictionary<string, object> rewardDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardJson);
        adventureDiaIncrease = Convert.ToInt32(rewardDict["DiaIncrease"]);
        adventureCloverIncrease = Convert.ToInt32(rewardDict["CloverIncrease"]);
        for (int i = 0; i < 9; i++)
        {
            Dictionary<string, string> adventureRewardDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString( rewardDict[$"Adventure_{i}"]));
            int dia = int.Parse(adventureRewardDict["Dia"]);
            int clover = int.Parse(adventureRewardDict["Clover"]);
            adventureRewardList.Add(new(dia, clover));
        }
        adventureEntranceFee = Convert.ToInt32(rewardDict["EntranceFee"]);
    }

    public List<IListViewItem> GetStageInfosAsItem(int start, int count)
    {
        List<IListViewItem> items = new();

        // 유효성 검사
        if (_normalStageInfoArr == null || start < 0 || count <= 0 || start >= _normalStageInfoArr.Length)
        {
            return items;
        }

        // 지정된 범위만큼 데이터를 가져옴
        int end = Mathf.Min(start + count, _normalStageInfoArr.Length);
        for (int i = start; i < end; i++)
        {
            items.Add(_normalStageInfoArr[i]);
        }

        return items;
    }
    public StageInfo GetNormalStageInfo(int stageNum) => _normalStageInfoArr[stageNum-1];
    public StageInfo GetCompanionTechStageInfo(int companionIndex, (int, int) companionTech)
    {
        StageInfo result = null;
        switch (companionIndex)
        {
            case 0:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_0_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_0_2[companionTech.Item2];
                        break;
                    case 3:
                        result =  _companion_0_3[companionTech.Item2];
                        break;
                }
                break;
            case 1:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_1_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_1_2[companionTech.Item2];
                        break;
                    case 3:
                        result =  _companion_1_3[companionTech.Item2];
                        break;
                }
                break;
            case 2:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_2_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_2_2[companionTech.Item2];
                        break;
                    case 3:
                        result = _companion_2_3[companionTech.Item2];
                        break;
                }
                break;
        }
        return result;
    }
#if UNITY_EDITOR
    [ContextMenu("SetDefaultStatus")]
    public void SetDefaultStatus()
    {
        foreach (StageInfo x in _normalStageInfoArr)
        {
            x.enemyStatusFromStage.maxHp = 10.ToString();
            x.enemyStatusFromStage.resist = 0f;
            EditorUtility.SetDirty(x);
        }
        foreach (StageInfo x in _normalStageInfoArr)
        {
            x.bossStatusFromStage.maxHp = 100.ToString();
            x.bossStatusFromStage.resist = 0f;
            x.bossStatusFromStage.power = 10.ToString();
            x.bossStatusFromStage.penetration = 0f;
            EditorUtility.SetDirty(x);
        }
        // 변경된 데이터를 애셋 파일에 저장
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [ContextMenu("SetDefaultReward")]
    public void SetDefaultReward()
    {
        // 변경된 데이터를 애셋 파일에 저장
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public StageInfo[] GetAdventureStageInfo(int currentSlotIndex)
    {
        switch (currentSlotIndex)
        {
            case 0:
                return _adventure_0;
            case 1:
                return _adventure_1;
            case 2:
                return _adventure_2;
            case 3:
                return _adventure_3;
            case 4:
                return _adventure_4;
            case 5:
                return _adventure_5;
            case 6:
                return _adventure_6;
            case 7:
                return _adventure_7;
            case 8:
                return _adventure_8;
            default:
                return null;
        }
    }
#endif
}