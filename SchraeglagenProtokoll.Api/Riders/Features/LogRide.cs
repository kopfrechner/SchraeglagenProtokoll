using Marten;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class LogRide
{
    public static void MapLogRide(this RouteGroupBuilder group)
    {
        group
            .MapPost("{riderId:guid}/start-ride", LogRideHandler)
            .WithName("LogRide")
            .WithOpenApi();
    }

    public record StartRideCommand(Guid RideId, string Start);

    public static async Task<IResult> LogRideHandler(
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

        var logged = new RideStarted(rideId, riderId, start);
        var stream = session.Events.StartStream<Ride>(rideId, logged);
        await session.SaveChangesAsync();

        return Results.Created($"rides/{stream.Id}", stream.Id);
    }
}
