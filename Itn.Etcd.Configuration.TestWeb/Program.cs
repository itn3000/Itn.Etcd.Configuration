using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("etcdtest");
ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
var intervalSec = builder.Configuration.GetValue<int?>("Etcd:CheckStatusIntervalSec");
builder.Configuration.AddEtcd(urls: connectionString, checkStatusIntervalSec: intervalSec.GetValueOrDefault(5));
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .WithTracing(tracer =>
    {
        tracer.AddSource(Itn.Etcd.Configuration.Definitions.ProviderDiagnosticName);
    })
    ;
var app = builder.Build();

app.MapGet("/", ([FromServices]IConfiguration cfg, [FromServices]ILogger<Program> logger) =>
{
    var str = cfg.GetValue<string>("MyOptions:X");
    logger.LogInformation("{Value}", str);
    return str ?? "";
});

app.Run();
