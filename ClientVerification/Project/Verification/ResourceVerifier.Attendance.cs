using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ClientVerification.Verification
{
    public partial class ResourceVerifier
    {
        private bool AttendanceCase(ResourceReport r, out string reason)
        {
            reason = "";

            if (attendanceReward == null)
            {
                reason = "ATTENDANCE_INFO missing in Remote Config";
                return false;
            }

            // 한국 표준시(UTC+9) 계산 (TimeZoneInfo 미사용)
            DateTime utcNow = DateTime.UtcNow;
            DateTime nowKorea = utcNow.AddHours(9);

            // 이미 오늘 출석했는지 확인
            if (DateTime.TryParse(serverData.lastAttendanceTime, out DateTime lastTime))
            {
                DateTime lastKorea = lastTime.ToUniversalTime().AddHours(9);

                if (lastKorea.Date == nowKorea.Date)
                {
                    reason = "Already attended today";
                    logger.LogInformation("[Attendance] Player already checked in today (KST)");
                    return false;
                }
            }

            int nextDay = serverData.lastAttendanceNum + 1;
            int totalDays = attendanceReward.Count;

            // 모든 출석 보상을 다 받았다면 1일차로 되돌림
            if (nextDay > totalDays)
            {
                nextDay = 1;
                logger.LogInformation("[Attendance] Cycle reset → Returning to Day 1");
            }

            // 보상 데이터 확인
            if (!attendanceReward.TryGetValue(nextDay.ToString(), out var rewardDict))
            {
                reason = $"No reward found for day {nextDay}";
                logger.LogWarning($"[Attendance] Reward not found for day {nextDay}");
                return false;
            }

            // 보상 지급
            foreach (var reward in rewardDict)
            {
                switch (reward.Key.ToLower())
                {
                    case "dia":
                        serverData.dia += reward.Value;
                        break;
                    case "gold":
                        serverData.gold += reward.Value;
                        break;
                    case "clover":
                        serverData.clover += reward.Value;
                        break;
                    case "scroll":
                        serverData.scroll += reward.Value;
                        break;
                    default:
                        logger.LogWarning($"[Attendance] Unknown reward type: {reward.Key}");
                        break;
                }
            }

            // 저장은 UTC 기준으로 일관성 있게 유지
            serverData.lastAttendanceNum = nextDay;
            serverData.lastAttendanceTime = utcNow.ToString("O");

            logger.LogInformation($"[Attendance] Success → Day {nextDay}, rewards granted (KST: {nowKorea:yyyy-MM-dd HH:mm:ss})");
            return true;
        }
    }
}
