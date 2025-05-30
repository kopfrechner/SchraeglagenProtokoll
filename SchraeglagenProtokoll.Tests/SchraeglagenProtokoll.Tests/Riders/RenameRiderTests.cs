using Alba;
using SchraeglagenProtokoll.Api.Riders;
using Shouldly;

namespace SchraeglagenProtokoll.Tests.Riders;

public class RenameRiderCommandTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_renaming_a_rider_then_it_is_renamed()
    {
        // Arrange
        var riderId = await StartStream(EventFaker.RiderRegistered());

        var renameRiderCommand = CommandFaker.RenameRider(version: 1);

        // Act
        await Host.Scenario(x =>
        {
            x.Post.Json(renameRiderCommand).ToUrl($"/rider/{riderId}/rename");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var updatedRider = await session.LoadAsync<Rider>(riderId);
            updatedRider.ShouldNotBeNull();
            updatedRider.FullName.ShouldBe(renameRiderCommand.FullName);
            await Verify(updatedRider);
        });
    }
}
