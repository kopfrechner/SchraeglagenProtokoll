using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class GetAll
{
    public static void MapGetAllRider(this RouteGroupBuilder group)
    {
        group
            .MapGet("{id}", GetAllHandler)
            .WithName("GetAll")
            .WithOpenApi();
    }
    
    private static async Task<IResult> GetAllHandler(
        IQuerySession session,
        [FromRoute] Guid id,
        [FromQuery(Name = "search-term")] string? searchTerm,
        [FromQuery(Name = "page-number")] int pageNumber = 2,
        [FromQuery(Name = "page-size")] int pageSize = 10
    )
    {
        var riders = await session.Query<Rider>()
            .Where(x => 
                x.FullName.Contains(searchTerm ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) 
                || x.NerdAlias.Contains(searchTerm ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
            .ToPagedListAsync(pageNumber, pageSize);
        return Results.Ok(riders);
    }
}
