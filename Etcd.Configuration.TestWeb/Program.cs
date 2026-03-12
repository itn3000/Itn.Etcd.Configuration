using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEtcd(urls: builder.Configuration.GetConnectionString("etcdtest"));
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
