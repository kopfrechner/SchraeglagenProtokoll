using Marten;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class AddLocationTrack
{
    public static void MapAddLocationTrack(this RouteGroupBuilder group)
    {
        group
            .MapPost("{rideId:guid}/track-location", AddLocationTrackHandler)
            .WithName("addLocationTrack")
            .WithOpenApi();
    }

    public record AddLocationTrackCommand(string Location, int Version);

    public static async Task<IResult> AddLocationTrackHandler(
        IDocumentSession session,
        Guid rideId,
        AddLocationTrackCommand command
    )
    {
        var ride = await session.LoadAsync<Ride>(rideId);
        if (ride == null)
        {
            return Results.BadRequest($"Unknown ride id {rideId}");
        }

        if (ride.Status == RideStatus.Finished)
        {
            return Results.BadRequest($"Cannot track location for finished ride {rideId}");
        }

        var (location, version) = command;
        var locationTracked = new RideLocationTracked(location);
        session.Events.Append(rideId, version, locationTracked);
        await session.SaveChangesAsync();

        return Results.Ok();
    }
}
