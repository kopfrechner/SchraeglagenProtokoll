using JasperFx;
using JasperFx.Events;
using Marten;
using Marten.Exceptions;
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
        try
        {
            var (comment, version) = command;
            await session.Events.WriteToAggregate<Rider>(
                id,
                version,
                stream => stream.AppendOne(new RiderDeletedAccount(id, comment))
            );

            // We need a separate transaction for archiving, otherwise RiderDeletedAccount won't be published
            await session.Events.WriteToAggregate<Rider>(
                id,
                version + 1,
                stream => stream.AppendOne(new Archived($"Rider deleted account: {comment}"))
            );

            return Results.NoContent();
        }
        catch (ConcurrencyException e)
        {
            return Results.BadRequest(e.Message);
        }
        catch (InvalidStreamOperationException e)
        {
            return Results.NotFound(e.Message);
        }
    }
}
