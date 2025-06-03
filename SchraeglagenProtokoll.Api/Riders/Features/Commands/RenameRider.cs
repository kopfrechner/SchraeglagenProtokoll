using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features.Commands;

public static class RenameRider
{
    public static void MapRenameRider(this RouteGroupBuilder group)
    {
        group.MapPost("{id:guid}/rename", RenameRiderHandler).WithName("RenameRider").WithOpenApi();
    }

    public record RenameRiderCommand(string FullName, int Version);

    public static async Task<IResult> RenameRiderHandler(
        IDocumentSession session,
        [FromRoute] Guid id,
        [FromBody] RenameRiderCommand command
    )
    {
        var (newFullName, version) = command;
        var riderRenamed = new RiderRenamed(id, newFullName);
        try
        {
            await session.Events.WriteToAggregate<Rider>(
                id,
                version,
                stream => stream.AppendOne(riderRenamed)
            );
            return Results.Ok();
        }
        catch (ConcurrencyException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}
