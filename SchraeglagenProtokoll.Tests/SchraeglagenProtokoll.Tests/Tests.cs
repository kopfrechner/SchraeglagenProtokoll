namespace SchraeglagenProtokoll.Tests;

public class Tests
{
    [ClassDataSource<WebAppFixture>(Shared = SharedType.PerTestSession)]
    public required WebAppFixture WebAppFixture { get; init; }

    [Test]
    public async Task Test()
    {
        var client = WebAppFixture.CreateClient();

        var response = await client.GetAsync("/ping");

        var stringContent = await response.Content.ReadAsStringAsync();

        await Assert.That(stringContent).IsEqualTo("Hello, World!");
    }
}