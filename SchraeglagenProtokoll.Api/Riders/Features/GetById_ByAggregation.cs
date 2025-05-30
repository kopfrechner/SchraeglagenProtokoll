using Marten;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetById_ByAggregation
{
    public static void MapGetRiderById_ByAggregation(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}/by-aggregation", GetRiderById_ByAggregation)
            .WithName("GetRiderById_ByAggregation")
            .WithOpenApi();
    }

    private static async Task<IResult> GetRiderById_ByAggregation(
        IQuerySession session,
        [FromRoute] Guid id
    )
    {
        var rider = await session.Events.AggregateStreamAsync<Rider>(id);
        return rider is null ? Results.NotFound() : Results.Ok(rider);
    }
}
