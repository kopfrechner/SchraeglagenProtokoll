using Marten;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class RegisterRider
{
    public static void MapRegisterRider(this RouteGroupBuilder group)
    {
        group.MapPost("/register", RegisterRiderHandler).WithName("RegisterRider").WithOpenApi();
    }

    public record RegisterRiderCommand(
        Guid? RiderId,
        string Email,
        string FullName,
        string RoadName
    );

    public static async Task<IResult> RegisterRiderHandler(
        IDocumentSession session,
        RegisterRiderCommand command
    )
    {
        var (riderId, email, fullName, roadName) = command;

        riderId ??= Guid.NewGuid();

        var registeredRider = new RiderRegistered(riderId.Value, email, fullName, roadName);
        var stream = session.Events.StartStream<Rider>(riderId.Value, registeredRider);
        await session.SaveChangesAsync();

        return Results.Created($"riders/{stream.Id}", stream.Id);
    }
}
