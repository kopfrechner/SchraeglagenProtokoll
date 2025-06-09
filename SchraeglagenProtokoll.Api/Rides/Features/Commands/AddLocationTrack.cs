using Wolverine;
using Wolverine.Marten;

namespace SchraeglagenProtokoll.Api.Rides.Features.Commands;

[AggregateHandler]
public static class AddLocationTrack
{
    public record AddLocationTrackCommand(Guid Id, string Location, int Version);

    public static void MapAddLocationTrack(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/track-location",
                (AddLocationTrackCommand command, IMessageBus bus) => bus.InvokeAsync(command)
            )
            .WithName("addLocationTrack")
            .WithOpenApi();
    }

    public static IEnumerable<object> Handle(AddLocationTrackCommand command, Ride ride)
    {
        if (ride.Status == RideStatus.Finished)
        {
            throw new InvalidCommandException($"Cannot track location for finished ride {ride.Id}");
        }

        var (rideId, location, version) = command;
        yield return new RideLocationTracked(rideId, location);
    }
}

public class InvalidCommandException(string message) : Exception(message);
