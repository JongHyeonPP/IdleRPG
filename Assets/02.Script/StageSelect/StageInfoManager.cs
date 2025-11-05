using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.RemoteConfig;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스테이지 데이터의 단일 진입점
/// 노말 스테이지와 동료 승급 스테이지와 어드벤처와 던전 스테이지 정보를 제공하고
/// 원격 설정에서 보상 테이블을 읽어 캐시한 뒤 브로커에 질의 함수를 노출한다
/// </summary>
public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;

    #region Stage Datas in Inspector
    [Header("Normal Stage")]
    [SerializeField] StageInfo[] _normalStageInfoArr;       // 1 기반 인덱스로 접근

    [Header("Companion Tech Stage")]
    [SerializeField] StageInfo[] _companion_0_1;            // 동료 0의 테크 1 라인
    [SerializeField] StageInfo[] _companion_0_2;            // 동료 0의 테크 2 라인
    [SerializeField] StageInfo[] _companion_0_3;            // 동료 0의 테크 3 라인
    [SerializeField] StageInfo[] _companion_1_1;            // 동료 1의 테크 1 라인
    [SerializeField] StageInfo[] _companion_1_2;            // 동료 1의 테크 2 라인
    [SerializeField] StageInfo[] _companion_1_3;            // 동료 1의 테크 3 라인
    [SerializeField] StageInfo[] _companion_2_1;            // 동료 2의 테크 1 라인
    [SerializeField] StageInfo[] _companion_2_2;            // 동료 2의 테크 2 라인
    [SerializeField] StageInfo[] _companion_2_3;            // 동료 2의 테크 3 라인

    [Header("Adventure Stage")]
    [SerializeField] StageInfo[] _adventure_0;              // 어드벤처 챕터 0
    [SerializeField] StageInfo[] _adventure_1;
    [SerializeField] StageInfo[] _adventure_2;
    [SerializeField] StageInfo[] _adventure_3;
    [SerializeField] StageInfo[] _adventure_4;
    [SerializeField] StageInfo[] _adventure_5;
    [SerializeField] StageInfo[] _adventure_6;
    [SerializeField] StageInfo[] _adventure_7;
    [SerializeField] StageInfo[] _adventure_8;

    [Header("Dungeon")]
    [SerializeField] StageInfo[] _dungeon_0;                // 던전 0 라인업
    [SerializeField] StageInfo[] _dungeon_1;                // 던전 1 라인업
    [SerializeField] StageInfo[] _dungeon_2;                // 던전 2 라인업
    [Header("Promote")]
    [SerializeField] StageInfo[] _promoteArr;
    [Header("Region")]
    [SerializeField] StageRegion[] _stageRegionArr;         // 지역 정보 배열

    [Header("AdventureReward")]
    public int adventureDiaIncrease;                        // 어드벤처 단계별 다이아 증가량
    public int adventureCloverIncrease;                     // 어드벤처 단계별 클로버 증가량
    public List<(int, int)> adventureRewardList = new();    // 각 챕터의 기본 보상 다이아와 클로버

    // 던전 보상 캐시
    // 인덱스 0은 골드
    // 인덱스 1은 스킬 조각
    // 인덱스 2는 클로버
    private Dictionary<int, DungeonReward>[] dungeonRewards = new Dictionary<int, DungeonReward>[3];

    public int adventureEntranceFee;                        // 어드벤처 입장료
    public int dungeonEntranceFee;                        // 어드벤처 입장료

    [Header("CompanionReward")]
    public List<(int, int, int, int)> companionRewardList = new(); // 기본 보상 다이아와 클로버와 단계 증가량 둘
    #endregion
    /// <summary>
    /// 지역 정보 조회
    /// </summary>
    public StageRegion GetRegionInfo(int index) => _stageRegionArr[index];

    private void Awake()
    {
        // 싱글톤 보장
        if (!instance) instance = this;
        else { Destroy(gameObject); return; }

        // 원격 설정 기반 보상 테이블 로딩과 캐시
        SetAdventureReward();
        SetCompanionReward();
        SetDungeonReward();

        // 브로커에 보상 질의 핸들러 연결
        BattleBroker.GetCompanionReward += GetCompanionReward;
        BattleBroker.GetAdventureReward += GetAdventureReward;
        BattleBroker.GetDungeonReward += GetDungeonReward;

        
    }
    private void Start()
    {
        SetDropInfo();
    }
    /// <summary>
    /// 동료 승급 전투 보상 계산
    /// index_0는 동료 인덱스
    /// index_1는 직전까지의 단계 수
    /// 기본 보상에 단계 증가량을 곱해 누적
    /// </summary>
    private (int, int) GetCompanionReward(int index_0, int index_1)
    {
        var reward = companionRewardList[index_0];
        return new(reward.Item1 + reward.Item3 * index_1,
                   reward.Item2 + reward.Item4 * index_1);
    }

    /// <summary>
    /// 어드벤처 보상 계산
    /// index_0는 챕터 인덱스
    /// index_1는 스테이지 단계
    /// 챕터 기본 보상에 단계 증가량을 누적
    /// </summary>
    private (int, int) GetAdventureReward(int index_0, int index_1)
    {
        var reward = adventureRewardList[index_0];
        return new(reward.Item1 + adventureDiaIncrease * index_1,
                   reward.Item2 + adventureCloverIncrease * index_1);
    }

    /// <summary>
    /// 던전 보상 조회
    /// 던전 인덱스와 스테이지 인덱스로 캐시에서 찾아 반환
    /// </summary>
    public DungeonReward GetDungeonReward(int dungeonIndex, int stageIndex)
    {
        if (dungeonIndex < 0 || dungeonIndex >= dungeonRewards.Length)
        {
            Debug.LogError($"Invalid dungeonIndex: {dungeonIndex}");
            return null;
        }

        if (dungeonRewards[dungeonIndex] == null || !dungeonRewards[dungeonIndex].ContainsKey(stageIndex))
        {
            Debug.LogError($"Dungeon reward not found for index {dungeonIndex}, stage {stageIndex}");
            return null;
        }

        return dungeonRewards[dungeonIndex][stageIndex];
    }

    /// <summary>
    /// 동료 승급 보상 테이블 로드
    /// Remote Config 키 COMPANION_REWARD
    /// Companion_i 딕셔너리를 읽어 기본 보상과 단계 증가량을 기록
    /// </summary>
    private void SetCompanionReward()
    {
        string rewardJson = RemoteConfigService.Instance.appConfig.GetJson("COMPANION_REWARD", "None");
        var rewardDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardJson);

        for (int i = 0; i < 3; i++)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(rewardDict[$"Companion_{i}"]));
            int dia = int.Parse(map["Dia"]);
            int clover = int.Parse(map["Clover"]);
            int diaIncrease = int.Parse(map["DiaIncrease"]);
            int cloverIncrease = int.Parse(map["CloverIncrease"]);
            companionRewardList.Add(new(dia, clover, diaIncrease, cloverIncrease));
        }
    }

    /// <summary>
    /// 어드벤처 보상 테이블 로드
    /// Remote Config 키 ADVENTURE_REWARD
    /// 챕터별 기본 보상과 전역 증가량 두 개 그리고 입장료를 기록
    /// </summary>
    private void SetAdventureReward()
    {
        string rewardJson = RemoteConfigService.Instance.appConfig.GetJson("ADVENTURE_REWARD", "None");
        var rewardDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardJson);

        adventureDiaIncrease = Convert.ToInt32(rewardDict["DiaIncrease"]);
        adventureCloverIncrease = Convert.ToInt32(rewardDict["CloverIncrease"]);

        for (int i = 0; i < 9; i++)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(rewardDict[$"Adventure_{i}"]));
            int dia = int.Parse(map["Dia"]);
            int clover = int.Parse(map["Clover"]);
            adventureRewardList.Add(new(dia, clover));
        }

        adventureEntranceFee = Convert.ToInt32(rewardDict["EntranceFee"]);
    }

    /// <summary>
    /// 던전 보상 테이블 로드
    /// Remote Config 키 DUNGEON_REWARD
    /// Dungeon_0은 골드 정수
    /// Dungeon_1은 조각 등급과 수량을 콤마로 구분한 문자열
    /// Dungeon_2는 클로버 정수
    /// EntranceFee는 던전 입장료로 저장
    /// </summary>
    private void SetDungeonReward()
    {
        string rewardJson = RemoteConfigService.Instance.appConfig.GetJson("DUNGEON_REWARD", "None");
        if (string.IsNullOrEmpty(rewardJson) || rewardJson == "None")
        {
            Debug.LogError("DUNGEON_REWARD not found in RemoteConfig");
            return;
        }

        var rewardDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardJson);

        // ===== 입장료(EntranceFee) 설정 =====
        if (rewardDict.TryGetValue("EntranceFee", out var feeObj))
            dungeonEntranceFee = Convert.ToInt32(feeObj);
        else
            dungeonEntranceFee = 0; // 없으면 0으로 처리

        // ===== 던전별 보상 설정 =====
        for (int dungeonIdx = 0; dungeonIdx < 3; dungeonIdx++)
        {
            string key = $"Dungeon_{dungeonIdx}";
            if (!rewardDict.ContainsKey(key)) continue;

            var dungeonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(rewardDict[key].ToString());
            var dict = new Dictionary<int, DungeonReward>();

            foreach (var pair in dungeonData)
            {
                // 상위 속성은 건너뜀
                if (pair.Key == "RewardType") continue;

                int stageIndex = int.Parse(pair.Key);

                switch (dungeonIdx)
                {
                    // 골드
                    case 0:
                        dict[stageIndex] = new DungeonReward(Resource.Gold, Convert.ToInt32(pair.Value));
                        break;

                    // 조각
                    case 1:
                        // 값 예시 Rare, 25
                        string[] parts = pair.Value.ToString().Split(',');
                        if (parts.Length == 2)
                        {
                            string rarityStr = parts[0].Trim();
                            int amount = int.Parse(parts[1].Trim());

                            if (Enum.TryParse(rarityStr, true, out Rarity rarity))
                                dict[stageIndex] = new DungeonReward(Resource.Fragment, amount, rarity);
                            else
                                Debug.LogError($"Invalid rarity: {rarityStr}");
                        }
                        break;

                    // 클로버
                    case 2:
                        dict[stageIndex] = new DungeonReward(Resource.Clover, Convert.ToInt32(pair.Value));
                        break;
                }
            }

            dungeonRewards[dungeonIdx] = dict;
        }

        Debug.Log($"[StageInfoManager] Dungeon rewards loaded. EntranceFee: {dungeonEntranceFee}");
    }


    /// <summary>
    /// 스테이지 셀 렌더용 간단 리스트 변환
    /// 시작 인덱스와 개수로 잘라서 IListViewItem 목록을 만든다
    /// </summary>
    public List<IListViewItem> GetStageInfosAsItem(int start, int count)
    {
        List<IListViewItem> items = new();

        if (_normalStageInfoArr == null || start < 0 || count <= 0 || start >= _normalStageInfoArr.Length)
            return items;

        int end = Mathf.Min(start + count, _normalStageInfoArr.Length);
        for (int i = start; i < end; i++)
            items.Add(_normalStageInfoArr[i]);

        return items;
    }

    /// <summary>
    /// 노말 스테이지 정보 조회
    /// 스테이지 번호는 1 기반
    /// </summary>
    public StageInfo GetNormalStageInfo(int stageNum) => _normalStageInfoArr[stageNum - 1];

    /// <summary>
    /// 동료 승급 스테이지 정보 조회
    /// companionIndex는 동료 인덱스
    /// companionTech는 테크 라인과 그 라인 안의 인덱스
    /// </summary>
    public StageInfo GetCompanionTechStageInfo(int companionIndex, (int, int) companionTech)
    {
        StageInfo result = null;

        switch (companionIndex)
        {
            case 0:
                switch (companionTech.Item1)
                {
                    case 1: result = _companion_0_1[companionTech.Item2]; break;
                    case 2: result = _companion_0_2[companionTech.Item2]; break;
                    case 3: result = _companion_0_3[companionTech.Item2]; break;
                }
                break;

            case 1:
                switch (companionTech.Item1)
                {
                    case 1: result = _companion_1_1[companionTech.Item2]; break;
                    case 2: result = _companion_1_2[companionTech.Item2]; break;
                    case 3: result = _companion_1_3[companionTech.Item2]; break;
                }
                break;

            case 2:
                switch (companionTech.Item1)
                {
                    case 1: result = _companion_2_1[companionTech.Item2]; break;
                    case 2: result = _companion_2_2[companionTech.Item2]; break;
                    case 3: result = _companion_2_3[companionTech.Item2]; break;
                }
                break;
        }

        return result;
    }

    /// <summary>
    /// 어드벤처 스테이지 배열 조회
    /// index는 챕터 인덱스
    /// </summary>
    public StageInfo[] GetAdventureStageInfo(int index)
    {
        switch (index)
        {
            case 0: return _adventure_0;
            case 1: return _adventure_1;
            case 2: return _adventure_2;
            case 3: return _adventure_3;
            case 4: return _adventure_4;
            case 5: return _adventure_5;
            case 6: return _adventure_6;
            case 7: return _adventure_7;
            case 8: return _adventure_8;
            default: return null;
        }
    }

    /// <summary>
    /// 던전 스테이지 배열 조회
    /// index는 던전 인덱스
    /// </summary>
    public StageInfo[] GetDungeonStageInfo(int index)
    {
        switch (index)
        {
            case 0: return _dungeon_0;
            case 1: return _dungeon_1;
            case 2: return _dungeon_2;
            default: return null;
        }
    }

    public StageInfo GetPromoteStageInfo(Rank rank)
    {
        return _promoteArr[(int)rank];
    }

    public (float goldBonusValue, float expBonusValue)GetBonusInfo(int stageNum)
    {
        if (stageNum <= 0 || stageNum > _normalStageInfoArr.Length)
            return (0f, 0f);

        var stage = _normalStageInfoArr[stageNum - 1];
        if (stage == null)
            return (0f, 0f);

        return (stage.goldBonusValue, stage.expBonusValue);
    }
    private void SetDropInfo()
    {
        // GOLD FORMULA
        string goldJson = Unity.Services.RemoteConfig.RemoteConfigService.Instance.appConfig.GetJson("GOLD_DROP_FORMULA", "None");
        if (string.IsNullOrEmpty(goldJson) || goldJson == "None")
        {
            Debug.LogError("GOLD_DROP_FORMULA not found in Remote Config.");
            return;
        }

        var goldDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(goldJson);
        string goldFormula = goldDict["Formula"].ToString();
        float goldRange = Convert.ToSingle(goldDict["Range"]);

        List<int> goldBonusStages = new();
        float goldBonusValue = 0f;
        if (goldDict.TryGetValue("Bonus", out var bonusObj))
        {
            var arr = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(bonusObj.ToString());
            foreach (var item in arr)
                goldBonusStages.Add(Convert.ToInt32(item));
        }
        if (goldDict.TryGetValue("BonusValue", out var bv))
            goldBonusValue = Convert.ToSingle(bv);

        // EXP FORMULA
        string expJson = Unity.Services.RemoteConfig.RemoteConfigService.Instance.appConfig.GetJson("EXP_DROP_FORMULA", "None");
        if (string.IsNullOrEmpty(expJson) || expJson == "None")
        {
            Debug.LogError("EXP_DROP_FORMULA not found in Remote Config.");
            return;
        }

        var expDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(expJson);
        string expFormula = expDict["Formula"].ToString();
        float expRange = Convert.ToSingle(expDict["Range"]);

        List<int> expBonusStages = new();
        float expBonusValue = 0f;
        if (expDict.TryGetValue("Bonus", out var expBonusObj))
        {
            var arr = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(expBonusObj.ToString());
            foreach (var item in arr)
                expBonusStages.Add(Convert.ToInt32(item));
        }
        if (expDict.TryGetValue("BonusValue", out var ev))
            expBonusValue = Convert.ToSingle(ev);

        // === Stage별로 설정 ===
        foreach (var stage in _normalStageInfoArr)
        {
            if (stage == null) continue;
            int num = stage.stageNum;

            // 보너스 세팅
            stage.goldBonusValue = goldBonusStages.Contains(num) ? goldBonusValue : 0f;
            stage.expBonusValue = expBonusStages.Contains(num) ? expBonusValue : 0f;

            // 기본 드랍 데이터는 CurrencyManager 계산 방식과 동일
            int baseGold = EvaluateFormula(goldFormula, num);
            int baseExp = EvaluateFormula(expFormula, num);
            var frag = CurrencyManager.instance.GetBaseFragmentValue(num);
            string weaponId = CurrencyManager.instance.GetWeaponValue(num);

            stage.fragmentDropInfo = frag.count > 0 ? frag : (Rarity.Common, 0);
            stage.weaponDropId = string.IsNullOrEmpty(weaponId) ? null : weaponId;
        }
    }



    /// <summary>
    /// 수식 계산 (DataTable.Compute)
    /// </summary>
    private int EvaluateFormula(string formula, int stageNum)
    {
        var table = new System.Data.DataTable();
        object result = table.Compute(formula.Replace("{stageNum}", stageNum.ToString()), null);
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// 프래그먼트 계산 로직 (forceAssign, interval, rarityAdjust 반영)
    /// </summary>
    private (Rarity rarity, int count)? CalculateFragmentDrop(
        string formula,
        float range,
        int dropInterval,
        Dictionary<string, float> rarityAdjust,
        Dictionary<string, string> forceAssign,
        int stageNum)
    {
        // 강제 지정 우선
        if (forceAssign.TryGetValue(stageNum.ToString(), out string forced))
        {
            Rarity rarity = Enum.Parse<Rarity>(forced);
            int baseVal = EvaluateFormula(formula, stageNum);
            float adj = rarityAdjust.ContainsKey(forced) ? rarityAdjust[forced] : 1f;
            return (rarity, Mathf.Max(1, Mathf.RoundToInt(baseVal * adj)));
        }

        // 드랍 주기 아닌 스테이지면 없음
        if (stageNum % dropInterval != 0)
            return (Rarity.Common, 0);

        // 기본 계산 (Common 기준)
        Rarity defaultRarity = Rarity.Common;
        int baseValue = EvaluateFormula(formula, stageNum);
        float adjust = rarityAdjust.ContainsKey(defaultRarity.ToString()) ? rarityAdjust[defaultRarity.ToString()] : 1f;
        int finalValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * adjust));
        return (defaultRarity, finalValue);
    }


}

