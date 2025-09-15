using EnumCollection;
using Google.MiniJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using Unity.Services.RemoteConfig;
using UnityEngine;
using static PriceInfo;
using Random = UnityEngine.Random;

public class CurrencyManager : MonoBehaviour
{
    private GameData _gameData;

    private string goldFormula;
    private float goldRange;

    private string expFormula;
    private float expRange;

    private string fragmentFormula;
    private float fragmentRange;

    private readonly Dictionary<string, float> rarityAdjust = new();
    private readonly List<(int min, int max, List<string> pattern)> fragmentDistribute = new();
    private int fragmentDropInterval;
    private readonly Dictionary<int, string> forceAssign = new();

    public int currentGoldValue;
    public int currentExpValue;
    public (Rarity rarity, int count) currentFragmentValue;
    public string currentWeaponValue;

    private readonly DataTable dataTable = new();
    public static CurrencyManager instance;

    private readonly DataTable _dataTable = new();

    private string _requireExpFormula;

    public Sprite[] _fragmentSprites;
    public Sprite _goldSprite;
    public Sprite _expSprite;
    public Sprite _diaSprite;
    public Sprite _cloverSprite;
    public Color[] rarityColor;

    [SerializeField] private PriceInfo _priceInfo;
    public const int MAXPLAYERSKILLLEVEL = 10;
    public const int MAXCOMPANIONSKILLLEVEL = 20;
    public const int MAXWEAPONLEVEL = 20;

