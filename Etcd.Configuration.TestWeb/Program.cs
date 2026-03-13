using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("etcdtest");
ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
builder.Configuration.AddEtcd(urls: connectionString);
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .WithTracing(tracer =>
    {
        tracer.AddSource(Etcd.Configuration.Definitions.ProviderDiagnosticName);
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
