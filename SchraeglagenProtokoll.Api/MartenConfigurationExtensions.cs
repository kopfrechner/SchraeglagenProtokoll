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

public static class MartenConfigurationExtensions
{
    public static MartenServiceCollectionExtensions.MartenConfigurationExpression ShouldInitializeSampleData(
        this MartenServiceCollectionExtensions.MartenConfigurationExpression services,
        bool initialize
    )
    {
        if (initialize)
            services.InitializeWith<InitialData>();
        return services;
    }
}

public static class StoreOptionsExtensions
{
    public static StoreOptions SetupDatabase(
        this StoreOptions options,
        string connectionString,
        bool isDevelopment
    )
    {
        options.Connection(connectionString);

        // If we're running in development mode, let Marten just take care
        // of all necessary schema building and patching behind the scenes
        if (isDevelopment)
        {
            options.AutoCreateSchemaObjects = AutoCreate.All;
        }

        return options;
    }

    public static StoreOptions SetupJsonSerialization(this StoreOptions options)
    {
        options.UseSystemTextJsonForSerialization(
            EnumStorage.AsString,
            Casing.Default,
            jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
        );

        return options;
    }

    public static StoreOptions SetupProjections(this StoreOptions options)
    {
        // Ride
        options.Projections.Snapshot<Ride>(SnapshotLifecycle.Inline);
        options.Projections.Add<RideSummaryProjection>(ProjectionLifecycle.Async);

        // Rider
        options.Projections.Snapshot<Rider>(SnapshotLifecycle.Inline);
        options.Projections.Add<RiderHistoryProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<RiderTripProjection>(ProjectionLifecycle.Async);

        // Recent optimization you'd want with FetchForWriting up above
        options.Projections.UseIdentityMapForAggregates = true;

        return options;
    }

    public static StoreOptions SetupMaskingPolicies(this StoreOptions options)
    {
        throw new InvalidOperationException(
            "Wait for Record Support https://github.com/JasperFx/marten/pull/3831"
        );

        // Mask protected information
        options.Events.AddMaskingRuleForProtectedInformation<RiderRegistered>(x =>
            x with
            {
                FullName = "****",
                Email = "****",
                RoadName = "****",
            }
        );
        options.Events.AddMaskingRuleForProtectedInformation<RiderRenamed>(x =>
            x with
            {
                FullName = "***",
            }
        );

        return options;
    }

    public static StoreOptions SetupArchiving(this StoreOptions options)
    {
        // Turn on the PostgreSQL table partitioning for hot/cold storage on archived events
        options.Events.UseArchivedStreamPartitioning = true;

        return options;
    }

    public static StoreOptions SetupOpenTelemetry(this StoreOptions options)
    {
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

        return options;
    }
}
