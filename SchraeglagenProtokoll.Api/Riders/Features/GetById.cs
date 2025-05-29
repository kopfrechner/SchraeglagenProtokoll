using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetById
{
    public static void MapGetRiderById(this RouteGroupBuilder group)
    {
        group.MapGet("{id}", GetRiderById_ByAggregation).WithName("GetRiderById").WithOpenApi();
    }

    private static async Task<IResult> GetRiderById_ByAggregation(
        IQuerySession session,
        [FromRoute] Guid id
    )
    {
        var rider = await session.Events.AggregateStreamAsync<Rider>(id);
        return rider is null ? Results.NotFound() : Results.Ok(rider);
    }

    private static async Task<IResult> GetRiderById_FromProjection(
        IQuerySession session,
        [FromRoute] Guid id
    )
    {
        var rider = await session.Events.LoadAsync<Rider>(id);
        return rider is null ? Results.NotFound() : Results.Ok(rider);
    }

    private static async Task GetRiderById_StreamedProjection(
        IQuerySession session,
        [FromRoute] Guid id,
        HttpContext context
    )
    {
        await session.Json.WriteById<Rider>(id, context);
    }
}
