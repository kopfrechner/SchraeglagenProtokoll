using System.Text.Json.Serialization;
using Marten;
using TUnit.Core.Extensions;
using Weasel.Core;

namespace MartenPlayground.Tests.Setup;

public abstract class TestBase
{
    protected static readonly string ConnectionString =
        "User ID=marten;Password=change-me-123#;Host=localhost;Port=5682;Database=marten";

    protected static IDocumentStore Store(string schema, Action<StoreOptions>? configure = null) =>
        DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);

            // So each test can decide its schema
            options.DatabaseSchemaName = schema;

            // enum serialization
            options.UseSystemTextJsonForSerialization(
                EnumStorage.AsString,
                Casing.Default,
                jsonOptions => jsonOptions.Converters.Add(new JsonStringEnumConverter())
            );

            configure?.Invoke(options);
        });

    protected static IDocumentSession Session(
        string schema,
        Action<StoreOptions>? configure = null
    ) => Store(schema, configure).LightweightSession();

    protected static async Task ResetAllData(string schema)
    {
        await Store(schema).Advanced.ResetAllData();
    }

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
