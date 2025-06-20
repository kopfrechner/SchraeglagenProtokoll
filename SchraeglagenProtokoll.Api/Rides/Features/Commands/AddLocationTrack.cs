using JasperFx;
using Marten;

namespace SchraeglagenProtokoll.Api.Rides.Features.Commands;

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
        var locationTracked = new RideLocationTracked(rideId, location);

        try
        {
            session.Events.Append(rideId, version + 1, locationTracked);
            await session.SaveChangesAsync();

            return Results.Accepted();
        }
        catch (ConcurrencyException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}
