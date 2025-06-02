using Alba;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetRiderHistoryTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_rider_history_with_full_lifecycle_then_all_events_are_tracked()
    {
        // Arrange
        var rider1Id = Guid.NewGuid();
        await StartStream(
            FakeEvent.RiderRegistered(rider1Id),
            FakeEvent.RiderRenamed(),
            FakeEvent.RiderRenamed()
        );

        await StartStream(
            FakeEvent.RideStarted(riderId: rider1Id),
            FakeEvent.RideLocationTracked(),
            FakeEvent.RideFinished()
        );

        var rider2Id = Guid.NewGuid();
        await StartStream(
            FakeEvent.RiderRegistered(rider2Id),
            FakeEvent.RiderRenamed(),
            FakeEvent.RiderRenamed()
        );

        await StartStream(FakeEvent.RideStarted(riderId: rider2Id), FakeEvent.RideFinished());

        // Act
        var resultRider1 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider1Id}/history");
            x.StatusCodeShouldBeOk();
        });

        var resultRider2 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider2Id}/history");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider1History = resultRider1.ReadAsJson<RiderHistory>();
        await Verify(rider1History).UseParameters("rider1");

        var rider2History = resultRider2.ReadAsJson<RiderHistory>();
        await Verify(rider2History).UseParameters("rider2");
    }

    [Test]
    public async Task when_getting_rider_history_of_deleted_account_then_no_details_are_found()
    {
        // Arrange
        var riderId = Guid.NewGuid();

        var riderRegistered = FakeEvent.RiderRegistered(riderId);
        var deleteCommand = FakeEvent.RiderDeletedAccount();

        // Act - Create events in sequence
        await StartStream(riderRegistered, deleteCommand);

        // Get the rider history
        await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/history");
            x.StatusCodeShouldBe(404);
        });
    }
}
