using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using SchraeglagenProtokoll.Api.Infrastructure.EMail;
using SchraeglagenProtokoll.Api.Riders;

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

        try
        {
            session.Events.Append(rideId, version + 1, rideFinished);
            await session.SaveChangesAsync();

            return Results.Accepted();
        }
        catch (ConcurrencyException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}

public class OnRideFinishedSendEmailNotificationHandler(IEmailService email) : SubscriptionBase
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    )
    {
        var rideFinishedEvents = page.Events.OfType<IEvent<RideFinished>>().ToList();

        var rides = await operations.LoadManyAsync<Ride>(
            rideFinishedEvents.Select(x => x.Data.RideId).ToList()
        );
        var riderList = await operations.LoadManyAsync<Rider>(
            rides.Select(x => x.RiderId).ToList()
        );

        foreach (var ride in rides)
        {
            var rider = riderList.Single(x => x.Id == ride.RiderId);

            await email.SendEmailAsync(
                rider.Email,
                "New Ride Finished",
                $"Congrats, new ride started at {ride.StartLocation} finished at {ride.Destination}"
            );
        }

        return NullChangeListener.Instance;
    }
}
