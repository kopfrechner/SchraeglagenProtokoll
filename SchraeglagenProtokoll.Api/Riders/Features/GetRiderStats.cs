using Marten;
using Marten.AspNetCore;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetRiderStats
{
    public static void MapGetRiderStats(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", GetRiderStatsHandler).WithName("GetRiderStats").WithOpenApi();
    }

    private static async Task GetRiderStatsHandler(IQuerySession session, HttpContext httpContext)
    {
        var s = await session.LoadAsync<RiderStats>(RiderStats.DocumentIdentifier);
        await session.Json.WriteById<RiderStats>(RiderStats.DocumentIdentifier, httpContext);
    }
}
