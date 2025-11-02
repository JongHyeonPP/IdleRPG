using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;

namespace ClientVerification.Verification
{
    public partial class ResourceVerifier : IDataVerifier
    {
        private readonly List<ResourceReport> reports;
        private readonly GameData serverData;
        private readonly ILogger logger;
        private readonly DataTable table = new();

        private readonly Dictionary<string, object> goldFormula;
        private readonly Dictionary<string, object> expFormula;
        private readonly Dictionary<string, object> fragmentFormula;
        private readonly Dictionary<string, object> weaponFormula;
        private readonly Dictionary<string, object> adventureReward;
        private readonly Dictionary<string, object> dungeonReward;
        private readonly Dictionary<string, object> companionReward;
        private readonly Dictionary<string, Dictionary<string, int>> attendanceReward;
        private readonly string levelExpFormula;

        private readonly int maxLevel = 9999;

        private readonly IVerificationSystem verificationSystem;
        private readonly Unity.Services.CloudCode.Core.IExecutionContext context;
        private readonly IGameApiClient gameApiClient;

        public ResourceVerifier(
            List<ResourceReport> reports,
            GameData serverData,
            ILogger logger,
            IVerificationSystem verificationSystem,
            Unity.Services.CloudCode.Core.IExecutionContext context,
            IGameApiClient gameApiClient)
        {
            this.reports = reports;
            this.serverData = serverData;
            this.logger = logger;
            this.verificationSystem = verificationSystem;
            this.context = context;
            this.gameApiClient = gameApiClient;

            goldFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GOLD_DROP_FORMULA");
            expFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "EXP_DROP_FORMULA");
            fragmentFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "FRAGMENT_DROP_FORMULA");
            weaponFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "WEAPON_DROP_FORMULA");
            adventureReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ADVENTURE_REWARD");
            dungeonReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "DUNGEON_REWARD");
            companionReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "COMPANION_REWARD");
            attendanceReward = verificationSystem.GetRemoteConfig<Dictionary<string, Dictionary<string, int>>>(context, gameApiClient, "ATTENDANCE_INFO");
            levelExpFormula = verificationSystem.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");
        }

        public bool Verify(out string reason)
        {
            reason = "";

            foreach (var r in reports)
            {
                if (!VerifySingle(r, out reason))
                    return false;
            }

            NormalizeLevelAndExp();
            return true;
        }

        private bool VerifySingle(ResourceReport r, out string reason)
        {
            switch (r.Source)
            {
                case Source.Battle: return BattleCase(r, out reason);
                case Source.Adventure: return AdventureCase(r, out reason);
                case Source.Dungeon: return DungeonCase(r, out reason);
                case Source.Companion: return CompanionCase(r, out reason);
                case Source.Attendance: return AttendanceCase(r, out reason);
                case Source.Advertise: return SimpleApply(r, out reason);
                default:
                    reason = $"Unknown source: {r.Source}";
                    return false;
            }
        }
    }
}
