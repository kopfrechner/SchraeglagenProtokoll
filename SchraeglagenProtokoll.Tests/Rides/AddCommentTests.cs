using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Features;

namespace SchraeglagenProtokoll.Tests.Rides;

public class AddCommentTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_adding_two_comments_to_a_ride_then_both_are_added()
    {
        // Arrange
        var riderAId = await StartStream(FakeEvent.RiderRegistered());
        var riderBId = await StartStream(FakeEvent.RiderRegistered());
        var rideId = await StartStream(FakeEvent.RideLogged(riderId: riderAId));

        var firstComment = new AddComment.AddCommentCommand(
            CommentedBy: riderBId,
            Text: "Great ride!",
            Version: 1
        );

        var secondComment = new AddComment.AddCommentCommand(
            CommentedBy: riderAId,
            Text: "Yeah, beautiful scenery on this route",
            Version: 2
        );

        // Act
        await Scenario(x =>
        {
            x.Post.Json(firstComment).ToUrl($"/rides/{rideId}/comment");
            x.StatusCodeShouldBe(200);
        });

        await Scenario(x =>
        {
            x.Post.Json(secondComment).ToUrl($"/rides/{rideId}/comment");
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
