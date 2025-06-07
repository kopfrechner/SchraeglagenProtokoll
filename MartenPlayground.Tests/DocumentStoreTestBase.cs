using System.Text.Json.Serialization;
using Marten;
using TUnit.Core.Extensions;
using Weasel.Core;

namespace MartenPlayground.Tests;

[ClassDataSource<PostgresContainerFixture>(Shared = SharedType.PerTestSession)]
public abstract class DocumentStoreTestBase(PostgresContainerFixture fixture)
{
    protected string ConnectionString => fixture.Container.GetConnectionString();

    protected IDocumentStore Store(Action<StoreOptions>? configure = null) =>
        DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);

            // enum serialization
            options.UseSystemTextJsonForSerialization(
                EnumStorage.AsString,
                Casing.Default,
                jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
            );

            configure?.Invoke(options);
        });

    protected IDocumentSession Session(Action<StoreOptions>? configure = null) =>
        Store(configure).LightweightSession();

    protected void AddToBag(string key, object value)
    {
        TestContext.Current!.ObjectBag.Add(key, value);
    }

    protected T GetFromBag<T>(string sourceTestName, string key)
    {
        var sourceTest = TestContext.Current?.GetTests(sourceTestName).FirstOrDefault();
        var success = sourceTest.ObjectBag.TryGetValue(key, out var value);
        if (!success)
            throw new InvalidOperationException($"Key {key} not found in bag");

        return (T)value!;
    }
}
