using Marten;
using Marten.AspNetCore;

namespace SchraeglagenProtokoll.Api.Rides.Features.Queries;

public static class GetById
{
    public static void MapGetRideById(this RouteGroupBuilder group)
    {
        group.MapGet("{id}", GetRideByIdHandler).WithName("GetRideById").WithOpenApi();
    }

    private static async Task GetRideByIdHandler(
        IQuerySession session,
        HttpContext httpContext,
        Guid id
    )
    {
        await session.Json.WriteById<Ride>(id, httpContext);
    }
}
