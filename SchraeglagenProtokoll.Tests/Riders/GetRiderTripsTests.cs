using Alba;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetRiderTripsTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_rider_history_with_full_lifecycle_then_all_events_are_tracked()
    {
        // Arrange
        var rider1Id = Guid.NewGuid();
        await StartStream(
            FakeEvent.RiderRegistered(rider1Id, "Rider 1", "Rider 1"),
            FakeEvent.RiderRenamed(),
            FakeEvent.RiderRenamed()
        );

        await StartStream(
            FakeEvent.RideStarted(riderId: rider1Id),
            FakeEvent.RideLocationTracked(),
            FakeEvent.RideFinished()
        );

        await StartStream(
            FakeEvent.RideStarted(riderId: rider1Id),
            FakeEvent.RideLocationTracked(),
            FakeEvent.RideLocationTracked(),
            FakeEvent.RideFinished()
        );

        var rider2Id = Guid.NewGuid();
        await StartStream(
            FakeEvent.RiderRegistered(rider2Id, "Rider 2", "Rider 2"),
            FakeEvent.RiderRenamed(),
            FakeEvent.RiderRenamed()
        );

        await StartStream(FakeEvent.RideStarted(riderId: rider2Id), FakeEvent.RideFinished());

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var resultRider1 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider1Id}/trips");
            x.StatusCodeShouldBeOk();
        });

        var resultRider2 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider2Id}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider1History = resultRider1.ReadAsJson<RiderTrips>();
        await Verify(rider1History).UseParameters("rider1");

        var rider2History = resultRider2.ReadAsJson<RiderTrips>();
        await Verify(rider2History).UseParameters("rider2");
    }
}
