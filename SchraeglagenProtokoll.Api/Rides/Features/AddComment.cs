using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Rides.Features;

public static class AddComment
{
    public static void MapAddComment(this RouteGroupBuilder group)
    {
        group.MapPost("{rideId}/comment", AddCommentHandler).WithName("AddComment").WithOpenApi();
    }

    public record AddCommentCommand(Guid CommentedBy, string Text);

    public static async Task<IResult> AddCommentHandler(
        IDocumentSession session,
        [FromRoute] Guid rideId,
        [FromBody] AddCommentCommand command
    )
    {
        var (commentedBy, text) = command;

        var commentAdded = new CommentAdded(commentedBy, text);
        session.Events.Append(rideId, commentAdded);
        await session.SaveChangesAsync();

        return Results.Ok();
    }
}
