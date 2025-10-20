using EnumCollection;
using Google.MiniJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Unity.Services.RemoteConfig;
using UnityEngine;
using static PriceInfo;
using Random = UnityEngine.Random;

public class CurrencyManager : MonoBehaviour
{
    // ======================================
    // [FIELDS]
    // ======================================

    private GameData _gameData;
    private readonly DataTable _dataTable = new();

    public static CurrencyManager instance;

    [SerializeField] private PriceInfo _priceInfo;

    // --- Gold ---
    private string goldFormula;
    private float goldRange;
    private List<int> goldBonusStages = new();
    private float goldBonusValue;

    // --- Exp ---
    private string expFormula;
    private float expRange;
    private List<int> expBonusStages = new();
    private float expBonusValue;
    private bool _ExpPassiveOn = false;
    private float _expPlusPercent;

    // --- Fragment ---
    private string fragmentFormula;
    private float fragmentRange;
    private int fragmentDropInterval;
    private readonly Dictionary<string, float> rarityAdjust = new();
    private readonly List<(int min, int max, List<string> pattern)> fragmentDistribute = new();
    private readonly Dictionary<int, string> forceAssign = new();

    // --- Weapon ---
    private readonly Dictionary<int, List<string>> weaponByStage = new();

    // --- Drop Result ---
    public int currentGoldValue;
    public int currentExpValue;
    public (Rarity rarity, int count) currentFragmentValue;
    public string currentWeaponValue;

    // --- Misc ---
    private string _requireExpFormula;

    public Sprite[] _fragmentSprites;
    public Sprite _goldSprite;
    public Sprite _expSprite;
    public Sprite _diaSprite;
    public Sprite _cloverSprite;
    public Color[] rarityColor;

    // --- Constants ---
    public const int MAXPLAYERSKILLLEVEL = 10;
    public const int MAXCOMPANIONSKILLLEVEL = 20;
    public const int MAXWEAPONLEVEL = 20;


    // ======================================
    // [UNITY LIFECYCLE]
    // ======================================

    private void Awake()
    {
        instance = this;
        _gameData = StartBroker.GetGameData();

        LoadGoldFormula();
        LoadExpFormula();
        LoadFragmentFormula(RemoteConfigService.Instance.appConfig.GetJson("FRAGMENT_DROP_FORMULA", "None"));
        LoadWeaponTable(RemoteConfigService.Instance.appConfig.GetJson("WEAPON_DROP_FORMULA", "None"));

        _requireExpFormula = RemoteConfigService.Instance.appConfig.GetString("LEVEL_UP_REQUIRE_EXP", "None");

        BattleBroker.OnStageChange += OnStageChange;
        BattleBroker.OnDrop += OnDrop;
        PlayerBroker.OnLevelExpSet += OnLevelExpSet;
        BattleBroker.GetNeedExp = GetNeedExp;
    }

    private void Start()
    {
        PlayerBroker.GetResourceSprite = GetResourceSprite;
        PlayerBroker.GetFragmentSprite = GetFragmentSprite;
    }


    // ======================================
    // [REMOTE CONFIG LOADERS]
    // ======================================

    private void LoadGoldFormula()
    {
        string json = RemoteConfigService.Instance.appConfig.GetJson("GOLD_DROP_FORMULA", "None");
        var dict = Json.Deserialize(json) as Dictionary<string, object>;
        goldFormula = dict["Formula"].ToString();
        goldRange = Convert.ToSingle(dict["Range"]);

        if (dict.ContainsKey("Bonus") && dict["Bonus"] is List<object> list)
            foreach (var item in list) goldBonusStages.Add(Convert.ToInt32(item));

        if (dict.ContainsKey("BonusValue"))
            goldBonusValue = Convert.ToSingle(dict["BonusValue"]);
    }

