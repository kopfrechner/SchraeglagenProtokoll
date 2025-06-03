using Alba;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Tests.Riders.Queries;

public class GetRiderTripsTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test, Skip("Only works when executed separately")]
    public async Task when_getting_rider_trips_with_multiple_finished_trips_then_all_trips_are_returned()
    {
        // Arrange
        var rider1Id = Guid.NewGuid();
        await StartStream(
            rider1Id,
            FakeEvent.RiderRegistered(rider1Id, "Alex Motorrad", "SpeedDemon")
        );

        // First finished trip
        var ride1Id = Guid.NewGuid();
        await StartStream(
            ride1Id,
            FakeEvent.RideStarted(ride1Id, riderId: rider1Id, startLocation: "Vienna"),
            FakeEvent.RideLocationTracked(ride1Id, "Salzburg"),
            FakeEvent.RideFinished(ride1Id, destination: "Innsbruck")
        );

        // Second finished trip
        var ride2Id = Guid.NewGuid();
        await StartStream(
            ride2Id,
            FakeEvent.RideStarted(ride2Id, riderId: rider1Id, startLocation: "Innsbruck"),
            FakeEvent.RideLocationTracked(ride2Id, "Kitzbuehel"),
            FakeEvent.RideLocationTracked(ride2Id, "Zell am See"),
            FakeEvent.RideFinished(ride2Id, destination: "Vienna")
        );

        var rider2Id = Guid.NewGuid();
        await StartStream(
            rider2Id,
            FakeEvent.RiderRegistered(rider2Id, "Maria Biker", "CurveQueen")
        );

        // Single finished trip for rider2
        var ride3Id = Guid.NewGuid();
        await StartStream(
            ride3Id,
            FakeEvent.RideStarted(ride3Id, riderId: rider2Id, startLocation: "Graz"),
            FakeEvent.RideFinished(ride3Id, destination: "Linz")
        );

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var resultRider1 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider1Id}/trips");
            x.StatusCodeShouldBeOk();
        });

        var resultRider2 = await Scenario(x =>
        {
            x.Get.Url($"/rider/{rider2Id}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider1Trips = resultRider1.ReadAsJson<RiderTrips>();
        await Verify(rider1Trips).UseParameters("rider1");

        var rider2Trips = resultRider2.ReadAsJson<RiderTrips>();
        await Verify(rider2Trips).UseParameters("rider2");
    }

    [Test, Skip("Only works when executed separately")]
    public async Task when_getting_rider_trips_with_unfinished_rides_then_only_finished_trips_are_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId, "Unfinished Rider", "NeverEnds")
        );

        // Finished trip - should appear in results
        var ride1Id = Guid.NewGuid();
        await StartStream(
            ride1Id,
            FakeEvent.RideStarted(ride1Id, riderId: riderId, startLocation: "Munich"),
            FakeEvent.RideLocationTracked(ride1Id, "Rosenheim"),
            FakeEvent.RideFinished(ride1Id, destination: "Salzburg")
        );

        // Unfinished trip - should NOT appear in results
        var ride2Id = Guid.NewGuid();
        await StartStream(
            ride2Id,
            FakeEvent.RideStarted(ride2Id, riderId: riderId, startLocation: "Salzburg"),
            FakeEvent.RideLocationTracked(ride2Id, "Bad Ischl"),
            FakeEvent.RideLocationTracked(ride2Id, "Hallstatt")
        // No RideFinished event
        );

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var riderTrips = result.ReadAsJson<RiderTrips>();
        await Verify(riderTrips);
    }

    [Test, Skip("Only works when executed separately")]
    public async Task when_getting_rider_trips_for_rider_with_no_finished_trips_then_empty_trips_list_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId, "New Rider", "FreshStart"));

        // Only unfinished ride
        var ride1Id = Guid.NewGuid();
        await StartStream(
            ride1Id,
            FakeEvent.RideStarted(ride1Id, riderId: riderId, startLocation: "Berlin"),
            FakeEvent.RideLocationTracked(ride1Id, "Dresden")
        // No RideFinished event
        );

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var riderTrips = result.ReadAsJson<RiderTrips>();
        await Verify(riderTrips);
    }

    [Test, Skip("Only works when executed separately")]
    public async Task when_getting_rider_trips_for_rider_with_no_rides_then_empty_trips_list_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId, "No Rides Rider", "StayHome")
        );

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var riderTrips = result.ReadAsJson<RiderTrips>();
        await Verify(riderTrips);
    }

    [Test, Skip("Only works when executed separately")]
    public async Task when_getting_rider_trips_for_nonexistent_rider_then_not_found_is_returned()
    {
        // Arrange
        var nonExistentRiderId = Guid.NewGuid();

        // Act & Assert
        await Scenario(x =>
        {
            x.Get.Url($"/rider/{nonExistentRiderId}/trips");
            x.StatusCodeShouldBe(404);
        });
    }

    [Test, Skip("Only works when executed separately")]
    public async Task when_rider_finishes_trip_after_multiple_location_tracks_then_trip_contains_start_and_end_locations()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId, "Track Master", "GpsLover"));

        // Trip with many location tracks
        var rideId = Guid.NewGuid();
        await StartStream(
            rideId,
            FakeEvent.RideStarted(rideId, riderId: riderId, startLocation: "Frankfurt"),
            FakeEvent.RideLocationTracked(rideId, "Wurzburg"),
            FakeEvent.RideLocationTracked(rideId, "Bamberg"),
            FakeEvent.RideLocationTracked(rideId, "Bayreuth"),
            FakeEvent.RideLocationTracked(rideId, "Hof"),
            FakeEvent.RideFinished(rideId, destination: "Prague")
        );

        await WaitForNonStaleProjectionDataAsync(15.Seconds());

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/trips");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var riderTrips = result.ReadAsJson<RiderTrips>();
        await Verify(riderTrips);
    }
}
