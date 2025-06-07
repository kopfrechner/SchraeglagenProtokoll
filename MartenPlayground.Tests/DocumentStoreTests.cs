using ImTools;
using Marten;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests;

public record User(Guid Id, string FirstName, string LastName);

public class DocumentStoreTests(PostgresContainerFixture fixture) : DocumentStoreTestBase(fixture)
{
    [Test]
    public async Task O1_create_documentstore_save_document_load_document()
    {
        // Create Marten DocumentStore from connection string
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = nameof(
                O1_create_documentstore_save_document_load_document
            );
        });
        await using var session = store.LightweightSession();

        // Create document
        var user = new User(Guid.NewGuid(), "John", "Doe");
        session.Store(user);
        await session.SaveChangesAsync();

        // Load document
        var loadedUser = await session.LoadAsync<User>(user.Id);
        loadedUser
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                u => u.FirstName.ShouldBe("John"),
                u => u.LastName.ShouldBe("Doe")
            );
    }

    [Test]
    public async Task O2_when_create_test_users_should_succeed()
    {
        User[] users =
        [
            new(Guid.NewGuid(), "Michael", "Smith"),
            new(Guid.NewGuid(), "Sarah", "Johnson"),
            new(Guid.NewGuid(), "David", "Williams"),
            new(Guid.NewGuid(), "Emily", "Brown"),
            new(Guid.NewGuid(), "James", "Anderson"),
        ];

        await using var session = Session();
        session.Store(users);
        await session.SaveChangesAsync();

        session.Query<User>().Count().ShouldBe(5);

        AddToBag("Users", users);
    }

    [Test]
    [DependsOn(nameof(O2_when_create_test_users_should_succeed))]
    public async Task O3_when_load_test_users_then_should_be_5()
    {
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var arrangedUserIds = arrangedUsers.Select(u => u.Id).ToList();

        await using var session = Session();
        var loadedUsers = await session.LoadManyAsync<User>(arrangedUserIds);

        loadedUsers.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(O3_when_load_test_users_then_should_be_5))]
    public async Task O4_when_sahra_johnson_married_then_she_is_upserted_to_sarah_miller()
    {
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var sarahJohnson = arrangedUsers.FindFirst(x =>
            x.FirstName == "Sarah" && x.LastName == "Johnson"
        );

        await using var session = Session();
        session.Store(sarahJohnson with { LastName = "Miller" });
        await session.SaveChangesAsync();

        var sarahMiller = await session.LoadAsync<User>(sarahJohnson.Id);
        sarahMiller.ShouldNotBeNull().ShouldBe(new User(sarahJohnson.Id, "Sarah", "Miller"));
        ;
    }

    [Test]
    [DependsOn(nameof(O3_when_load_test_users_then_should_be_5))]
    public async Task O5_when_deleted_michael_smith_then_michael_smith_is_not_loaded()
    {
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var michaelSmith = arrangedUsers.FindFirst(x =>
            x.FirstName == "Michael" && x.LastName == "Smith"
        );

        await using var session = Session();
        session.Delete<User>(michaelSmith.Id);
        await session.SaveChangesAsync();

        var loadedUsers = await session.LoadManyAsync<User>(arrangedUsers.Select(u => u.Id));
        loadedUsers.Count.ShouldBe(4);
        loadedUsers.ShouldNotContain(michaelSmith);
    }
}
