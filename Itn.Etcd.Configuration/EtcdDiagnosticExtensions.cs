using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Itn.Etcd.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EtcdDiagnosticExtensions
    {
        public static IDisposable SubscribeEtcdLog(this IServiceProvider serviceProvider)
        {
            return DiagnosticListener.AllListeners.Subscribe(
                new EtcdConfigrationDiagnosticListener(serviceProvider.GetRequiredService<ILogger<EtcdConfigrationDiagnosticListener>>()));
        }
    }
    class EtcdConfigrationDiagnosticListener(ILogger<EtcdConfigrationDiagnosticListener> logger) : IObserver<DiagnosticListener>
    {
        DisposableBag disposableBag = new DisposableBag();

        public void OnCompleted()
        {
            disposableBag.Dispose();
        }

        public void OnError(Exception error)
        {
            logger.LogError(error, "Error in DiagnosticListener");
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name.Equals(Itn.Etcd.Configuration.Definitions.ProviderDiagnosticName))
            {
                value.Subscribe(new EtcdConfigurationDiagnosticEventListener(logger)).AddTo(ref disposableBag);
            }
        }
    }
    partial class EtcdConfigurationDiagnosticEventListener(ILogger logger) : IObserver<KeyValuePair<string, object?>>
    {
        [LoggerMessage(EventId = 1, EventName = "Error", Level = LogLevel.Error, Message = "{Name}: {ActivityId}")]
        static private partial void Log_Error(ILogger logger, Exception error, string name, string activityId);

        [LoggerMessage(EventId = 2, EventName = "WatchStatus", Level = LogLevel.Information, Message = "{RootKey}, {WatchId}, {Version}, {CurrentTokensCount}, {ActivityId}")]
        static private partial void Log_WatchStatus(ILogger logger, Exception? exception,
            string rootKey, long? watchId, string version, int currentTokensCount, string activityId);
        [LoggerMessage(EventId = 3, EventName = "Cancel", Level = LogLevel.Information, Message = "{Name}: {ActivityId}")]
        static private partial void Log_Cancel(ILogger logger, Exception? exception,
            string name, string activityId);
        [LoggerMessage(EventId = 4, EventName = "ClientAlreadyUpdate", Level = LogLevel.Information, Message = "{ActivityId}")]
        static private partial void Log_ClientAlreadyUpdate(ILogger logger, Exception? exception,
            string activityId);
        [LoggerMessage(EventId = 5, EventName = "UpdateByAnother", Level = LogLevel.Information, Message = "{ActivityId}")]
        static private partial void Log_UpdateByAnother(ILogger logger, Exception? exception,
            string activityId);
        [LoggerMessage(EventId = 6, EventName = "RenewClient", Level = LogLevel.Information, Message = "client renew done")]
        static private partial void Log_RenewClient(ILogger logger, Exception? exception);
        [LoggerMessage(EventId = 7, EventName = "LoadDone", Level = LogLevel.Information, Message = "key load done({RootKey})")]
        static private partial void Log_LoadDone(ILogger logger, Exception? exception, string rootKey);
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            switch (value.Value)
            {
                case null:
                    {
                        break;
                    }
                case (WatchStatusEventArgs watchStatus):
                    {
                        Log_WatchStatus(logger, null, watchStatus.RootKey, watchStatus.WatchId, watchStatus.Version, watchStatus.CurrentTokensCount, watchStatus.ActivityId);
                        break;
                    }
                case (ExceptionEventArgs error):
                    {
                        Log_Error(logger, error.Exception, value.Key, error.ActivityId);
                        break;
                    }
                case (CancelEventArgs cancel):
                    {
                        Log_Cancel(logger, null, value.Key, cancel.ActivityId);
                        break;
                    }
                case (ClientAlreadyUpdateEventArgs args):
                    {
                        Log_ClientAlreadyUpdate(logger, null, args.ActivityId);
                        break;
                    }
                case (UpdateByAnotherEventArgs args):
                    {
                        Log_UpdateByAnother(logger, null, args.ActivityId);
                        break;
                    }
                case (ConnectionRenewArgs args):
                    {
                        Log_RenewClient(logger, null);
                        break;
                    }
                case (LoadDoneArgs args):
                    {
                        Log_LoadDone(logger, null, args.RootKey);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
