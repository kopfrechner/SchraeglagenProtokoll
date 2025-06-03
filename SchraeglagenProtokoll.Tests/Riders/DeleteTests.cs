using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Riders.Features;
using Shouldly;

namespace SchraeglagenProtokoll.Tests.Riders;

public class DeleteTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_deleting_a_rider_then_projection_is_removed_but_events_remain()
    {
        // Arrange
        var riderId = new Guid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId),
            FakeEvent.RiderRenamed(riderId)
        );

        var deleteCommand = new Delete.DeleteRiderCommand("Test feedback", 2);

        // Act
        await Scenario(x =>
        {
            x.Delete.Json(deleteCommand).ToUrl($"/rider/{riderId}");
            x.StatusCodeShouldBe(204);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            // Projection should not exist
            var rider = await session.LoadAsync<Rider>(riderId);
            rider.ShouldBeNull();

            // Events and stream should still exist
            var events = await session.Events.FetchStreamAsync(riderId);
            events.ShouldNotBeEmpty();
            events.Count.ShouldBe(3); // RiderRegistered + RiderDeletedAccount

            events[0].Data.ShouldBeOfType<RiderRegistered>();
            events[1].Data.ShouldBeOfType<RiderRenamed>();
            events[2].Data.ShouldBeOfType<RiderDeletedAccount>();

            var deletedEvent = (RiderDeletedAccount)events[2].Data;
            deletedEvent.RiderFeedback.ShouldBe("Test feedback");
        });
    }
}
