using System;
using System.Collections.Generic;
using System.Text;

namespace Itn.Etcd.Configuration
{
    public record class ExceptionEventArgs(Exception Exception, string ActivityId);
    public record class WatchStatusEventArgs(string Version,
        long DbSize,
        long? WatchId,
        string RootKey,
        int CurrentTokensCount,
        string ActivityId);
    public record class ClientAlreadyUpdateEventArgs(string ActivityId);
    public record class UpdateByAnotherEventArgs(string ActivityId);
    public record class CancelEventArgs(string ActivityId);
    public record class ConnectionRenewArgs();
    public record class LoadDoneArgs(string RootKey, char KeySeparator, int KeyCount);
    public static class Definitions
    {
        public const string ProviderDiagnosticName = $"Itn.Etcd.Configuration.{nameof(EtcdConfigurationProvider)}";
        public static class EventNames
        {
            public const string LoadError = nameof(LoadError);
            public const string LoadAsyncError = nameof(LoadAsyncError);
            public const string WatchError = nameof(WatchError);
            public const string OperationCancel = nameof(OperationCancel);
            public const string WaitTaskError = nameof(WaitTaskError);
            public const string DisposeClientError = nameof(DisposeClientError);
            public const string UpdateByAnother = nameof(UpdateByAnother);
            public const string Cancel = nameof(Cancel);
            public const string RenewClientError = nameof(RenewClientError);
            public const string Status = nameof(Status);
            public const string ClientAlreadyUpdate = nameof(ClientAlreadyUpdate);
        }
        public static class ActivityNames
        {
            public const string Dispose = nameof(Dispose);
            public const string LoadAsync = nameof(LoadAsync);
            public const string Load = nameof(Load);
            public const string RenewClient = nameof(RenewClient);
            public const string WatchDetected = nameof(WatchDetected);
            public const string CheckWatch = nameof(CheckWatch);
            public const string CancelWatch = nameof(CancelWatch);
        }

    }


}