    private void LoadExpFormula()
    {
        string json = RemoteConfigService.Instance.appConfig.GetJson("EXP_DROP_FORMULA", "None");
        var dict = Json.Deserialize(json) as Dictionary<string, object>;
        expFormula = dict["Formula"].ToString();
        expRange = Convert.ToSingle(dict["Range"]);

        if (dict.ContainsKey("Bonus") && dict["Bonus"] is List<object> list)
            foreach (var item in list) expBonusStages.Add(Convert.ToInt32(item));

        if (dict.ContainsKey("BonusValue"))
            expBonusValue = Convert.ToSingle(dict["BonusValue"]);
    }

    private void LoadFragmentFormula(string json)
    {
        var root = Json.Deserialize(json) as Dictionary<string, object>;
        fragmentFormula = root["Formula"].ToString();
        fragmentDropInterval = Convert.ToInt32(root["DropInterval"]);
        fragmentRange = Convert.ToSingle(root["Range"]);

        var rarityDict = root["RarityAdjust"] as Dictionary<string, object>;
        foreach (var kv in rarityDict)
            rarityAdjust[kv.Key] = Convert.ToSingle(kv.Value);

        var valueDistribute = root["ValueDistribute"] as Dictionary<string, object>;
        foreach (var kv in valueDistribute)
        {
            int min, max;
            var split = kv.Key.Split('-', '~');
            if (split.Length == 1)
                min = max = int.Parse(split[0]);
            else
            {
                min = int.Parse(split[0]);
                max = int.Parse(split[1]);
            }

            List<string> pattern = new();
            if (kv.Value is string single)
                pattern.Add(single);
            else if (kv.Value is List<object> list)
                foreach (var v in list) pattern.Add(v.ToString());

            fragmentDistribute.Add((min, max, pattern));
        }

        if (root.ContainsKey("ForceAssign"))
        {
            var forceDict = root["ForceAssign"] as Dictionary<string, object>;
            foreach (var kv in forceDict)
                forceAssign[int.Parse(kv.Key)] = kv.Value.ToString();
        }
    }

    private void LoadWeaponTable(string json)
    {
        if (Json.Deserialize(json) is not Dictionary<string, object> root) return;
        if (!root.ContainsKey("WeaponByStage")) return;

        weaponByStage.Clear();
        var map = root["WeaponByStage"] as Dictionary<string, object>;
        if (map == null) return;

        foreach (var kv in map)
        {
            int stage = int.Parse(kv.Key);
            var list = new List<string>();
            if (kv.Value is string single)
                list.Add(single);
            else if (kv.Value is List<object> many)
                foreach (var m in many) list.Add(m.ToString());
            weaponByStage[stage] = list;
        }
    }


    // ======================================
    // [DROP HANDLING]
    // ======================================

    private void OnDrop(DropType type, int value, string id)
    {
        switch (type)
        {
            case DropType.Gold: GetGoldByDrop(value); break;
            case DropType.Exp: GetExpByDrop(value); break;
            case DropType.Fragment: GetFragmentByDrop(id, value); break;
            case DropType.Weapon: GetWeaponByDrop(id, value); break;
        }
    }

    private void OnStageChange()
    {
        int stage = _gameData.currentStageNum;

        currentGoldValue = GetBaseGoldValue(stage);
        currentExpValue = GetBaseExpValue(stage);

        if (goldBonusStages.Contains(stage))
            currentGoldValue = Mathf.CeilToInt(currentGoldValue * (1f + goldBonusValue));

        if (expBonusStages.Contains(stage))
            currentExpValue = Mathf.CeilToInt(currentExpValue * (1f + expBonusValue));

        currentFragmentValue = GetBaseFragmentValue(stage);
        currentWeaponValue = GetWeaponValue(stage);
    }


    // ======================================
    // [INDIVIDUAL DROP PROCESSORS]
    // ======================================

    private void GetGoldByDrop(int value)
    {
        _gameData.gold += value;
        PlayerBroker.OnGoldSet();
        NetworkBroker.QueueResourceReport(value, null, Resource.Gold, Source.Battle);
    }

