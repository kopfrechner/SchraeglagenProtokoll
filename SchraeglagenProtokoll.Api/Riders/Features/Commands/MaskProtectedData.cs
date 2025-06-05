using Marten;

namespace SchraeglagenProtokoll.Api.Riders.Features.Commands;

public static class MaskProtectedData
{
    public static void MapMaskProtectedDataRiderById(this RouteGroupBuilder group)
    {
        group
            .MapPost("{id}/mask-protected-data", MaskProtectedDataById)
            .WithName("MaskProtectedDataRiderById")
            .WithOpenApi();
    }

    public static async Task<IResult> MaskProtectedDataById(
        IDocumentSession session,
        Guid id,
        CancellationToken token
    )
    {
        await session.DocumentStore.Advanced.ApplyEventDataMasking(x => x.IncludeStream(id), token);

        return Results.Accepted();
    }
}
