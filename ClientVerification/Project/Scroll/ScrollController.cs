using ClientVerification.Etc;
using ClientVerification.Verification;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace ClientVerification.Scroll
{
    public class ScrollController
    {
        private readonly ILogger<ScrollController> _logger;

        public ScrollController(ILogger<ScrollController> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction("RegenerateScroll")]
        public async Task<object> RegenerateScroll(
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IVerificationSystem verificationSystem)
        {
            const int regenIntervalSec = 180; // 3분
            const int maxScroll = 100;
            const int initialScroll = 0;

            // 1. GameData 로드
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.ServiceToken,
                context.ProjectId,
                context.PlayerId,
                new() { "GameData" });

            GameData gameData = new GameData
            {
                level = 1,
                maxStageNum = 1,
                currentStageNum = 1,
                lastScrollTime = DateTime.UtcNow.ToString("O")
            };

            bool isFirstInit = false;

            if (res.Data.Results.Count > 0)
            {
                string json = res.Data.Results[0].Value.ToString();
                gameData = JsonConvert.DeserializeObject<GameData>(json);
            }
            else
            {
                isFirstInit = true;
            }

            // 2. 최초 생성
            if (isFirstInit)
            {
                gameData.scroll = initialScroll;
                gameData.lastScrollTime = DateTime.UtcNow.ToString("O");
                string initJson = JsonConvert.SerializeObject(gameData);

                await gameApiClient.CloudSaveData.SetItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId,
                    context.PlayerId,
                    new("GameData", initJson));

                return new { scroll = gameData.scroll, nextInSeconds = regenIntervalSec.ToString() };
            }

            // 3. lastScrollTime 파싱
            DateTime lastTime;
            if (string.IsNullOrEmpty(gameData.lastScrollTime) || gameData.lastScrollTime == "Max")
                lastTime = DateTime.UtcNow;
            else
                lastTime = DateTime.Parse(gameData.lastScrollTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

            var now = DateTime.UtcNow;
            double elapsedSec = (now - lastTime).TotalSeconds;
            int regenCount = (int)(elapsedSec / regenIntervalSec);

            // 4. 스크롤 증가
            if (regenCount > 0)
            {
                gameData.scroll = Math.Min(gameData.scroll + regenCount, maxScroll);
                lastTime = lastTime.AddSeconds(regenCount * regenIntervalSec);
            }

            // 5. 다음 충전까지 남은 시간 계산
            string remainStr;
            if (gameData.scroll >= maxScroll)
            {
                remainStr = "Max";
                gameData.lastScrollTime = "Max";
            }
            else
            {
                double remain = regenIntervalSec - (now - lastTime).TotalSeconds;
                if (remain < 0) remain = 0;
                remainStr = remain.ToString("F0");
                gameData.lastScrollTime = lastTime.ToString("O");
            }

            // 6. 저장
            string updatedJson = JsonConvert.SerializeObject(gameData);
            await gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.ServiceToken,
                context.ProjectId,
                context.PlayerId,
                new("GameData", updatedJson));

            // 7. 반환 (최신 scroll 포함)
            return new
            {
                scroll = gameData.scroll,
                nextInSeconds = remainStr
            };
        }
    }
}
