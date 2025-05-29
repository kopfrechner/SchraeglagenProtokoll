using SchraeglagenProtokoll.Api.Rides.Features;

namespace SchraeglagenProtokoll.Api.Rides;

public static class Endpoints
{
    public static void MapRide(this WebApplication app)
    {
        var group = app.MapGroup("/rides").WithTags("Ride");

        group.MapLogRide();
        group.MapGetRideById();
        group.MapAddComment();
        group.MapGetScorePerRider();
    }
}
