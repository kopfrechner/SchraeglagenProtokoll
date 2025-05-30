using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Tests.Riders;

public class RegisterRiderTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_registering_a_rider_then_it_is_created()
    {
        // Arrange
        var registerRiderCommand = CommandFaker.RegisterRider();

        // Act
        var result = await Host.Scenario(x =>
        {
            x.Post.Json(registerRiderCommand).ToUrl("/rider/register");
            x.StatusCodeShouldBe(201);
        });

        // Assert
        var createdWithId = result.ReadAsJson<Guid>();
        await DocumentSessionAsync(async session =>
        {
            var rider = await session.LoadAsync<Rider>(createdWithId);
            await Verify(rider);
        });
    }
}
