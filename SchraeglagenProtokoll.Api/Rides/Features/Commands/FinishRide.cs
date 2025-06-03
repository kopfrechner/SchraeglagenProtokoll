using Marten;

namespace SchraeglagenProtokoll.Api.Rides.Features.Commands;

public static class FinishRide
{
    public static void MapFinishRide(this RouteGroupBuilder group)
    {
        group
            .MapPost("{rideId:guid}/finish", FinishRideHandler)
            .WithName("finishRide")
            .WithOpenApi();
    }

    public record FinishRideCommand(string Destination, Distance Distance, int Version);

    public static async Task<IResult> FinishRideHandler(
        IDocumentSession session,
        Guid rideId,
        FinishRideCommand command
    )
    {
        var ride = await session.LoadAsync<Ride>(rideId);
        if (ride == null)
        {
            return Results.BadRequest($"Unknown ride id {rideId}");
        }

        if (ride.Status == RideStatus.Finished)
        {
            return Results.BadRequest($"Ride {rideId} is already finished");
        }

        var (destination, distance, version) = command;
        var rideFinished = new RideFinished(rideId, destination, distance);

        session.Events.Append(rideId, version + 1, rideFinished);
        await session.SaveChangesAsync();

        // await session.Events.WriteToAggregate<Ride>(
        //     rideId,
        //     version,
        //     stream => stream.AppendOne(rideFinished)
        // );

        return Results.Ok();
    }
}
