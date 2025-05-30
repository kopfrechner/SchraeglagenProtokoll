using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Riders;

public class LogRideTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_logging_a_ride_then_it_is_created()
    {
        // Arrange
        var riderId = await StartStream(EventFaker.RiderRegistered());

        var logRideCommand = CommandFaker.LogRide();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(logRideCommand).ToUrl($"/rider/{riderId}/log-ride");
            x.StatusCodeShouldBe(201);
        });

        // Assert
        var createdWithId = result.ReadAsJson<Guid>();
        await DocumentSessionAsync(async session =>
        {
            var ride = await session.LoadAsync<Ride>(createdWithId);
            await Verify(ride);
        });
    }

    [Test]
    public async Task when_logging_a_ride_for_nonexistent_rider_then_bad_request_is_returned()
    {
        // Arrange
        var nonExistentRiderId = Guid.NewGuid();
        var logRideCommand = CommandFaker.LogRide();

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(logRideCommand).ToUrl($"/rider/{nonExistentRiderId}/log-ride");
            x.StatusCodeShouldBe(400);
        });
    }
}
