using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Rides;

public class GetByIdTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_a_ride_by_id_then_it_is_returned()
    {
        // Arrange
        var riderId = await StartStream(FakeEvent.RiderRegistered());
        var rideId = await StartStream(FakeEvent.RideLogged(riderId: riderId));

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