    private void GetExpByDrop(int value)
    {
        if (_ExpPassiveOn)
            value += Mathf.CeilToInt(value * _expPlusPercent / 100f);

        _gameData.exp += value;
        PlayerBroker.OnLevelExpSet();
        NetworkBroker.QueueResourceReport(value, null, Resource.Exp, Source.Battle);
    }

    private void GetWeaponByDrop(string id, int value)
    {
        var dict = _gameData.weaponCount;
        if (!dict.ContainsKey(id)) dict.Add(id, value);
        else dict[id] += value;

        PlayerBroker.OnWeaponCountSet(id, dict[id]);
        NetworkBroker.QueueResourceReport(value, id, Resource.Weapon, Source.Battle);
    }

    private void GetFragmentByDrop(string id, int value)
    {
        Rarity rarity = Enum.Parse<Rarity>(id);
        var dict = _gameData.skillFragment;
        if (!dict.ContainsKey(rarity)) dict.Add(rarity, value);
        else dict[rarity] += value;

        PlayerBroker.OnFragmentSet();
    }


    // ======================================
    // [LEVEL / EXP]
    // ======================================

    private void OnLevelExpSet()
    {
        while (true)
        {
            BigInteger needExp = BattleBroker.GetNeedExp();
            if (_gameData.exp < needExp) break;

            _gameData.exp -= needExp;
            _gameData.level++;
            _gameData.statPoint++;
            PlayerBroker.OnStatPointSet();
        }
    }

    private BigInteger GetNeedExp()
    {
        object resultObj = _dataTable.Compute(_requireExpFormula.Replace("{level}", _gameData.level.ToString()), null);
        return Convert.ToInt32(resultObj);
    }


    // ======================================
    // [VALUE CALCULATION HELPERS]
    // ======================================

    private int EvaluateFormula(string formula, int stageNum)
    {
        string expr = formula.Replace("{stageNum}", stageNum.ToString());
        object obj = _dataTable.Compute(expr, null);
        return Convert.ToInt32(obj);
    }

    public int GetBaseGoldValue(int stageNum) => EvaluateFormula(goldFormula, stageNum);
    public int GetBaseExpValue(int stageNum) => EvaluateFormula(expFormula, stageNum);

    public (Rarity rarity, int count) GetBaseFragmentValue(int stageNum)
    {
        if (forceAssign.ContainsKey(stageNum))
        {
            string rarityStr = forceAssign[stageNum];
            Rarity rarity = Enum.Parse<Rarity>(rarityStr);
            int baseVal = EvaluateFormula(fragmentFormula, stageNum);
            float adj = rarityAdjust.ContainsKey(rarityStr) ? rarityAdjust[rarityStr] : 1f;
            return (rarity, Mathf.Max(1, Mathf.RoundToInt(baseVal * adj)));
        }

        if (stageNum % fragmentDropInterval != 0)
            return (Rarity.Common, 0);

        string rarityName = "Common";
        foreach (var range in fragmentDistribute)
        {
            if (stageNum >= range.min && stageNum <= range.max)
            {
                if (range.pattern.Count == 1)
                    rarityName = range.pattern[0];
                else
                {
                    int dropIdx = stageNum / fragmentDropInterval;
                    int rarityIdx = dropIdx % range.pattern.Count;
                    rarityName = range.pattern[rarityIdx];
                }
                break;
            }
        }

        Rarity r = Enum.Parse<Rarity>(rarityName);
        int baseValue = EvaluateFormula(fragmentFormula, stageNum);
        float adjVal = rarityAdjust.ContainsKey(rarityName) ? rarityAdjust[rarityName] : 1f;

        return (r, Mathf.Max(1, Mathf.RoundToInt(baseValue * adjVal)));
    }

    public int GetGoldRangedValue()
    {
        int min = Mathf.Max(1, Mathf.FloorToInt(currentGoldValue * (1 - goldRange)));
        int max = Mathf.CeilToInt(currentGoldValue * (1 + goldRange)) + 1;
        return Random.Range(min, max);
    }

