using System.Net.Http.Json;

namespace SchraeglagenProtokoll.Tests.Riders;

public class RegisterRiderTests
{
        [ClassDataSource<WebAppFixture>(Shared = SharedType.PerTestSession)]
        public required WebAppFixture WebAppFixture { get; init; }

        [Test]
        public async Task Test()
        {
            var client = WebAppFixture.CreateClient();

            
            
            var response = await client.PostAsJsonAsync("/riders");

            var stringContent = await response.Content.ReadAsStringAsync();

            await Assert.That(stringContent).IsEqualTo("Hello, World!");
        }
    
}