using Marten;
using Marten.Events.Daemon.Resiliency;
using Oakton;
using Scalar.AspNetCore;
using SchraeglagenProtokoll.Api;
using SchraeglagenProtokoll.Api.Infrastructure;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Features.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add command line parsing for Marten
builder.Host.ApplyOaktonExtensions();

// Learn more about configuring OpenAPI at https://ak.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services to the container
builder.Services.AddEmail(builder.Configuration);

var connectionString =
    builder.Configuration.GetConnectionString("Marten")
    ?? throw new Exception("No connection string found");
var isDevelopment = builder.Environment.IsDevelopment();

// Marten
var martenConfiguration = builder
    .Services.AddMarten(options => options.SetupStoreOptions(connectionString, isDevelopment))
    .AddSubscriptionWithServices<OnRideFinishedSendEmailNotificationHandler>(
        ServiceLifetime.Singleton,
        o => o.IncludeType<RideFinished>()
    )
    // Another performance optimization if you're starting from scratch
    .UseLightweightSessions()
    // Enable projection daemon
    .AddAsyncDaemon(DaemonMode.Solo);

// Initial data
if (builder.Configuration.GetValue("InitializeWithInitialData", defaultValue: false))
{
    martenConfiguration.InitializeWith<InitialData>();
}

// OpenApi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ======================================
// Build app
var app = builder.Build();

// Clean all Marten data
if (builder.Configuration.GetValue("CleanAllMartenData", defaultValue: false))
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

app.MapRider();
app.MapRide();

await app.RunOaktonCommands(args);

namespace SchraeglagenProtokoll.Api
{
    public abstract partial class Program;
}
