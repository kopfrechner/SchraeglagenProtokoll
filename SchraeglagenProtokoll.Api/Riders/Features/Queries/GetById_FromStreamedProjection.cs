using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features.Queries;

public static class GetById_FromStreamedProjection
{
    public static void MapGetRiderById_FromStreamedProjection(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}/from-streamed-projection", GetRiderById_FromStreamedProjection)
            .WithName("GetRiderById_FromStreamedProjection")
            .WithOpenApi();
    }

    private static async Task GetRiderById_FromStreamedProjection(
        IQuerySession session,
        [FromRoute] Guid id,
        HttpContext context
    )
    {
        await session.Json.WriteById<Rider>(id, context);
    }
}
