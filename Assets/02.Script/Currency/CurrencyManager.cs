using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private GameData _gameData;
    private Dictionary<Resource, string> _stageDropFormular;
    private string _levelUpRequireExp;

    private readonly DataTable _dataTable = new();
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        BattleBroker.GetNeedExp = GetNeedExp;
        //Drop
        BattleBroker.OnExpByDrop += GetExpByDrop;
        BattleBroker.OnGoldByDrop += GetGoldByDrop;
        BattleBroker.GetDropValue = GetDropValue;

        string stageDropFormularStr =  RemoteConfigService.Instance.appConfig.GetJson("STAGE_DROP_FORMULA", "None");
        _stageDropFormular = UtilityManager.GetParsedFormularDict<Resource>(stageDropFormularStr);
        string rawJson = RemoteConfigService.Instance.appConfig.GetJson("LEVEL_UP_REQUIRE_EXP", "None");
        Dictionary<string, string> dict = UtilityManager.GetParsedFormularDict<string>(rawJson);
        _levelUpRequireExp = dict["requireExp"];

    }
    public void GetGoldByDrop(int value)
    {
        _gameData.gold += value;
        BattleBroker.OnGoldSet();
    }
    public void GetExpByDrop(int value)
    {
        _gameData.exp += value;

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
        BattleBroker.OnLevelExpSet();
    }
    private BigInteger GetNeedExp()
    {
        object resultObj = _dataTable.Compute(_levelUpRequireExp.Replace("{level}", _gameData.level.ToString()), null);
        return Convert.ToInt32(resultObj);
    }
    private int GetDropValue(DropType dropType)
    {
        int baseValue = 0;
        var battleType = BattleBroker.GetBattleType();
        var _currentStageInfo = StageInfoManager.instance.GetNormalStageInfo(_gameData.currentStageNum);
        if (battleType == BattleType.Default)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    baseValue = _currentStageInfo.enemyStatusFromStage.gold;
                    break;
                case DropType.Exp:
                    baseValue = _currentStageInfo.enemyStatusFromStage.exp;
                    break;
            }
        }
        else if (battleType == BattleType.Boss)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    baseValue = _currentStageInfo.bossStatusFromStage.gold;
                    break;
                case DropType.Exp:
                    baseValue = _currentStageInfo.bossStatusFromStage.exp;
                    break;
            }
        }

        // ±10% 범위의 랜덤 int 생성
        int min = Mathf.FloorToInt(baseValue * 0.9f);
        int max = Mathf.CeilToInt(baseValue * 1.1f) + 1; // Random.Range의 max는 exclusive
        int result = UnityEngine.Random.Range(min, max);
        return result;
    }
}
