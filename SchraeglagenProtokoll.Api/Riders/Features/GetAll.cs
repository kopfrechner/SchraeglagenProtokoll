using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features;

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
            return Results.Ok(riders);
        }

        var filteredRiders = await session
            .Query<Rider>()
            .Where(x =>
                x.FullName.Contains(searchTerm!, StringComparison.InvariantCultureIgnoreCase)
                || x.NerdAlias.Contains(searchTerm!, StringComparison.InvariantCultureIgnoreCase)
            )
            .ToPagedListAsync(pageNumber, pageSize);

        return Results.Ok(filteredRiders);
    }
}
