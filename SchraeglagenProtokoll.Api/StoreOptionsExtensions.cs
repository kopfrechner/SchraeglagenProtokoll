using Marten;
using Marten.Events.Projections;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Projections;
using Weasel.Core;

namespace SchraeglagenProtokoll.Api;

public static class StoreOptionsExtensions
{
    public static void Setup(this StoreOptions options, string connectionString, bool isDevelopment)
    {
        options.Connection(connectionString);

        options.UseSystemTextJsonForSerialization();

        // If we're running in development mode, let Marten just take care
        // of all necessary schema building and patching behind the scenes
        if (isDevelopment)
        {
            options.AutoCreateSchemaObjects = AutoCreate.All;
        }

        options.Projections.Snapshot<Rider>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<Ride>(SnapshotLifecycle.Inline);
        options.Projections.Add<ScorePerRiderProjection>(ProjectionLifecycle.Async);

        // Recent optimization you'd want with FetchForWriting up above
        options.Projections.UseIdentityMapForAggregates = true;
    }
}
