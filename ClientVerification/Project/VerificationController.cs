using ClientVerification;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace Verification;

public class VerificationController
{
    public static Dictionary<string, string> stageDropFormula { get; private set; }
    public static string levelUpRequireExp { get; private set; }
    private int _verifiCationInterval;
    private int _verifiCationAllowedAttempt;
    private ILogger<VerificationController> _logger;
    private ServerGameData gameData;

    public VerificationController(ILogger<VerificationController> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("VerificationReport")]
    public async Task<ReportResult> ClientVerificationToSave(
    string serializedGoldReport,
    string serializedReinforceReport,
    string playerId,
    IExecutionContext context,
    IGameApiClient gameApiClient,
    IRemoteConfigService RemoteConfigService)
    {
        ReportResult result = new()
        {
            isVerificationSuccess = false,
            failureFactor = "Unknown",
            invalidCount = 0
        };

        ApiResponse<GetItemsResponse> gameDataResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
            context, context.ServiceToken, context.ProjectId, playerId, new() { "GameData" });
        string serializedGameData = gameDataResponse.Data.Results[0].Value.ToString();
        gameData = JsonConvert.DeserializeObject<ServerGameData>(serializedGameData);

        Dictionary<string, string> verificationInfoConfig = RemoteConfigService.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "VERIFICATION_INFO");
        _verifiCationInterval = int.Parse(verificationInfoConfig["Interval"]);
        _verifiCationAllowedAttempt = int.Parse(verificationInfoConfig["AllowedAttempt"]);

        
        result.invalidCount = gameData.invalidCount;

        // early return: 이미 invalidCount가 3 이상이면 저장 없이 반환
        if (gameData.invalidCount >= 3)
        {
            _logger.LogError("Invalid user attempted to save data.");
            result.failureFactor = "ExceededInvalidCount";
            return result;
        }

        Dictionary<string, object> addFieldValues = new();
        List<VerificationReport> verificationReportList = JsonConvert.DeserializeObject<List<VerificationReport>>(serializedGoldReport);

        if (verificationReportList.Count > _verifiCationAllowedAttempt)
        {
            addFieldValues.Add("invalidCount", 1);
            _logger.LogError("Invalid Report Count : " + JsonConvert.SerializeObject(new
            {
                limitCount = _verifiCationAllowedAttempt,
                reportedCount = verificationReportList.Count,
                invalidCount = gameData.invalidCount + 1
            }));

            result.failureFactor = "ExceededVerificationAttempt";
            result.invalidCount = gameData.invalidCount + 1;
        }
        else
        {
            List<ReinforceReport> reinforceReportList = JsonConvert.DeserializeObject<List<ReinforceReport>>(serializedReinforceReport);

            stageDropFormula = RemoteConfigService.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "STAGE_DROP_FORMULA");
            levelUpRequireExp = RemoteConfigService.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");

            bool isResourcePassed = ResourceVerification(verificationReportList, RemoteConfigService, context, gameApiClient, playerId, gameData.currentStageNum, addFieldValues);
            bool isReinforcePassed = ReinforceVerification(reinforceReportList);

            if (isResourcePassed && isReinforcePassed)
            {
                result.isVerificationSuccess = true;
                result.failureFactor = "";
            }
            else
            {
                addFieldValues.Add("invalidCount", 1);
                result.failureFactor = "VerificationFailed";
                result.invalidCount = gameData.invalidCount + 1;
            }
        }

        // 공통 저장 지점
        if (addFieldValues.Count > 0)
        {
            string modifiedGameData = JsonModifier.AddToFieldValues(serializedGameData, addFieldValues, _logger);
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, playerId,
                new SetItemBody("GameData", modifiedGameData)
            );
        }

        ApiResponse<GetItemsResponse> lastVerificationResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
            context, context.ServiceToken, context.ProjectId, playerId, new() { "OfflineRewardInfo" });

        string offlineRewardJson = lastVerificationResponse.Data.Results.Count > 0
            ? lastVerificationResponse.Data.Results[0].Value.ToString()
            : "{}";

        Dictionary<string, string> offlineRewardDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(offlineRewardJson)
            ?? new Dictionary<string, string>();

        offlineRewardDict["lastChecked"] = DateTime.UtcNow.ToString("o");
        string modifiedRewardJson = JsonConvert.SerializeObject(offlineRewardDict);

        await gameApiClient.CloudSaveData.SetItemAsync(
            context, context.ServiceToken, context.ProjectId, playerId,
            new SetItemBody("OfflineRewardInfo", modifiedRewardJson));


        return result;
    }


    private bool ResourceVerification(List<VerificationReport> verificationReportList, IRemoteConfigService formulaService,
        IExecutionContext context, IGameApiClient gameApiClient, string playerId, int currentStageNum, Dictionary<string, object> result)
    {
        foreach (VerificationReport report in verificationReportList)
        {
            int caseValue = report.Source switch
            {
                "Battle" => BattleCase(report, formulaService, context, gameApiClient, playerId, currentStageNum),
                "Ad" => AdCase(report),
                _ => 0
            };

            if (caseValue <= 0)
                return false;

            string resource = report.Resource.ToLowerInvariant();
            if (result.ContainsKey(resource))
            {
                BigInteger currentValue = BigInteger.Parse(result[resource].ToString());
                result[resource] = currentValue + BigInteger.Parse(report.Value.ToString());
            }
            else
            {
                result[resource] = report.Value;
            }
        }

        return true;
    }

    private int AdCase(VerificationReport report)
    {
        // 광고 보상 검증 로직 필요 시 작성
        return 0;
    }

    private int BattleCase(VerificationReport report, IRemoteConfigService formulaService, IExecutionContext context, IGameApiClient gameApiClient, string playerId, int currentStageNum)
    {
        DataTable dataTable = new();

        string standardValueStr = stageDropFormula[$"{report.Resource}Standard"].Replace("{stageNum}", currentStageNum.ToString());
        float valueRange = float.Parse(stageDropFormula[$"{report.Resource}Range"]);
        int standardValue = Convert.ToInt32(dataTable.Compute(standardValueStr, ""));
        int max = (int)Math.Ceiling(standardValue * (1 + valueRange)) + 1;

        if (report.Value >= max)
        {
            Dictionary<string, object> anomaly = new()
            {
                { "type", $"{report.Resource}Gain" },
                { "expectedMax", max - 1 },
                { $"reported{report.Resource}", report.Value },
                { "invalidCount", gameData.invalidCount + 1 }
            };
            _logger.LogError($"Unexpected {report.Resource}: " + JsonConvert.SerializeObject(anomaly));
            return -1;
        }

        Dictionary<string, object> appropriate = new()
        {
            { "type", $"{report.Resource}Gain" },
            { "expectedMax", max - 1 },
            { $"reported{report.Resource}", report.Value }
        };
        _logger.LogDebug($"Appropriate {report.Resource}: " + JsonConvert.SerializeObject(appropriate));

        return report.Value;
    }

    private bool ReinforceVerification(List<ReinforceReport> reinforceReportList)
    {
        // 강화 검증 로직 필요 시 구현
        return true;
    }

    [CloudCodeFunction("StageClearVerification")]
    public bool StageClearVerification()
    {
        return true;
    }
}
