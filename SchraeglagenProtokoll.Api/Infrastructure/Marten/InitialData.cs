using Marten;
using Marten.Schema;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Infrastructure.Marten;

internal class InitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();

        // --- Rider GUIDs ---
        // Rider GUIDs (last 4 digits unique)
        var rider1Guid = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Rider 1, renamed, has rides
        var rider2Guid = Guid.Parse("00000000-0000-0000-0000-000000000002"); // Rider 2, has rides
        var rider3Guid = Guid.Parse("00000000-0000-0000-0000-000000000003"); // Deleted rider
        var noRidesRiderGuid = Guid.Parse("00000000-0000-0000-0000-000000000004"); // No rides
        var mixedRiderGuid = Guid.Parse("00000000-0000-0000-0000-000000000005"); // Mixed rides

        // --- Register Riders ---
        await session.StartStream<Rider>(
            rider1Guid,
            new RiderRegistered(
                rider1Guid,
                "schorschi@schraeg.at",
                "Josef Kniewinkel",
                "Schraeglage9000"
            ),
            new RiderRenamed(rider1Guid, "Kurven Raeuber")
        );

        await session.StartStream<Rider>(
            rider2Guid,
            new RiderRegistered(rider2Guid, "max@schraeg.at", "Max Mustermann", "SpeedyGonzales")
        );

        await session.StartStream<Rider>(
            rider3Guid,
            new RiderRegistered(rider3Guid, "ganz@schraeg.at", "Schraeger Rider", "GhostRider")
        );

        await session.StartStream<Rider>(
            noRidesRiderGuid,
            new RiderRegistered(noRidesRiderGuid, "norides@schraeg.at", "Fahr Nix", "Stehplatz")
        );

        await session.StartStream<Rider>(
            mixedRiderGuid,
            new RiderRegistered(mixedRiderGuid, "mixed@schraeg.at", "Anna Schräg", "KurvenQueen")
        );

        // --- Rides for Rider 1 (renamed) ---
        var ride1a = Guid.Parse("00000000-0000-0000-0001-000000000001"); // Rider 1, Ride A
        var ride1b = Guid.Parse("00000000-0000-0000-0002-000000000001"); // Rider 1, Ride B
        await session.StartStream<Ride>(
            ride1a,
            new RideStarted(ride1a, rider1Guid, "Vienna"),
            new RideLocationTracked(ride1a, "Wienerwald"),
            new RideLocationTracked(ride1a, "Tulln"),
            new RideFinished(ride1a, "Krems", new Distance(82.5, DistanceUnit.Kilometers))
        );
        await session.StartStream<Ride>(
            ride1b,
            new RideStarted(ride1b, rider1Guid, "Graz"),
            new RideFinished(ride1b, "Salzburg", new Distance(280.0, DistanceUnit.Kilometers))
        );

        // --- Rides for Rider 2 ---
        var ride2a = Guid.Parse("00000000-0000-0000-0001-000000000002"); // Rider 2, Ride A
        await session.StartStream<Ride>(
            ride2a,
            new RideStarted(ride2a, rider2Guid, "Linz"),
            new RideLocationTracked(ride2a, "Wels"),
            new RideLocationTracked(ride2a, "Attnang"),
            new RideFinished(ride2a, "Vöcklabruck", new Distance(120.0, DistanceUnit.Kilometers)),
            new RideRated(ride2a, SchraeglagenRating.Kurvenspa)
        );
        // unfinished ride
        var ride2b = Guid.Parse("00000000-0000-0000-0002-000000000002"); // Rider 2, Ride B
        await session.StartStream<Ride>(
            ride2b,
            new RideStarted(ride2b, rider2Guid, "Wien"),
            new RideLocationTracked(ride2b, "Stockerau")
        );

        // --- Rides for Mixed Rider ---
        var rideM1 = Guid.Parse("00000000-0000-0000-0001-000000000005"); // Mixed Rider, Ride 1
        await session.StartStream<Ride>(
            rideM1,
            new RideStarted(rideM1, mixedRiderGuid, "Innsbruck"),
            new RideFinished(rideM1, "Bregenz", new Distance(180.0, DistanceUnit.Kilometers))
        );
        var rideM2 = Guid.Parse("00000000-0000-0000-0002-000000000005"); // Mixed Rider, Ride 2
        await session.StartStream<Ride>(
            rideM2,
            new RideStarted(rideM2, mixedRiderGuid, "Villach")
        // Not finished, no tracks
        );

        // --- Deleted rider: one finished ride before deletion ---
        var rideD1 = Guid.Parse("00000000-0000-0000-0001-000000000003"); // Deleted Rider, Ride 1
        await session.StartStream<Ride>(
            rideD1,
            new RideStarted(rideD1, rider3Guid, "Klagenfurt"),
            new RideFinished(rideD1, "Wolfsberg", new Distance(65.0, DistanceUnit.Kilometers))
        );

        // Save all changes
        await session.SaveChangesAsync();
    }
}

internal static class InitialDataExtensions
{
    public static async Task StartStream<T>(
        this IDocumentSession session,
        Guid id,
        params object[] events
    )
        where T : class
    {
        session.Events.StartStream<T>(id, events);
        await session.SaveChangesAsync();
    }
}
