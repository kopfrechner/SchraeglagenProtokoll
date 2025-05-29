using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
    
    private static async Task<IResult> GetScorePerRiderHandler(IQuerySession session)
    {
        var scorePerRider = await session.LoadAsync<ScorePerRider>(ScorePerRider.DocumentIdentifier);
        return scorePerRider is null
            ? Results.NotFound()
            : Results.Ok(scorePerRider);
    }
}
