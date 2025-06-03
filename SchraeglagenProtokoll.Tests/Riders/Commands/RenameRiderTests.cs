using Alba;
using SchraeglagenProtokoll.Api.Riders;
using Shouldly;

namespace SchraeglagenProtokoll.Tests.Riders.Commands;

public class RenameRiderCommandTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_renaming_a_rider_then_it_is_renamed()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));

        var renameRiderCommand = FakeCommand.RenameRider(version: 1);

        // Act
        await Scenario(x =>
        {
            x.Post.Json(renameRiderCommand).ToUrl($"/rider/{riderId}/rename");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var updatedRider = await session.LoadAsync<Rider>(riderId);
            updatedRider.ShouldNotBeNull().FullName.ShouldBe(renameRiderCommand.FullName);
            await Verify(updatedRider);
        });
    }

    [Test]
    public async Task when_renaming_a_rider_then_it_is_not_renamed_if_version_is_wrong()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(
            riderId,
            FakeEvent.RiderRegistered(riderId), // v1
            FakeEvent.RiderRenamed(riderId) // v2
        );

        var wrongVersion = 1; // should be 2
        var renameRiderCommand = FakeCommand.RenameRider(wrongVersion);

        // Act
        await Scenario(x =>
        {
            x.Post.Json(renameRiderCommand).ToUrl($"/rider/{riderId}/rename");
            x.StatusCodeShouldBe(400);
        });

        // Assert
        await DocumentSessionAsync(async session =>
        {
            var updatedRider = await session.LoadAsync<Rider>(riderId);
            updatedRider.ShouldNotBeNull().FullName.ShouldNotBe(renameRiderCommand.FullName);
        });
    }
}
