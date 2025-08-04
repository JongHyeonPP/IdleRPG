using EnumCollection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Unity.Services.RemoteConfig;
using UnityEngine;


public class CurrencyManager : MonoBehaviour
{
    private GameData _gameData;
    private string _levelUpRequireExp;
    private string goldFormula;
    public string expFormula;

    private int currentGoldValue;
    private int currentExpValue;

    private float goldRange;
    private float expRange;

    private readonly DataTable _dataTable = new();
    public static CurrencyManager instance;
    private void Awake()
    {
        instance = this;
        _gameData = StartBroker.GetGameData();
        string formulaJson = RemoteConfigService.Instance.appConfig.GetJson("STAGE_DROP_FORMULA", "None");
        Dictionary<string, string> dict = UtilityManager.GetParsedFormularDict<string>(formulaJson);
        goldFormula = dict["GoldStandard"];
        expFormula = dict["ExpStandard"];
        goldRange = float.Parse(dict["GoldRange"]);
        expRange = float.Parse(dict["ExpRange"]);
        BattleBroker.OnStageChange += OnStageChange;
    }
    private void Start()
    {
        BattleBroker.GetNeedExp = GetNeedExp;
        //Drop
        BattleBroker.OnExpByDrop += GetExpByDrop;
        BattleBroker.OnLevelExpSet += OnLevelExpSet;
        BattleBroker.OnGoldByDrop += GetGoldByDrop;
        BattleBroker.GetDropValue = GetDropValue;

        string stageDropFormularStr =  RemoteConfigService.Instance.appConfig.GetJson("STAGE_DROP_FORMULA", "None");
        Dictionary<Resource, string> _stageDropFormular = UtilityManager.GetParsedFormularDict<Resource>(stageDropFormularStr);
        _levelUpRequireExp = RemoteConfigService.Instance.appConfig.GetString("LEVEL_UP_REQUIRE_EXP");
    }

    private void GetExpByDrop(int value)
    {
        _gameData.exp += value;
        BattleBroker.OnLevelExpSet();
        NetworkBroker.QueueResourceReport(value, Resource.Exp, Source.Battle);
    }
    public void OnLevelExpSet()
    {
        while (true)
        {
            BigInteger needExp = BattleBroker.GetNeedExp();
            if (_gameData.exp < needExp)
                break;
            if (_gameData.exp >= needExp)
            {
                _gameData.exp -= needExp;
                _gameData.level++;
                _gameData.statPoint++;
                BattleBroker.OnStatPointSet();
            }
        }
        
    }
    public void GetGoldByDrop(int value)
    {
        _gameData.gold += value;
        BattleBroker.OnGoldSet();
        NetworkBroker.QueueResourceReport(value, Resource.Gold, Source.Battle);
    }


    private BigInteger GetNeedExp()
    {
        object resultObj = _dataTable.Compute(_levelUpRequireExp.Replace("{level}", _gameData.level.ToString()), null);
        return Convert.ToInt32(resultObj);
    }
    private int GetDropValue(DropType dropType)
    {
        int baseValue = 0;
        float range = 0;
        var battleType = BattleBroker.GetBattleType();
        if (battleType == BattleType.Default)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    baseValue = currentGoldValue;
                    range = goldRange;
                    break;
                case DropType.Exp:
                    baseValue = currentExpValue;
                    range = expRange;
                    break;
            }
        }
        // ±10% 범위의 랜덤 int 생성
        int min = Mathf.Max(1, Mathf.FloorToInt(baseValue * (1 - range)));
        int max = Mathf.CeilToInt(baseValue * 1+range) + 1; // Random.Range의 max는 exclusive
        int result = UnityEngine.Random.Range(min, max);
        return result;
    }
    private void OnStageChange()
    {
        string stageGoldFormula = goldFormula;
        string stageExpFormula = expFormula;
        currentGoldValue = Convert.ToInt32(_dataTable.Compute(stageGoldFormula.Replace("{stageNum}", _gameData.currentStageNum.ToString()), ""));
        currentExpValue = Convert.ToInt32(_dataTable.Compute(stageExpFormula.Replace("{stageNum}", _gameData.currentStageNum.ToString()), ""));
    }
}