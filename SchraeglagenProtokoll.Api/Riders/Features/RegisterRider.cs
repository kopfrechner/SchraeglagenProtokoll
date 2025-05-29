using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class RegisterRider
{
    public static void MapRegisterRider(this RouteGroupBuilder group)
    {
        group.MapPost("", RegisterRiderHandler).WithName("RegisterRider").WithOpenApi();
    }

    public record RegisterRiderCommand(
        Guid RiderId,
        string Email,
        string FullName,
        string NerdAlias
    );

    public static async Task<IResult> RegisterRiderHandler(
        IDocumentSession session,
        RegisterRiderCommand command
    )
    {
        var (riderId, email, fullName, nerdAlias) = command;

        var registeredRider = new RiderRegistered(riderId, email, fullName, nerdAlias);
        var stream = session.Events.StartStream<Rider>(riderId, registeredRider);
        await session.SaveChangesAsync();

        return Results.Created($"riders/{stream.Id}", stream.Id);
    }
}
