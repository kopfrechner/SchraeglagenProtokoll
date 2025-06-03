using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Rides.Features.Queries;

public static class GetAll
{
    public static void MapGetAllRides(this RouteGroupBuilder group)
    {
        group.MapGet("", GetAllHandler).WithName("GetAllRides").WithOpenApi();
    }

    private static async Task<IResult> GetAllHandler(
        IQuerySession session,
        [FromQuery(Name = "search-term")] string? searchTerm,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 10
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            var riders = await session.Query<Ride>().ToPagedListAsync(pageNumber, pageSize);
            return Results.Ok(riders);
        }

        var filteredRides = await session
            .Query<Ride>()
            .Where(r =>
                r.TrackedLocations.Any(l =>
                    l.Contains(searchTerm!, StringComparison.InvariantCultureIgnoreCase)
                )
            )
            .ToPagedListAsync(pageNumber, pageSize);

        return Results.Ok(filteredRides);
    }
}
