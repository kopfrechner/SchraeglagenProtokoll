using Alba;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetRiderHistoryTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_rider_history_with_full_lifecycle_then_all_events_are_tracked()
    {
        // Arrange
        var riderId = Guid.NewGuid();

        var riderRegistered = EventFaker.RiderRegistered(riderId);
        var firstRename = EventFaker.RiderRenamed();
        var rideLogged = EventFaker.RideLogged(riderId: riderId);
        var commentAdded = EventFaker.CommentAdded(riderId);
        var secondRename = EventFaker.RiderRenamed();
        //var deleteCommand = EventFaker.RiderDeletedAccount();

        // Act - Create events in sequence
        await StartStream(
            riderRegistered,
            firstRename,
            rideLogged,
            commentAdded,
            secondRename
        //    deleteCommand
        );

        // Wait for projection to be updated
        //await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Get the rider history
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/history");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var riderHistory = result.ReadAsJson<RiderHistory>();
        await Verify(riderHistory);
    }
}
