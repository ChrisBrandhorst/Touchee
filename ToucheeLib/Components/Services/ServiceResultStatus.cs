namespace Touchee.Components.Services {

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
