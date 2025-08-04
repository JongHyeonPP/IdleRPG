using System;

namespace ClientVerification.Verification
{
    public interface IDataVerifier
    {
        bool Verify(out string failReason);
    }
}
