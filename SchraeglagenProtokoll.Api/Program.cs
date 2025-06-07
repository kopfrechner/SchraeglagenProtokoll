using JasperFx;
using JasperFx.Events.Daemon;
using Marten;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using SchraeglagenProtokoll.Api;
using SchraeglagenProtokoll.Api.Infrastructure;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Subscriptions;

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
            .SetupArchivingOptions()
            //.SetupMaskingPolicies() // Not supported yet https://github.com/JasperFx/marten/pull/3831
            .SetupOpenTelemetry()
    )
    // Event subscription
    .AddSubscriptionWithServices<OnRideFinishedSendEmailNotificationHandler>(
        ServiceLifetime.Singleton,
        o => o.IncludeType<RideFinished>()
    )
    // Another performance optimization if you're starting from scratch
    .UseLightweightSessions()
    // Enable projection daemon
    .AddAsyncDaemon(DaemonMode.Solo)
    // Initialize sample data
    .ShouldInitializeSampleData(
        builder.Configuration.GetValue("ResetSampleData", defaultValue: false)
    );

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

if (builder.Configuration.GetValue("EnableCli", defaultValue: false))
{
    await app.RunJasperFxCommands(args);
}
{
    await app.RunAsync();
}

// For integration testing
public abstract partial class Program;
