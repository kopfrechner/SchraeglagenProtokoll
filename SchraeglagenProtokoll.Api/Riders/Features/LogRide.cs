using Marten;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class LogRide
{
    public static void MapLogRide(this RouteGroupBuilder group)
    {
        group.MapPost("{riderId:guid}/log-ride", LogRideHandler).WithName("LogRide").WithOpenApi();
    }

    public record LogRideCommand(
        Guid RideId,
        DateTimeOffset Date,
        string Start,
        string Destination,
        Distance Distance
    );

    public static async Task<IResult> LogRideHandler(
        IDocumentSession session,
        Guid riderId,
        LogRideCommand command
    )
    {
        var (rideId, date, start, destination, distance) = command;

        var riderExists = await session.Query<Rider>().AnyAsync(x => x.Id == riderId);
        if (!riderExists)
        {
            return Results.BadRequest($"Unknown rider id {riderId}");
        }

        var logged = new RideLogged(rideId, riderId, date, start, destination, distance);
        var stream = session.Events.StartStream<Ride>(rideId, logged);
        await session.SaveChangesAsync();

        return Results.Created($"rides/{stream.Id}", stream.Id);
    }
}
