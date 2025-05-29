using SchraeglagenProtokoll.Api.Riders.Features;

namespace SchraeglagenProtokoll.Api.Riders;

public static class Endpoints
{
    public static void MapRider(this WebApplication app)
    {
        var group = app.MapGroup("/rider").WithTags("Rider");

        group.MapRegisterRider();
        group.MapRenameRider();
        group.MapGetRiderById();
        group.MapGetAllRider();
        group.MapDeleteRiderById();
    }
}
