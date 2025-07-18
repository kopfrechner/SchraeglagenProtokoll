using JasperFx;
using JasperFx.Events.Daemon;
using Marten;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using SchraeglagenProtokoll.Api.Infrastructure;
using SchraeglagenProtokoll.Api.Infrastructure.Marten;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue("EnableCli", defaultValue: false))
{
    builder.Host.ApplyJasperFxExtensions();
}

// Marten
builder
    .Services.AddMarten(options =>
        options
            .SetupDatabase(
                builder.Configuration.GetConnectionString("Marten")
                    ?? throw new InvalidOperationException("Missing ConnectionString"),
                builder.Environment.IsDevelopment()
            )
            .SetupJsonSerialization()
            .SetupProjections()
            .SetupArchiving()
            .SetupMaskingPolicies()
            .SetupOpenTelemetry()
    )
    // Another performance optimization if you're starting from scratch
    .UseLightweightSessions()
    // Enable projection daemon
    .AddAsyncDaemon(DaemonMode.Solo)
    // Initialize sample data
    .ShouldInitializeSampleData(
        builder.Configuration.GetValue("ResetSampleData", defaultValue: false)
    )
    // Our handlers
    .IntegrateWithWolverine()
    // Notice the allow list filtering of event types and the possibility of overriding
    // the starting point for this subscription at runtime
    .ProcessEventsWithWolverineHandlersInStrictOrder(
        "Orders",
        o =>
        {
            // It's more important to create an allow list of event types that can be processed
            o.IncludeType<RideFinished>();

            o.Options.SubscribeFromPresent();
        }
    );
;

//builder.Host.UseWolverine();
builder.Services.AddWolverine(ExtensionDiscovery.Automatic).AddWolverineHttp();

// Add services to the container
builder.Services.AddEmail(builder.Configuration);

// Add OpenTelemetry support
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.ParseStateValues = true;
    logging.AddOtlpExporter();
});

builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(config => config.AddService("SchraeglagenProtokoll.Api"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddSource("Marten");
        tracing.AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Marten");
        metrics.AddOtlpExporter();
    });

// OpenApi
builder.Services.AddOpenApi().AddEndpointsApiExplorer();

// ======================================
// Build app
var app = builder.Build();

// Clean all Marten data
if (builder.Configuration.GetValue("ResetSampleData", defaultValue: false))
{
    await app.CleanAllMartenDataAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map Endpoints
app.MapRider();
app.MapRide();
app.MapWolverineEndpoints();

if (builder.Configuration.GetValue("EnableCli", defaultValue: false))
{
    await app.RunJasperFxCommands(args);
}
{
    await app.RunAsync();
}

// For integration testing
namespace SchraeglagenProtokoll.Api
{
    public abstract partial class Program;
}
