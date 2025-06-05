using Marten;

namespace SchraeglagenProtokoll.Api.Riders.Features.Commands;

public static class RebuildProjections
{
    public static void MapRebuildProjectionsById(this RouteGroupBuilder group)
    {
        group
            .MapPost("{id}/rebuild-projections", RebuildProjectionsById)
            .WithName("RebuildProjectionsById")
            .WithOpenApi();
    }

    public static async Task<IResult> RebuildProjectionsById(
        IDocumentSession session,
        Guid id,
        CancellationToken token
    )
    {
        await session.DocumentStore.Advanced.RebuildSingleStreamAsync<Rider>(id);

        return Results.Accepted();
    }
}
