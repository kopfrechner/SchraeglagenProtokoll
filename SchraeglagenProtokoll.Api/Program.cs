using Marten;
using Marten.Events.Daemon.Resiliency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using SchraeglagenProtokoll.Api;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://ak.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("Marten")
    ?? throw new Exception("No connection string found");
var isDevelopment = builder.Environment.IsDevelopment();

// Marten
builder.Services.AddMarten(options => options.Setup(connectionString, isDevelopment))
    // Another performance optimization if you're starting from scratch
    .UseLightweightSessions()
    // Enable projection daemon
    .AddAsyncDaemon(DaemonMode.Solo);

// OpenApi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ======================================
// Build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapRider();
app.MapRide();

app.Run();
