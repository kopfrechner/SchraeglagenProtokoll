using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features.Commands;

public static class Delete
{
    public static void MapDeleteRiderById(this RouteGroupBuilder group)
    {
        group.MapDelete("{id}", DeleteById).WithName("DeleteRiderById").WithOpenApi();
    }

    public record DeleteRiderCommand(string? RiderFeedback, int Version);

    public static async Task<IResult> DeleteById(
        IDocumentSession session,
        [FromRoute] Guid id,
        [FromBody] DeleteRiderCommand command
    )
    {
        var (comment, version) = command;
        var riderDeletedAccount = new RiderDeletedAccount(id, comment);
        await session.Events.WriteToAggregate<Rider>(
            id,
            version,
            stream => stream.AppendOne(riderDeletedAccount)
        );
        await session.SaveChangesAsync();

        return Results.NoContent();
    }
}
