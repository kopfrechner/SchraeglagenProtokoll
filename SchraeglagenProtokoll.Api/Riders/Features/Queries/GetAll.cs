using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using SchraeglagenProtokoll.Api.Responses;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features.Queries;

public static class GetAll
{
    public static void MapGetAllRider(this RouteGroupBuilder group)
    {
        group.MapGet("", GetAllHandler).WithName("GetAll").WithOpenApi();
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
            var riders = await session.Query<Rider>().ToPagedListAsync(pageNumber, pageSize);
            return Results.Ok(riders.ToResponse());
        }

        var filteredRiders = await session
            .Query<Rider>()
            .Where(x =>
                x.FullName.Contains(searchTerm!, StringComparison.InvariantCultureIgnoreCase)
                || x.RoadName.Contains(searchTerm!, StringComparison.InvariantCultureIgnoreCase)
            )
            .ToPagedListAsync(pageNumber, pageSize);

        return Results.Ok(filteredRiders.ToResponse());
    }
}
