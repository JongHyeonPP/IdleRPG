using ClientVerification.Etc;
using System;
using System.Collections.Generic;

namespace ClientVerification.Verification
{
    public static class ServerReinforceCalculator
    {
        public static float GetReinforceValueGold(StatusType type, int level, Dictionary<StatusType, ReinforceRule> valueTable)
        {
            if (!valueTable.TryGetValue(type, out var rule))
                return 0;

            float total = rule.startValue;
            for (int i = 1; i <= level; i++)
            {
                float inc = rule.baseInc + (i / (float)rule.step) * rule.stepInc;
                total += inc;
            }
            return total;
        }
    }
}
