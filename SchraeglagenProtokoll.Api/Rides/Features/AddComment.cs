using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class AddComment
{
    public static void MapAddComment(this RouteGroupBuilder group)
    {
        group.MapPost("{rideId}/comment", AddCommentHandler).WithName("AddComment").WithOpenApi();
    }

    public record AddCommentCommand(Guid CommentedBy, string Text, int Version);

    public static async Task<IResult> AddCommentHandler(
        IDocumentSession session,
        [FromRoute] Guid rideId,
        [FromBody] AddCommentCommand command
    )
    {
        var (commentedBy, text, version) = command;

        var commentAdded = new CommentAdded(commentedBy, text);
        await session.Events.WriteToAggregate<Ride>(
            rideId,
            version,
            stream => stream.AppendOne(commentAdded)
        );
        await session.SaveChangesAsync();

        return Results.Ok();
    }
}
