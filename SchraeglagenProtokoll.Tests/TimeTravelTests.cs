using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests;

public class TimeTravelTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    public async Task when_time_travel_by_version_we_get_the_supposed_version(int version)
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId), // V1
            FakeEvent.RiderRenamed(riderId), // V2
            FakeEvent.RiderRenamed(riderId) // V3
        );

        await DocumentSessionAsync(async session =>
        {
            var riderVersion = session.Events.AggregateStreamAsync<Rider>(
                riderId,
                version: version
            );
            await Verify(riderVersion).UseParameters($"V{version}");
        });
    }

    [Test]
    [Arguments(0)]
    [Arguments(50)]
    [Arguments(150)]
    [Arguments(250)]
    [Arguments(350)]
    [Arguments(450)]
    [Arguments(550)]
    [Arguments(650)]
    public async Task when_time_traveling_then_it_works(int millisecondOffset)
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId), // V1
            FakeEvent.RiderRenamed(riderId), // V2
            FakeEvent.RiderRenamed(riderId) // V3
        );

        var rideId = Guid.NewGuid();
        await StartStream(rideId, FakeEvent.RideStarted(rideId, riderId: riderId));

        IList<object> rideEvents =
        [
            FakeEvent.RideLocationTracked(rideId),
            FakeEvent.RideLocationTracked(rideId),
            FakeEvent.RideLocationTracked(rideId),
            FakeEvent.RideLocationTracked(rideId),
            FakeEvent.RideFinished(rideId),
        ];

        var now = DateTimeOffset.UtcNow;
        foreach (var rideEvent in rideEvents)
        {
            await DocumentSessionAsync(async session =>
            {
                session.Events.Append(rideId, rideEvent);
                await session.SaveChangesAsync();
            });

            // add some delay
            await Task.Delay(100);
        }

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var rideVersion = session.Events.AggregateStreamAsync<Ride>(
                rideId,
                timestamp: now.AddMilliseconds(millisecondOffset)
            );
            await Verify(rideVersion).UseParameters($"ms_offset_{millisecondOffset}");
        });
    }
}
