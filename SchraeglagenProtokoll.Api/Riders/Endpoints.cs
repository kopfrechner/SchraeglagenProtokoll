using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Rides.Features;

namespace SchraeglagenProtokoll.Api.Riders;

public static class Endpoints
{
    public static void MapRider(this WebApplication app)
    {
        var group = app.MapGroup("/rider").WithTags("Rider");

        group.MapRegisterRider();
        group.MapLogRide();
        group.MapRenameRider();
        group.MapGetRiderById();
        group.MapGetAllRider();
        group.MapGetRiderStats();
        group.MapDeleteRiderById();
    }
}
