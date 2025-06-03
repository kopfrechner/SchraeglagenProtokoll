using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Rides.Queries;

public class GetByIdTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_a_ride_by_id_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(rideId, FakeEvent.RideStarted(rideId, riderId: riderId));

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rides/{rideId}");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var ride = result.ReadAsJson<Ride>();
        await Verify(ride);
    }
}
