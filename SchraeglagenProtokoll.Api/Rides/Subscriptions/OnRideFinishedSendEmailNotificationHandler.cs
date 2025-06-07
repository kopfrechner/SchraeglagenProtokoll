using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Services;
using Marten.Subscriptions;
using SchraeglagenProtokoll.Api.Infrastructure.EMail;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Api.Rides.Subscriptions;

public class OnRideFinishedSendEmailNotificationHandler(IEmailService email) : SubscriptionBase
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    ) => await Task.FromResult(new Callback(email));

    public class Callback(IEmailService email) : IChangeListener
    {
        public async Task AfterCommitAsync(
            IDocumentSession session,
            IChangeSet commit,
            CancellationToken token
        )
        {
            var rideFinishedEvents = commit.GetEvents().OfType<IEvent<RideFinished>>().ToList();

            var rides = await session.LoadManyAsync<Ride>(
                token,
                rideFinishedEvents.Select(x => x.Data.RideId).ToList()
            );
            var riderList = await session.LoadManyAsync<Rider>(
                token,
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
        }

        public async Task BeforeCommitAsync(
            IDocumentSession session,
            IChangeSet commit,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
        }
    }
}
