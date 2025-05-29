using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class Delete
{
    public static void MapDeleteRiderById(this RouteGroupBuilder group)
    {
        group.MapDelete("{id}", DeleteById).WithName("DeleteRiderById").WithOpenApi();
    }

    public record DeleteRiderCommand(string? RiderFeedback);

    public static async Task<IResult> DeleteById(
        IDocumentSession session,
        [FromRoute] Guid id,
        [FromBody] DeleteRiderCommand command
    )
    {
        var comment = command.RiderFeedback;
        var riderDeletedAccount = new RiderDeletedAccount(comment);
        await session.Events.WriteToAggregate<Rider>(
            id,
            stream => stream.AppendOne(riderDeletedAccount)
        );
        await session.SaveChangesAsync();

        return Results.NoContent();
    }
}
