using Marten;
using Marten.AspNetCore;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetRiderHistory
{
    public static void MapGetRiderHistory(this RouteGroupBuilder group)
    {
        group.MapGet("{id}/history", GetRiderHistoryById).WithName("GetRiderHistory").WithOpenApi();
    }

    private static async Task GetRiderHistoryById(
        IQuerySession session,
        Guid id,
        HttpContext httpContext
    )
    {
        await session.Json.WriteById<RiderHistory>(id, httpContext);
    }
}
