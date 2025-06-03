using Marten;
using Marten.AspNetCore;
using SchraeglagenProtokoll.Api.Rides.Projections;

namespace SchraeglagenProtokoll.Api.Rides.Features.Queries;

public static class GetSummaryInfoById
{
    public static void MapGetRideSummaryInfoById(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}/summary", GetRideSummaryByIdHandler)
            .WithName("GetRideSummaryInfoById")
            .WithOpenApi();
    }

    private static async Task GetRideSummaryByIdHandler(
        IQuerySession session,
        HttpContext httpContext,
        Guid id
    )
    {
        await session.Json.WriteById<RideSummary>(id, httpContext);
    }
}
