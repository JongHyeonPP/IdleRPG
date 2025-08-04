using System.Linq;

namespace ClientVerification.Verification
{
    public class StatPointVerifier : IDataVerifier
    {
        private GameData _clientData;
        public StatPointVerifier(GameData clientData)
        {
            _clientData = clientData;
        }
        public bool Verify(out string failReason)
        {
            int used = _clientData.statLevel_StatPoint.Values.Sum();
            if (used > _clientData.level-1)
            {
                failReason = $"StatPointMismatch...used : { used}, level : {_clientData.level}";
                return false;
            }

            failReason = "";
            return true;
        }
    }
}
