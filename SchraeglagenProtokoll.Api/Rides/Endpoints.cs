using SchraeglagenProtokoll.Api.Rides.Features.Commands;
using SchraeglagenProtokoll.Api.Rides.Features.Queries;

namespace SchraeglagenProtokoll.Api.Rides;

public static class Endpoints
{
    public static void MapRide(this WebApplication app)
    {
        var group = app.MapGroup("/rides").WithTags("Rides");

        group.MapGetRideById();
        group.MapGetRideSummaryInfoById();
        group.MapGetAllRides();
        group.MapAddLocationTrack();
        group.MapFinishRide();
    }
}
