using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Rides.Commands;

public class AddLocationTrackTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_adding_location_track_to_active_ride_then_location_is_tracked()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(rideId, FakeEvent.RideStarted(rideId, riderId: riderId));

        var addLocationTrackCommand = FakeCommand.AddLocationTrack(rideId: rideId, version: 1);

        // Act
        var result = await Scenario(x =>
        {
            x.Post.Json(addLocationTrackCommand).ToUrl($"/rides/track-location");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var ride = await session.LoadAsync<Ride>(rideId);
            await Verify(ride);
        });
    }

    [Test]
    public async Task when_adding_location_track_to_nonexistent_ride_then_bad_request_is_returned()
    {
        // Arrange
        var nonExistentRideId = Guid.NewGuid();
        var addLocationTrackCommand = FakeCommand.AddLocationTrack(
            rideId: nonExistentRideId,
            version: 1
        );

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(addLocationTrackCommand).ToUrl($"/rides/track-location");
            x.StatusCodeShouldBe(400);
        });
    }

    [Test]
    public async Task when_adding_location_track_to_finished_ride_then_bad_request_is_returned()
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

        var addLocationTrackCommand = FakeCommand.AddLocationTrack(rideId: rideId, version: 2);

        // Act & Assert
        await Scenario(x =>
        {
            x.Post.Json(addLocationTrackCommand).ToUrl($"/rides/track-location");
            x.StatusCodeShouldBe(400);
        });
    }

    [Test]
    public async Task when_adding_multiple_location_tracks_then_all_locations_are_tracked()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));
        var rideId = Guid.NewGuid();
        await StartStream(rideId, FakeEvent.RideStarted(rideId, riderId: riderId));

        var firstLocationCommand = FakeCommand.AddLocationTrack(
            rideId: rideId,
            version: 1,
            location: "Vienna"
        );
        var secondLocationCommand = FakeCommand.AddLocationTrack(
            rideId: rideId,
            version: 2,
            location: "Salzburg"
        );

        // Act
        await Scenario(x =>
        {
            x.Post.Json(firstLocationCommand).ToUrl($"/rides/track-location");
            x.StatusCodeShouldBe(200);
        });

        await Scenario(x =>
        {
            x.Post.Json(secondLocationCommand).ToUrl($"/rides/track-location");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var ride = await session.LoadAsync<Ride>(rideId);
            await Verify(ride);
        });
    }
}
