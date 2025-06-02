using SchraeglagenProtokoll.Api.Riders.Features;

namespace SchraeglagenProtokoll.Api.Riders;

public static class Endpoints
{
    public static void MapRider(this WebApplication app)
    {
        var group = app.MapGroup("/rider").WithTags("Rider");

        group.MapRegisterRider();
        group.MapLogRide();
        group.MapRenameRider();
        group.MapGetRiderById_ByAggregation();
        group.MapGetRiderById_FromProjection();
        group.MapGetRiderById_FromStreamedProjection();
        group.MapGetAllRider();
        group.MapGetRiderHistory();
        group.MapDeleteRiderById();
    }
}
