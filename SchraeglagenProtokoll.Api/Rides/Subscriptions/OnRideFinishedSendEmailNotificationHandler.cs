using JasperFx.Events;
using Marten;
using SchraeglagenProtokoll.Api.Infrastructure.EMail;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides.Projections;

namespace SchraeglagenProtokoll.Api.Rides.Subscriptions;

public static class OnRideFinishedSendEmailNotificationHandler
{
    public static async Task<RideSummary?> LoadAsync(
        IEvent<RideFinished> e,
        IQuerySession session,
        CancellationToken cancellationToken
    )
    {
        return await session.LoadAsync<RideSummary>(e.StreamId, cancellationToken);
    }

    public static async Task Handle(
        IEvent<RideFinished> e,
        IEmailService email,
        IQuerySession session,
        RideSummary rideSummary
    )
    {
        var ride = await session.LoadAsync<Ride>(e.StreamId);
        var rider = await session.LoadAsync<Rider>(ride!.RiderId);

        await email.SendEmailAsync(
            rider!.Email,
            $"Hey {rider.RoadName}, your ride is finished",
            $"Congrats, you just finished a new ride. "
                + $"You started at {rideSummary.StartLocation} finished at {rideSummary.Destination}. "
                + $"It took {rideSummary.Duration} and you drove {rideSummary.Distance}."
        );
    }
}
