using Marten;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetById_FromProjection
{
    public static void MapGetRiderById_FromProjection(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}/from-projection", GetRiderById_FromProjection)
            .WithName("GetRiderById_FromProjection")
            .WithOpenApi();
    }

    private static async Task<IResult> GetRiderById_FromProjection(IQuerySession session, Guid id)
    {
        var rider = await session.LoadAsync<Rider>(id);
        return rider is null ? Results.NotFound() : Results.Ok(rider);
    }
}
