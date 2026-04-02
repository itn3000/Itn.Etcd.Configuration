using Itn.Etcd.Configuration;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using R3;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("etcdtest");
ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
var intervalSec = builder.Configuration.GetValue<int?>("Etcd:CheckStatusIntervalSec");
var keySeparator = builder.Configuration.GetSection("Etcd").GetValue<string>("KeySeparator");
if(string.IsNullOrEmpty(keySeparator))
{
    keySeparator = ":";
}
builder.Configuration.AddEtcd(urls: connectionString, loadThrottle: TimeSpan.Zero, checkStatusInterval: TimeSpan.FromSeconds(intervalSec.GetValueOrDefault(5)), keySeparator: keySeparator[0]);
//builder.Services.AddSingleton<EtcdConfigrationDiagnosticListener>();
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .WithTracing(tracer =>
    {
        tracer.AddSource(Itn.Etcd.Configuration.Definitions.ProviderDiagnosticName);
    })
    ;
var app = builder.Build();
//var listener = app.Services.GetRequiredService<EtcdConfigrationDiagnosticListener>();
//using var subscription = DiagnosticListener.AllListeners.Subscribe(listener);
using var subscription = app.Services.SubscribeEtcdLog();

app.MapGet("/", ([FromServices]IConfiguration cfg, [FromServices]ILogger<Program> logger) =>
{
    var str = cfg.GetValue<string>("MyOptions:X");
    logger.LogInformation("{Value}", str);
    return str ?? "";
});

app.Run();

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
    class EtcdConfigurationDiagnosticEventListener(ILogger logger) : IObserver<KeyValuePair<string, object?>>
    {
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
                        logger.LogInformation("{EventName}: {RootKey}, {WatchId}, {Version}, {CurrentTokensCount}, {ActivityId}",
                            value.Key,
                            watchStatus.RootKey,
                            watchStatus.WatchId,
                            watchStatus.Version,
                            watchStatus.CurrentTokensCount,
                            watchStatus.ActivityId)
                            ;
                        break;
                    }
                case (ExceptionEventArgs error):
                    {
                        logger.LogError(error.Exception, "{EventName}: {ActivityId}", value.Key, error.ActivityId);
                        break;
                    }
                case (CancelEventArgs cancel):
                    {
                        logger.LogInformation("{EventName}: {ActivityId}", value.Key, cancel.ActivityId);
                        break;
                    }
                case (ClientAlreadyUpdateEventArgs args):
                    {
                        logger.LogInformation("{EventName}: {ActivityId}", value.Key, args.ActivityId);
                        break;
                    }
                case (UpdateByAnotherEventArgs args):
                    {
                        logger.LogInformation("{EventName}: {ActivityId}", value.Key, args.ActivityId);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}