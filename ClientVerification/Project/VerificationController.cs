using ClientVerification;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudSave.Model;

namespace Verification;
public class VerificationController
{
    public static Dictionary<string, string> stageDropFormula { get; private set; }
    public static string levelUpRequireExp { get; private set; }
    private ILogger<VerificationController> _logger;
    public VerificationController(ILogger<VerificationController> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("VerificationReport")]
    public async void ClientVerificationToSave(
            string serializedGoldReport,
            string serializedReinforceReport,
            string serializedGameData,
            string playerId,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IRemoteConfigService formulaService
        )
    {
        //// 공식 데이터를 문자열(JSON)로 직렬화해서 리턴
        // 다시 JSON 문자열로 직렬화해서 리턴
        List<VerificationReport> goldReportList = JsonConvert.DeserializeObject<List<VerificationReport>>(serializedGoldReport);
        List<ReinforceReport> reinforceReportList = JsonConvert.DeserializeObject<List<ReinforceReport>>(serializedReinforceReport);
        ServerGameData gameData = JsonConvert.DeserializeObject<ServerGameData>(serializedGameData);
        stageDropFormula = formulaService.GetFormulas(context, gameApiClient, FormulaType.STAGE_DROP_FORMULA);
        levelUpRequireExp = formulaService.GetFormulas(context, gameApiClient, FormulaType.LEVEL_UP_REQUIRE_EXP)["requireExp"];

        bool isResourcePassed = ResourceVerification(goldReportList, formulaService, context, gameApiClient, playerId, gameData.currentStageNum, out Dictionary<string, object> result);
        bool isReinforcePassed = ReinforceVerification(reinforceReportList);
        if (isResourcePassed && isReinforcePassed)
        {
            string modifiedGameData = JsonModifier.AddToFieldValues(serializedGameData, result, _logger);
            //_logger.LogDebug(modifiedGameData);
            gameApiClient.CloudSaveData
                .SetItemAsync(context, context.ServiceToken, context.ProjectId, playerId,
                    new SetItemBody("GameData", serializedGameData));
        }
    }
    private bool ResourceVerification(List<VerificationReport> verificationReportList, IRemoteConfigService formulaService,
        IExecutionContext context, IGameApiClient gameApiClient,string playerId, int currentStageNum, out Dictionary<string, object> result)
    {
        result = new();
        foreach (VerificationReport report in verificationReportList)
        {
            int caseValue = 0;
            switch (report.Source)
            {
                case "Battle":
                    caseValue = BattleCase(report, formulaService, context, gameApiClient, playerId, currentStageNum);
                    break;
                case "Ad":
                    caseValue = AdCase(report);
                    break;
            }
            if (caseValue > 0)
            {
                string resource = report.Resource.ToLower();
                if (result.ContainsKey(resource))
                {
                    switch (resource)
                    {
                        case "gold":
                        case "exp":
                            BigInteger parsedValue = BigInteger.Parse(result[report.Resource].ToString());
                            result[report.Resource] = parsedValue + BigInteger.Parse(report.Value.ToString());
                            break;
                    }
                    result[report.Resource] = report.Value;
                    
                
                }
                else
                    result[report.Resource] = report.Value;
            }
            else
            {
                return false;
            }


        }

        return true;

        int AdCase(VerificationReport report)
        {
            Dictionary<string, string> valueFormulas = formulaService.GetFormulas(context, gameApiClient, FormulaType.REINFORCE_VALUE_STATUS);
            return 0;
        }
        
    }
    int BattleCase(VerificationReport report, IRemoteConfigService formulaService, IExecutionContext context, IGameApiClient gameApiClient, string playerId, int currentStageNum)
    {
        DataTable dataTable = new();
        string standardValueStr = stageDropFormula[$"{report.Resource}Standard"].Replace("{stageNum}", currentStageNum.ToString());
        float valueRange = float.Parse(stageDropFormula[$"{report.Resource}Range"]);
        int standardValue = Convert.ToInt32(dataTable.Compute(standardValueStr, null));
        float overRatio = ((float)report.Value - standardValue) / standardValue;
        if (overRatio > valueRange)
        {
            Dictionary<string, object> anomaly = new()
            {
                { "type", $"{report.Resource}Gain" },
                { "percentOver", $"{Math.Round(overRatio * 100f, 1)}%" },
                { $"expected{report.Resource}", standardValue },
                { $"reported{report.Resource}", report.Value }
            };
            _logger.LogError($"Unexpceted {report.Resource} : " + JsonConvert.SerializeObject(anomaly));
            return -1;
        }
        return report.Value;
    }
    private bool ReinforceVerification(List<ReinforceReport> reinforceReportList)
    {


        return true;
    }

    [CloudCodeFunction("StageClearVerification")]
    public bool StageClearVerification() 
    {
        //Dictionary<string, string> stageExpectedDPS = formulaService.GetFormulas(context, gameApiClient, FormulaType.STAGE_EXPECTED_DPS);
        return true;
    }
}

