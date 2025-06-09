using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Http.Marten;

namespace SchraeglagenProtokoll.Api.Rides.Features.Commands;

//[AggregateHandler]
public static class AddLocationTrack
{
    public record AddLocationTrackCommand(string Location, int Version);

    public static ProblemDetails Validate(Ride ride)
    {
        if (ride.Status == RideStatus.Finished)
        {
            return new ProblemDetails()
            {
                Detail = $"Cannot track location for finished ride {ride.Id}",
                Status = 400,
            };
        }

        // All good, keep on going!
        return WolverineContinue.NoProblems;
    }

    [WolverinePost("rides/{id:guid}/track-location"), EmptyResponse]
    public static RideLocationTracked Handle(
        AddLocationTrackCommand command,
        Guid id,
        [Aggregate] Ride ride
    )
    {
        var (location, _) = command;
        return new RideLocationTracked(id, location);
    }
}
