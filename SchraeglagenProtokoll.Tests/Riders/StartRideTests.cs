using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Riders;

public class StartRideTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_starting_a_ride_then_it_is_created()
    {
        // Arrange
        var riderId = await StartStream(FakeEvent.RiderRegistered());

        var startRideCommand = FakeCommand.StartRide();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(startRideCommand).ToUrl($"/rider/{riderId}/start-ride");
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
    public async Task when_starting_a_ride_for_nonexistent_rider_then_bad_request_is_returned()
    {
        // Arrange
        var nonExistentRiderId = Guid.NewGuid();
        var startRideCommand = FakeCommand.StartRide();

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(startRideCommand).ToUrl($"/rider/{nonExistentRiderId}/start-ride");
            x.StatusCodeShouldBe(400);
        });
    }

    [Test]
    public async Task when_starting_a_ride_and_there_is_one_unfinished_then_bad_request_is_returned()
    {
        // Arrange
        var riderId = await StartStream(FakeEvent.RiderRegistered());
        var unfinishedRideId = await StartStream(FakeEvent.RideStarted(riderId: riderId));

        var startRideCommand = FakeCommand.StartRide();

        // Act & Assert
        var result = await Scenario(x =>
        {
            x.Post.Json(startRideCommand).ToUrl($"/rider/{riderId}/start-ride");
            x.StatusCodeShouldBe(400);
        });
    }
}
