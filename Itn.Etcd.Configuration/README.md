# Itn.Etcd.Configuration

This package provides Microsoft.Extensions.Configuration's provider using [etcd](https://etcd.io).

# Prerequisits

* net8.0 or higher
* etcd service in runtime environment
    * [using binary](https://etcd.io/docs/v3.6/install/)
    * [using kubernetes](https://etcd.io/docs/v3.6/op-guide/kubernetes/)

# Basic Usage

1. create C# project
2. add nuget package "Itn.Etcd.Configuration" to project
3. add configuration provider by `Itn.Etcd.Configuration.EtcdConfigurationExtensions.AddEtcd(this IConfigurationBuilder builder, string rootKey, ...)`
    * see also [dotnet-etcd](https://github.com/shubhamranjan/dotnet-etcd) for options

# Diagnostics

## Activity

This package provides some Activity, source name is `Itn.Etcd.Configuration.EtcdConfigurationProvider`.

Activity names are following

* Load, LoadAsync
    * activity of loading keyvalue from etcd
* WatchDetected
    * change detected in etcd service
* RenewClient
    * re-connecting if connection is invalid
* CheckWatch
    * periodic check for connection
* CancelWatch
    * cancelling watch when shutdown

### OpenTelemetry

If you want to trace activity with opentelemetry,add instrumentation like following.

```csharp
// builder is WebApplicationBuilder or ApplicationBuilder
// you need to add OpenTelemetry.Extensions.Hosting for AddOpenTelemetry, 
// and OpenTelemetry.Exporter.OpenTelemetryProtocol for UseOtlpExporter
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .WithTracing(tracer =>
    {
        tracer.AddSource("Itn.Etcd.Configuration.EtcdConfigurationProvider");
    })
    ;

```

## DiagnosticSource

If you want to get logs, subscribe DiagnosticSource named `Itn.Etcd.Configuration.EtcdConfigurationProvider`.
`IDisposable SubscribeEtcdLog(this IServiceProvider provider)` will help you if you want to log all diagnostic events with `ILogger`.