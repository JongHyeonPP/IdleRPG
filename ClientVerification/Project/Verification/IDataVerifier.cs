using System;

namespace ClientVerification.Verification
{
    public interface IDataVerifier
    {
        bool Verify(GameData clientData, out string failReason);
    }
}
