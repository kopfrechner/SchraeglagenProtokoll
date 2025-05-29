using Marten;
using Microsoft.AspNetCore.Mvc;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class LogRide
{
    public static void MapLogRide(this RouteGroupBuilder group)
    {
        group.MapPost("", LogRideHandler).WithName("LogRide").WithOpenApi();
    }
    
    public record LogRideCommand(
        Guid RideId,
        Guid RiderId,
        DateTimeOffset Date,
        string Start,
        string Destination,
        Distance Distance
    );
    
    public static async Task<IResult> LogRideHandler(
        IDocumentSession session,
        [FromBody] LogRideCommand command
    )
    {
        var (rideId, riderId, date, start, destination, distance) = command;
        
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
