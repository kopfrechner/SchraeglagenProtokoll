using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class GetById
{
    public static void MapGetRideById(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}", GetRideByIdHandler)
            .WithName("GetRideById")
            .WithOpenApi();
    }
    
    private static async Task<IResult> GetRideByIdHandler(
        IQuerySession session,
        [FromRoute] Guid id
    )
    {
        var ride = await session.Events.AggregateStreamAsync<Ride>(id);
        return ride is null
            ? Results.NotFound()
            : Results.Ok(ride);
    }
}
