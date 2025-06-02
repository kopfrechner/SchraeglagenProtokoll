using Marten;
using Marten.Schema;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Api;

internal class InitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();

        var rider1Guid = Guid.Parse("c7ebc3c5-51d6-439c-93bb-87b36baac106");
        var rider2Guid = Guid.Parse("88ca015d-ba86-4891-9531-6c65ee7d3640");
        var deletedRiderGuid = Guid.Parse("e4f201e6-0887-480d-ab07-cf81b1161066");

        // Rider 1
        await session.StartStream<Rider>(
            new RiderRegistered(
                rider1Guid,
                "schorschi@schraeg.at",
                "Josef Kniewinkel",
                "Schraeglage9000"
            ),
            new RiderRenamed("Kurven Raeuber")
        );

        // Rider 2
        await session.StartStream<Rider>(
            new RiderRegistered(rider2Guid, "max@schraeg.at", "Max Mustermann", "SpeedyGonzales")
        );

        // Deleted Rider
        await session.StartStream<Rider>(
            new RiderRegistered(
                deletedRiderGuid,
                "deleted@schraeg.at",
                "Deleted Rider",
                "GhostRider"
            ),
            new RiderDeletedAccount("1000PS, sonst nix.")
        );

        await session.SaveChangesAsync();
    }
}

internal static class InitialDataExtensions
{
    public static async Task<Guid> StartStream<T>(
        this IDocumentSession session,
        params IList<object> events
    )
        where T : class
    {
        var id = TryExtractEventIdFromFirstEvent(events);

        var stream = id.HasValue
            ? session.Events.StartStream<T>(id.Value, events)
            : session.Events.StartStream<T>(events);
        await session.SaveChangesAsync();

        return stream.Id;
    }

    private static Guid? TryExtractEventIdFromFirstEvent(IList<object> events)
    {
        return
            events.FirstOrDefault() is { } e
            && e.GetType().GetProperty("Id") is { PropertyType: Type t } p
            && t == typeof(Guid)
            ? p.GetValue(e) as Guid?
            : null;
    }
}
