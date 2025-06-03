using Alba;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders.Queries;

public class GetRiderHistoryTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_rider_history_with_full_lifecycle_then_all_events_are_tracked()
    {
        // Arrange
        var rider1Id = Guid.NewGuid();
        await StartStream(
            rider1Id,
            FakeEvent.RiderRegistered(rider1Id),
            FakeEvent.RiderRenamed(rider1Id),
            FakeEvent.RiderRenamed(rider1Id)
        );

        var ride1Id = Guid.NewGuid();
        await StartStream(
            ride1Id,
            FakeEvent.RideStarted(ride1Id, riderId: rider1Id),
            FakeEvent.RideLocationTracked(ride1Id),
            FakeEvent.RideFinished(ride1Id)
        );

        var rider2Id = Guid.NewGuid();
        await StartStream(
            rider2Id,
            FakeEvent.RiderRegistered(rider2Id),
            FakeEvent.RiderRenamed(rider2Id),
            FakeEvent.RiderRenamed(rider2Id)
        );

        var ride2Id = Guid.NewGuid();
        await StartStream(
            ride2Id,
            FakeEvent.RideStarted(ride2Id, riderId: rider2Id),
            FakeEvent.RideFinished(ride2Id)
        );

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
        var deleteCommand = FakeEvent.RiderDeletedAccount(riderId);

        // Act - Create events in sequence
        await StartStream(riderId, riderRegistered, deleteCommand);

        // Get the rider history
        await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/history");
            x.StatusCodeShouldBe(404);
        });
    }
}
