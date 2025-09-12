using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace ClientVerification.Verification
{
    public class VerificaitonController
    {
        private readonly ILogger<VerificaitonController> _logger;
        private GameData _serverData;
        private Dictionary<string, string> _offlineRewardDict;
        

        
        public VerificaitonController(ILogger<VerificaitonController> logger)
        {
            _logger = logger;
        }
        [CloudCodeFunction("VerificationReport")]
        public async Task<ReportResult> VerifyClientData(string serializedResourceReport,string serializedSpendReport, string serializedGameData, bool isAcquireOfflineReward, IExecutionContext context, IGameApiClient gameApiClient, IVerificationSystem verificationSystem)
        {
            ReportResult result = new() { isVerificationSuccess = false, failureFactor = "Unknown" };

            _serverData = await LoadGameData(context, gameApiClient);
            var clientData = JsonConvert.DeserializeObject<GameData>(serializedGameData);

            ApplyUsualFieldsOnly(_serverData, clientData);
            _offlineRewardDict = await LoadOfflineRewardData(context, gameApiClient);
            result.invalidCount = _serverData.invalidCount;

            if (_serverData.invalidCount >= 3)
                return Fail(result, "ExceededInvalidCount");

            var info = verificationSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "VERIFICATION_INFO");
            int allowedAttempt = int.Parse(info["AllowedAttempt"]);

            List<ResourceReport> resourceReports = JsonConvert.DeserializeObject<List<ResourceReport>>(serializedResourceReport);
            Dictionary<string, int> spendReports = JsonConvert.DeserializeObject<Dictionary<string, int>>(serializedSpendReport);
            if (resourceReports.Count > allowedAttempt)
                return Fail(result, $"ExceededVerificationAttempt...{{reports.Count}}");

            var verifiers = new List<IDataVerifier>
            {
                new StatPointVerifier(clientData),
                new ResourceVerifier(resourceReports, _serverData, _logger, verificationSystem, context, gameApiClient),
                new SpendVerifier(spendReports, _serverData, _logger, verificationSystem, context, gameApiClient)
            };


            foreach (var verifier in verifiers)
            {
                if (!verifier.Verify(out string reason))
                    return Fail(result, reason);
            }

            

            if (isAcquireOfflineReward)
            {
                ApplyOfflineReward(context, gameApiClient, verificationSystem);
                _offlineRewardDict["AccumulatedTime"] = "0";
            }

            await SaveData(context, gameApiClient);
            result.isVerificationSuccess = true;
            result.failureFactor = "";
            return result;
        }

        private ReportResult Fail(ReportResult result, string reason)
        {
            result.failureFactor = reason;
            result.invalidCount = ++_serverData.invalidCount;
            _logger.LogError(reason);
            return result;
        }

        private async Task<GameData> LoadGameData(IExecutionContext context, IGameApiClient gameApiClient)
        {
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            if (res.Data.Results.Count == 0)
                return new GameData { level = 1, maxStageNum = 1, currentStageNum = 1 };

            return JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString());
        }

        private async Task<Dictionary<string, string>> LoadOfflineRewardData(IExecutionContext context, IGameApiClient gameApiClient)
        {
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "OfflineRewardInfo" });

            return res.Data.Results.Count > 0
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(res.Data.Results[0].Value.ToString())
                : new();
        }

        private async Task SaveData(IExecutionContext context, IGameApiClient gameApiClient)
        {
            //_logger.LogDebug(JsonConvert.SerializeObject(_serverData));
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(_serverData)));

            _offlineRewardDict["lastChecked"] = DateTime.UtcNow.ToString("o");
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("OfflineRewardInfo", JsonConvert.SerializeObject(_offlineRewardDict)));
        }

        private void ApplyUsualFieldsOnly(GameData server, GameData client)
        {
            server.level = client.level;
            server.statLevel_StatPoint = new(client.statLevel_StatPoint);
            server.stat_Promote = new(client.stat_Promote);
            server.skillLevel = new(client.skillLevel);
            server.skillFragment = new(client.skillFragment);
            server.equipedSkillArr = (string[])client.equipedSkillArr.Clone();
            server.weaponCount = new(client.weaponCount);
            server.weaponLevel = new(client.weaponLevel);
            server.playerWeaponId = client.playerWeaponId;
            server.companionWeaponIdArr = (string[])client.companionWeaponIdArr.Clone();
            server.currentStageNum = client.currentStageNum;
            server.maxStageNum = client.maxStageNum;
            server.playerPromoteEffect = new(client.playerPromoteEffect);
            server.companionPromoteEffect = new Dictionary<int, (StatusType, Rarity)>[3];
            for (int i = 0; i < 3; i++)
                server.companionPromoteEffect[i] = new(client.companionPromoteEffect[i]);
            server.companionPromoteTech = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                server.companionPromoteTech[i] = new int[2];
                Array.Copy(client.companionPromoteTech[i], server.companionPromoteTech[i], 2);
            }
            server.currentCompanionPromoteTech = (ValueTuple<int, int>[])client.currentCompanionPromoteTech.Clone();
            server.equipedCostumes = new(client.equipedCostumes);
            server.ownedCostumes = new(client.ownedCostumes);
            server.playerRankIndex = client.playerRankIndex;
            
        }

        private void ApplyOfflineReward(IExecutionContext context, IGameApiClient gameApiClient, IVerificationSystem verificationSystem)
        {
            var config = verificationSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "OFFLINE_REWARD_INFO");
            double sec = double.Parse(_offlineRewardDict["AccumulatedTime"]);
            var dt = new DataTable();

            int Calc(string key) => Convert.ToInt32(dt.Compute(config[key]
                .Replace("{second}", sec.ToString())
                .Replace("{maxStageNum}", _serverData.maxStageNum.ToString()), null)) / 60;

            _serverData.gold += Calc("Gold");
            _serverData.exp += Calc("Exp");
            _serverData.dia += Calc("Dia");
            _serverData.clover += Calc("Clover");
        }
    }
}
