using Marten;
using Marten.AspNetCore;
using SchraeglagenProtokoll.Api.Rides.Projections;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class GetScorePerRider
{
    public static void MapGetScorePerRider(this RouteGroupBuilder group)
    {
        group
            .MapGet("score-per-rider", GetScorePerRiderHandler)
            .WithName("GetScorePerRider")
            .WithOpenApi();
    }

    private static async Task GetScorePerRiderHandler(
        IQuerySession session,
        HttpContext httpContext
    )
    {
        await session.Json.WriteById<ScorePerRider>(ScorePerRider.DocumentIdentifier, httpContext);
    }
}
