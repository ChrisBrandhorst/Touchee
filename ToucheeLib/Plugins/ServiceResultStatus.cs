using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Service {

    public enum ServiceResultStatus {
        Success,
        NoResult,
        InvalidResponse,
        ClientError,
        ServiceOffline,
        TemporaryError,
        AuthenticationFailed,
        Throttled,
        UnknownError
    }

}
