using ImTools;
using Marten;
using Shouldly;

namespace MartenPlayground.Tests;

public record User(Guid Id, string FirstName, string LastName);

public class DocumentStoreTests(PostgresContainerFixture fixture) : DocumentStoreTestBase(fixture)
{
    [Test]
    public async Task can_create_documentStore()
    {
        // Create Marten DocumentStore from connection string
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = nameof(can_create_documentStore);
        });
        await using var session = store.LightweightSession();

        // Create document
        var user = new User(Guid.NewGuid(), "John", "Doe");
        session.Insert(user);
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
    public async Task can_create_test_users()
    {
        await using var session = Session();

        IList<User> users =
        [
            new(Guid.NewGuid(), "Michael", "Smith"),
            new(Guid.NewGuid(), "Sarah", "Johnson"),
            new(Guid.NewGuid(), "David", "Williams"),
            new(Guid.NewGuid(), "Emily", "Brown"),
            new(Guid.NewGuid(), "James", "Anderson"),
        ];

        session.Insert<User>(users);

        await session.SaveChangesAsync();

        AddToBag("Users", users);
    }

    [Test]
    [DependsOn(nameof(can_create_test_users))]
    public async Task can_load_test_users()
    {
        var arrangedUsers = GetFromBag<IList<User>>(nameof(can_create_test_users), "Users");
        var arrangedUserIds = arrangedUsers.Select(u => u.Id).ToList();

        await using var session = Session();

        var loadedUsers = await session.LoadManyAsync<User>(arrangedUserIds);
        loadedUsers.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(can_load_test_users))]
    public async Task when_deleted_michael_smith_then_michael_smith_is_not_loaded()
    {
        var preparedUsers = GetFromBag<IList<User>>(nameof(can_create_test_users), "Users");
        var firstUser = preparedUsers.FindFirst(x =>
            x.FirstName == "Michael" && x.LastName == "Smith"
        );
        await using var session = Session();

        session.Delete<User>(firstUser.Id);
        await session.SaveChangesAsync();

        var loadedUsers = await session.LoadManyAsync<User>(preparedUsers.Select(u => u.Id));
        loadedUsers.Count.ShouldBe(4);
        loadedUsers.ShouldNotContain(firstUser);
    }
}
