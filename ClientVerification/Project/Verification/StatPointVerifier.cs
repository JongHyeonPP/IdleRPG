using System.Linq;

namespace ClientVerification.Verification
{
    public class StatPointVerifier : IDataVerifier
    {
        public bool Verify(GameData clientData, out string failReason)
        {
            int used = clientData.statLevel_StatPoint.Values.Sum();
            if (clientData.statPoint + used != clientData.level-1)
            {
                failReason = $"StatPointMismatch...have : {clientData.statPoint + used}, expected : {clientData.level - 1}";
                return false;
            }

            failReason = "";
            return true;
        }
    }
}
