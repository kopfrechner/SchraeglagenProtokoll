using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Rides.Commands;

public class FinishRideTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_finishing_an_active_ride_then_ride_is_finished()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));

        var rideId = Guid.NewGuid();
        await StartStream(rideId, FakeEvent.RideStarted(rideId, riderId: riderId));

        var finishRideCommand = FakeCommand.FinishRide(version: 1);

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(finishRideCommand).ToUrl($"/rides/{rideId}/finish");
            x.StatusCodeShouldBe(202);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var ride = await session.LoadAsync<Ride>(rideId);
            await Verify(ride);
        });
    }

    [Test]
    public async Task when_finishing_a_nonexistent_ride_then_bad_request_is_returned()
    {
        // Arrange
        var nonExistentRideId = Guid.NewGuid();
        var finishRideCommand = FakeCommand.FinishRide(version: 1);

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(finishRideCommand).ToUrl($"/rides/{nonExistentRideId}/finish");
            x.StatusCodeShouldBe(400);
        });
    }

    [Test]
    public async Task when_finishing_an_already_finished_ride_then_bad_request_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(
            rideId,
            FakeEvent.RideStarted(rideId, riderId: riderId),
            FakeEvent.RideFinished(rideId)
        );

        var finishRideCommand = FakeCommand.FinishRide(version: 2);

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(finishRideCommand).ToUrl($"/rides/{rideId}/finish");
            x.StatusCodeShouldBe(400);
        });
    }

    [Test]
    public async Task when_finishing_ride_with_tracked_locations_then_all_data_is_preserved()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(
            rideId,
            FakeEvent.RideStarted(rideId, riderId: riderId),
            FakeEvent.RideLocationTracked(rideId, "Vienna"),
            FakeEvent.RideLocationTracked(rideId, "Salzburg")
        );

        var finishRideCommand = FakeCommand.FinishRide(
            version: 3,
            destination: "Innsbruck",
            distance: new Distance(250, DistanceUnit.Kilometers)
        );

        // Act
        await Scenario(x =>
        {
            x.Post.Json(finishRideCommand).ToUrl($"/rides/{rideId}/finish");
            x.StatusCodeShouldBe(202);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var ride = await session.LoadAsync<Ride>(rideId);
            await Verify(ride);
        });
    }

    [Test]
    public async Task when_starting_new_ride_after_finishing_previous_then_it_is_allowed()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(
            rideId,
            FakeEvent.RideStarted(rideId, riderId: riderId),
            FakeEvent.RideFinished(rideId)
        );

        var startNewRideCommand = FakeCommand.StartRide();

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(startNewRideCommand).ToUrl($"/rider/{riderId}/start-ride");
            x.StatusCodeShouldBe(201);
        });

        // Assert
        var newRideId = result.ReadAsJson<Guid>();
        await DocumentSessionAsync(async session =>
        {
            var newRide = await session.LoadAsync<Ride>(newRideId);
            await Verify(newRide);
        });
    }
}