    private readonly Dictionary<int, List<string>> weaponByStage = new();
    private bool _ExpPassiveOn=false;
    private float _expPlusPercent;
    private void Awake()
    {
        instance = this;
        _gameData = StartBroker.GetGameData();

        string goldJson = RemoteConfigService.Instance.appConfig.GetJson("GOLD_DROP_FORMULA", "None");
        var goldDict = Json.Deserialize(goldJson) as Dictionary<string, object>;
        goldFormula = goldDict["Formula"].ToString();
        goldRange = Convert.ToSingle(goldDict["Range"]);

        string expJson = RemoteConfigService.Instance.appConfig.GetJson("EXP_DROP_FORMULA", "None");
        var expDict = Json.Deserialize(expJson) as Dictionary<string, object>;
        expFormula = expDict["Formula"].ToString();
        expRange = Convert.ToSingle(expDict["Range"]);

        string fragmentJson = RemoteConfigService.Instance.appConfig.GetJson("FRAGMENT_DROP_FORMULA", "None");
        LoadFragmentFormula(fragmentJson);

        string weaponJson = RemoteConfigService.Instance.appConfig.GetJson("WEAPON_DROP_FORMULA", "None");
        if (!string.IsNullOrEmpty(weaponJson) && weaponJson != "None")
            LoadWeaponTable(weaponJson);

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

    private Sprite GetResourceSprite(Resource resource)
    {
        switch (resource)
        {
            case Resource.Gold:
                return _goldSprite;
            case Resource.Exp:
                return _expSprite;
            case Resource.Dia:
                return _diaSprite;
            case Resource.Clover:
                return _cloverSprite;
            case Resource.Scroll:
                break;
            case Resource.Fragment:
                throw new Exception("Use GetFragmentSprite");
            case Resource.None:
                break;
            case Resource.Weapon:
                break;
        }
        return null;
    }
    private Sprite GetFragmentSprite(Rarity rarity) => _fragmentSprites[(int)rarity];
    private void OnDrop(DropType type, int value, string id)
    {
        switch (type)
        {
            case DropType.Gold:
                GetGoldByDrop(value);
                break;
            case DropType.Exp:
                GetExpByDrop(value);
                break;
            case DropType.Fragment:
                GetFragmentByDrop(id, value);
                break;
            case DropType.Weapon:
                GetWeaponByDrop(id, value);
                break;
        }
    }

    private void GetWeaponByDrop(string id, int value)
    {
        var dict = _gameData.weaponCount;
        if (!dict.ContainsKey(id)) dict.Add(id, value);
        else dict[id] += value;
        PlayerBroker.OnWeaponCountSet(id, dict[id]);
        NetworkBroker.QueueResourceReport(value, id, Resource.Weapon, Source.Battle);
    }

    private void GetExpByDrop(int value)
    {
        Debug.Log($"원래경험치:{value}");
        if (_ExpPassiveOn)
        {
            value += Mathf.CeilToInt(value * _expPlusPercent / 100f);
        }
        Debug.Log($"추가된경험치:{value}");
        _gameData.exp += value;
        PlayerBroker.OnLevelExpSet();
        NetworkBroker.QueueResourceReport(value, null, Resource.Exp, Source.Battle);
    }
    public void PassiveOn(float expPercent)
    {
        _ExpPassiveOn = true;   
        _expPlusPercent = expPercent;
    }
    private void OnLevelExpSet()
    {
        while (true)
        {
            BigInteger needExp = BattleBroker.GetNeedExp();
            if (_gameData.exp < needExp)
                break;
            _gameData.exp -= needExp;
            _gameData.level++;
            _gameData.statPoint++;
            PlayerBroker.OnStatPointSet();
        }
    }

    private void GetGoldByDrop(int value)
    {
        _gameData.gold += value;
        PlayerBroker.OnGoldSet();
        NetworkBroker.QueueResourceReport(value, null, Resource.Gold, Source.Battle);
    }

    private BigInteger GetNeedExp()
    {
        object resultObj = _dataTable.Compute(_requireExpFormula.Replace("{level}", _gameData.level.ToString()), null);
        return Convert.ToInt32(resultObj);
    }

    private void GetFragmentByDrop(string id, int value)
    {
        Rarity rarity = Enum.Parse<Rarity>(id);
        var fragmentDict = _gameData.skillFragment;
        if (!fragmentDict.ContainsKey(rarity))
            fragmentDict.Add(rarity, value);
        else
            fragmentDict[rarity] += value;
        PlayerBroker.OnFragmentSet();
    }

    public int GetBaseGoldValue(int stageNum) => EvaluateFormula(goldFormula, stageNum);
    public int GetBaseExpValue(int stageNum) => EvaluateFormula(expFormula, stageNum);

    public (Rarity rarity, int count) GetBaseFragmentValue(int stageNum)
    {
        if (forceAssign.ContainsKey(stageNum))
        {
            string forcedRarityStr = forceAssign[stageNum];
            Rarity forcedRarity = Enum.Parse<Rarity>(forcedRarityStr);
            int baseValueF = EvaluateFormula(fragmentFormula, stageNum);
            float adjF = rarityAdjust.ContainsKey(forcedRarityStr) ? rarityAdjust[forcedRarityStr] : 1f;
            return (forcedRarity, Mathf.Max(1, Mathf.RoundToInt(baseValueF * adjF)));
        }

        if (stageNum % fragmentDropInterval != 0)
            return (Rarity.Common, 0);

        string rarityStr = "Common";
        foreach (var r in fragmentDistribute)
        {
            if (stageNum >= r.min && stageNum <= r.max)
            {
                if (r.pattern.Count == 1)
                {
                    rarityStr = r.pattern[0];
                }
                else
                {
                    int dropIndex = stageNum / fragmentDropInterval;
                    int rarityIndex = dropIndex % r.pattern.Count;
                    rarityStr = r.pattern[rarityIndex];
                }
                break;
            }
        }

        Rarity rarity = Enum.Parse<Rarity>(rarityStr);
        int baseValue = EvaluateFormula(fragmentFormula, stageNum);
        float adj = rarityAdjust.ContainsKey(rarityStr) ? rarityAdjust[rarityStr] : 1f;
        return (rarity, Mathf.Max(1, Mathf.RoundToInt(baseValue * adj)));
    }

    public int GetGoldRangedValue()
    {
        float range = goldRange;
        int min = Mathf.Max(1, Mathf.FloorToInt(currentGoldValue * (1 - range)));
        int max = Mathf.CeilToInt(currentGoldValue * (1 + range)) + 1;
        return Random.Range(min, max);
    }

    public int GetExpRangedValue()
    {
        float range = expRange;
        int min = Mathf.Max(1, Mathf.FloorToInt(currentExpValue * (1 - range)));
        int max = Mathf.CeilToInt(currentExpValue * (1 + range)) + 1;
        return Random.Range(min, max);
    }

    public (Rarity rarity, int count) GetFragmentRangedValue()
    {
        var rarity = currentFragmentValue.rarity;
        int baseCount = currentFragmentValue.count;

        if (baseCount <= 0)
            return (rarity, 0);

        float range = fragmentRange;
        int min = Mathf.Max(1, Mathf.FloorToInt(baseCount * (1 - range)));
        int max = Mathf.CeilToInt(baseCount * (1 + range)) + 1;
        int ranged = Random.Range(min, max);
        return (rarity, ranged);
    }

    public string GetWeaponValue(int stageNum)
    {
        if (weaponByStage.TryGetValue(stageNum, out var list))
        {
            if (list == null || list.Count == 0) return null;
            if (list.Count == 1) return list[0];
            int idx = stageNum % list.Count;
            return list[idx];
        }
        return null;
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
            int min;
            int max;
            var split = kv.Key.Split('-', '~');
            if (split.Length == 1)
            {
                min = max = int.Parse(split[0]);
            }
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
            {
                int stage = int.Parse(kv.Key);
                forceAssign[stage] = kv.Value.ToString();
            }
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
            var entry = kv.Value;

            var list = new List<string>();
            if (entry is string one)
            {
                list.Add(one);
            }
            else if (entry is List<object> many)
            {
                foreach (var m in many)
                    list.Add(m.ToString());
            }

            weaponByStage[stage] = list;
        }
    }

