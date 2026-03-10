using System;
using System.Collections.Generic;
using System.Text;

namespace Etcd.Configuration
{
    public static class Definitions
    {
        public const string ProviderDiagnosticName = $"Etcd.Configuration.{nameof(EtcdConfigurationProvider)}";
        public static class EventNames
        {
            public const string LoadError = nameof(LoadError);
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
