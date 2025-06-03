using Marten;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Features.Commands;

public static class StartRide
{
    public static void MapStartRide(this RouteGroupBuilder group)
    {
        group
            .MapPost("{riderId:guid}/start-ride", StartRideHandler)
            .WithName("startRide")
            .WithOpenApi();
    }

    public record StartRideCommand(Guid RideId, string Start);

    public static async Task<IResult> StartRideHandler(
        IDocumentSession session,
        Guid riderId,
        StartRideCommand command
    )
    {
        var (rideId, start) = command;

        var riderExists = await session.Query<Rider>().AnyAsync(x => x.Id == riderId);
        if (!riderExists)
        {
            return Results.BadRequest($"Unknown rider id {riderId}");
        }

        var hasUnfinishedRides = await session
            .Query<Ride>()
            .AnyAsync(x => x.RiderId == riderId && x.Status != RideStatus.Finished);

        if (hasUnfinishedRides)
        {
            return Results.BadRequest($"Rider {riderId} has unfinished rides");
        }

        var logged = new RideStarted(rideId, riderId, start);
        var stream = session.Events.StartStream<Ride>(rideId, logged);
        await session.SaveChangesAsync();

        return Results.Created($"rides/{stream.Id}", stream.Id);
    }
}
