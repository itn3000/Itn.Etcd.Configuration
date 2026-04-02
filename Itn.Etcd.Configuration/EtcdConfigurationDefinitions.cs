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
        public const int DefaultCheckStatusIntervalSec = 30;
        public const int DefaultLoadThrottleSec = 1;
        public static class EventNames
        {
            /// <summary>
            /// fired when load keys from etcd on first read
            /// </summary>
            /// <remarks><seealso cref="ExceptionEventArgs"/></remarks>
            public const string LoadError = nameof(LoadError);
            /// <summary>
            /// fired when load keys
            /// </summary>
            /// <remarks><seealso cref="ExceptionEventArgs"/></remarks>
            public const string LoadAsyncError = nameof(LoadAsyncError);
            /// <summary>
            /// fired when watching etcd key
            /// </summary>
            /// <remarks><seealso cref="ExceptionEventArgs"/></remarks>
            public const string WatchError = nameof(WatchError);
            /// <summary>
            /// fired when operation cancelled
            /// </summary>
            /// <seealso cref="CancelEventArgs"/>
            public const string OperationCancel = nameof(OperationCancel);
            /// <summary>
            /// fired when error in waiting shutting down
            /// </summary>
            /// <remarks><seealso cref="ExceptionEventArgs"/></remarks>
            public const string WaitTaskError = nameof(WaitTaskError);
            /// <summary>
            /// fired when error in disposing client on shutting down
            /// </summary>
            /// <remarks><seealso cref="ExceptionEventArgs"/></remarks>
            public const string DisposeClientError = nameof(DisposeClientError);
            /// <summary>
            /// fired when detected updating keys by another thread
            /// </summary>
            /// <remarks><seealso cref="UpdateByAnotherEventArgs"/></remarks>
            public const string UpdateByAnother = nameof(UpdateByAnother);
            /// <summary>
            /// fired when cancelling any operation
            /// </summary>
            public const string Cancel = nameof(Cancel);
            /// <summary>
            /// fired when error in renewing etcd client
            /// </summary>
            /// <seealso cref="ExceptionEventArgs"/>
            public const string RenewClientError = nameof(RenewClientError);
            /// <summary>
            /// fired when successing etcd service status(occured every EtcdConfigurationOptions.CheckStatusIntervalSec
            /// </summary>
            /// <seealso cref="WatchStatusEventArgs"/>
            public const string Status = nameof(Status);
            /// <summary>
            /// fired when etcd client already updated by another thread
            /// </summary>
            /// <remarks><seealso cref="ClientAlreadyUpdateEventArgs"/></remarks>
            public const string ClientAlreadyUpdate = nameof(ClientAlreadyUpdate);
        }
        public static class ActivityNames
        {
            public const string Dispose = nameof(Dispose);
            /// <summary>
            /// activity of updating keys
            /// </summary>
            public const string LoadAsync = nameof(LoadAsync);
            /// <summary>
            /// activity of updating keys in initialize
            /// </summary>
            public const string Load = nameof(Load);
            /// <summary>
            /// activity of renewing client if connection invalid
            /// </summary>
            public const string RenewClient = nameof(RenewClient);
            /// <summary>
            /// activity of callback for detecting etcd changed 
            /// </summary>
            public const string WatchDetected = nameof(WatchDetected);
            /// <summary>
            /// activity of getting etcd service status
            /// </summary>
            public const string CheckWatch = nameof(CheckWatch);
            /// <summary>
            /// activity of cancelling watching(on shutting down)
            /// </summary>
            public const string CancelWatch = nameof(CancelWatch);
        }

    }


}
