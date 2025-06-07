using System.Text.Json.Serialization;
using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Services;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Riders.Projections;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Projections;
using Weasel.Core;

namespace SchraeglagenProtokoll.Api;

public static class StoreOptionsExtensions
{
    public static void SetupStoreOptions(
        this StoreOptions options,
        string connectionString,
        bool isDevelopment
    )
    {
        options.Connection(connectionString);

        options.UseSystemTextJsonForSerialization(
            EnumStorage.AsString,
            Casing.Default,
            jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
        );

        // If we're running in development mode, let Marten just take care
        // of all necessary schema building and patching behind the scenes
        if (isDevelopment)
        {
            options.AutoCreateSchemaObjects = AutoCreate.All;
        }

        // Ride
        options.Projections.Snapshot<Ride>(SnapshotLifecycle.Inline);
        options.Projections.Add<RideSummaryProjection>(ProjectionLifecycle.Async);

        // Rider
        options.Projections.Snapshot<Rider>(SnapshotLifecycle.Inline);
        options.Projections.Add<RiderHistoryProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<RiderTripProjection>(ProjectionLifecycle.Async);

        // Mask protected information
        // options.Events.AddMaskingRuleForProtectedInformation<RiderRegistered>(x =>
        //     x with
        //     {
        //         FullName = "****",
        //         Email = "****",
        //         RoadName = "****",
        //     }
        // );
        // options.Events.AddMaskingRuleForProtectedInformation<RiderRenamed>(x =>
        //     x with
        //     {
        //         FullName = "***",
        //     }
        // );

        // Recent optimization you'd want with FetchForWriting up above
        options.Projections.UseIdentityMapForAggregates = true;

        // Turn on the PostgreSQL table partitioning for hot/cold storage on archived events
        options.Events.UseArchivedStreamPartitioning = true;

        // OTEL ===============================================
        // Track Marten connection usage
        options.OpenTelemetry.TrackConnections = TrackLevel.Normal;

        // Track the number of events being appended to the system
        options.OpenTelemetry.TrackEventCounters();

        // Enable OpenTelemetry tracing for docs and events
        options.Policies.ForAllDocuments(x =>
        {
            x.Metadata.CausationId.Enabled = true;
            x.Metadata.CorrelationId.Enabled = true;
        });
        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;
    }
}
