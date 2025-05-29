using Marten;
using Microsoft.AspNetCore.Mvc;

namespace SchraeglagenProtokoll.Api.Riders.Features;

public static class RenameRiderEndpoint
{
    public static void MapRenameRider(this RouteGroupBuilder group)
    {
        group.MapPost("{id:guid}/rename", RenameRiderHandler).WithName("RenameRider").WithOpenApi();
    }
    
    public record RenameRider(string FullName);
    
    public static async Task<IResult> RenameRiderHandler(
        IDocumentSession session,
        [FromRoute] Guid id, 
        RenameRider command
    )
    {
        var newFullName = command.FullName;
        var riderRenamed = new RiderRenamed(newFullName);
        await session.Events.WriteToAggregate<Rider>(id, stream => stream.AppendOne(riderRenamed));
        return Results.Ok();
    }
}
