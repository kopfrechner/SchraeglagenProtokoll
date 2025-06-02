using Marten;
using Marten.AspNetCore;
using SchraeglagenProtokoll.Api.Riders.Projections;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetRiderTrips
{
    public static void MapGetRiderTrips(this RouteGroupBuilder group)
    {
        group.MapGet("{id}/trips", GetRiderTripsById).WithName("GetRiderTrips").WithOpenApi();
    }

    private static async Task GetRiderTripsById(
        IQuerySession session,
        Guid id,
        HttpContext httpContext
    )
    {
        await session.Json.WriteById<RiderTrips>(id, httpContext);
    }
}
