using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetRiderStatsTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_score_per_rider_with_multiple_riders_and_rides_then_totals_are_calculated()
    {
        // Arrange
        var rider1Id = await StartStream(FakeEvent.RiderRegistered());
        var rider2Id = await StartStream(FakeEvent.RiderRegistered());
        var rider3Id = await StartStream(FakeEvent.RiderRegistered());
        var rider4Id = await StartStream(FakeEvent.RiderRegistered());
        var rider5Id = await StartStream(FakeEvent.RiderRegistered());

        // Add multiple rides for each rider
        await StartStream(FakeEvent.RideLogged(riderId: rider1Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider1Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider2Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider3Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider4Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider5Id));
        await StartStream(FakeEvent.RideLogged(riderId: rider5Id));

        // The daemon updates the projection asynchronously
        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider/stats");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var scorePerRider = result.ReadAsJson<RiderStats>();
        await Verify(scorePerRider);
    }
}