    public int GetExpRangedValue()
    {
        int min = Mathf.Max(1, Mathf.FloorToInt(currentExpValue * (1 - expRange)));
        int max = Mathf.CeilToInt(currentExpValue * (1 + expRange)) + 1;
        return Random.Range(min, max);
    }

    public (Rarity rarity, int count) GetFragmentRangedValue()
    {
        var rarity = currentFragmentValue.rarity;
        int baseCount = currentFragmentValue.count;
        if (baseCount <= 0) return (rarity, 0);

        int min = Mathf.Max(1, Mathf.FloorToInt(baseCount * (1 - fragmentRange)));
        int max = Mathf.CeilToInt(baseCount * (1 + fragmentRange)) + 1;
        return (rarity, Random.Range(min, max));
    }

    public string GetWeaponValue(int stageNum)
    {
        if (!weaponByStage.TryGetValue(stageNum, out var list) || list == null || list.Count == 0)
            return null;

        return list.Count == 1 ? list[0] : list[stageNum % list.Count];
    }


    // ======================================
    // [RESOURCE REQUIREMENT FUNCTIONS]
    // ======================================

    public int GetRequireFragment_Skill(Rarity rarity, int level) =>
        rarity switch
        {
            Rarity.Common => _priceInfo.commonSkillPrice[level],
            Rarity.Uncommon => _priceInfo.uncommonSkillPrice[level],
            Rarity.Rare => _priceInfo.rareSkillPrice[level],
            Rarity.Unique => _priceInfo.uniqueSkillPrice[level],
            Rarity.Legendary => _priceInfo.legendarySkillPrice[level],
            Rarity.Mythic => _priceInfo.mythicSkillPrice[level],
            _ => int.MaxValue
        };

    public int GetRequireWeaponCount(Rarity rarity, int level) =>
        rarity switch
        {
            Rarity.Common => _priceInfo.commonWeaponPrice[level],
            Rarity.Uncommon => _priceInfo.uncommonWeaponPrice[level],
            Rarity.Rare => _priceInfo.rareWeaponPrice[level],
            Rarity.Unique => _priceInfo.uniqueWeaponPrice[level],
            Rarity.Legendary => _priceInfo.legendaryWeaponPrice[level],
            Rarity.Mythic => _priceInfo.mythicWeaponPrice[level],
            _ => int.MaxValue
        };

    public CompanionSkillPrice GetRequireCompanionSkill_CloverFragment(int companionIndex, int skillIndex, int skillLevel)
    {
        CompanionSkillPrice price = new();

        switch (companionIndex)
        {
            case 0:
                if (skillIndex == 0) price = _priceInfo.companion0_SkillPrice0[skillLevel];
                else if (skillIndex == 1) price = _priceInfo.companion0_SkillPrice1[skillLevel];
                else if (skillIndex == 2) price = _priceInfo.companion0_SkillPrice2[skillLevel];
                break;

            case 1:
                if (skillIndex == 0) price = _priceInfo.companion1_SkillPrice0[skillLevel];
                else if (skillIndex == 1) price = _priceInfo.companion1_SkillPrice1[skillLevel];
                else if (skillIndex == 2) price = _priceInfo.companion1_SkillPrice2[skillLevel];
                break;

            case 2:
                if (skillIndex == 0) price = _priceInfo.companion2_SkillPrice0[skillLevel];
                else if (skillIndex == 1) price = _priceInfo.companion2_SkillPrice1[skillLevel];
                else if (skillIndex == 2) price = _priceInfo.companion2_SkillPrice2[skillLevel];
                break;
        }

        return price;
    }


    // ======================================
    // [UI / BROKER HELPERS]
    // ======================================

    private Sprite GetResourceSprite(Resource resource) =>
        resource switch
        {
            Resource.Gold => _goldSprite,
            Resource.Exp => _expSprite,
            Resource.Dia => _diaSprite,
            Resource.Clover => _cloverSprite,
            _ => null
        };

    private Sprite GetFragmentSprite(Rarity rarity) => _fragmentSprites[(int)rarity];
}