    private void OnStageChange()
    {
        int stage = _gameData.currentStageNum;
        currentGoldValue = GetBaseGoldValue(stage);
        currentExpValue = GetBaseExpValue(stage);
        currentFragmentValue = GetBaseFragmentValue(stage);
        currentWeaponValue = GetWeaponValue(stage);
    }

    private int EvaluateFormula(string formula, int stageNum)
    {
        string expr = formula.Replace("{stageNum}", stageNum.ToString());
        object obj = dataTable.Compute(expr, null);
        return Convert.ToInt32(obj);
    }

    public int GetRequireFragment_Skill(Rarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case Rarity.Common: return _priceInfo.commonSkillPrice[level];
            case Rarity.Uncommon: return _priceInfo.uncommonSkillPrice[level];
            case Rarity.Rare: return _priceInfo.rareSkillPrice[level];
            case Rarity.Unique: return _priceInfo.uniqueSkillPrice[level];
            case Rarity.Legendary: return _priceInfo.legendarySkillPrice[level];
            case Rarity.Mythic: return _priceInfo.mythicSkillPrice[level];
        }
        return int.MaxValue;
    }

    public int GetRequireWeaponCount(Rarity weaponRarity, int level)
    {
        switch (weaponRarity)
        {
            case Rarity.Common: return _priceInfo.commonWeaponPrice[level];
            case Rarity.Uncommon: return _priceInfo.uncommonWeaponPrice[level];
            case Rarity.Rare: return _priceInfo.rareWeaponPrice[level];
            case Rarity.Unique: return _priceInfo.uniqueWeaponPrice[level];
            case Rarity.Legendary: return _priceInfo.legendaryWeaponPrice[level];
            case Rarity.Mythic: return _priceInfo.mythicWeaponPrice[level];
        }
        return int.MaxValue;
    }

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
}
