using Marten.Events.Projections;

namespace SchraeglagenProtokoll.Api.Rides.Projections;

public class ScorePerRider
{
    // Helping us out to identify the document
    public static readonly Guid DocumentIdentifier = Guid.Parse(
        "0957ddc5-74ac-49e5-8ef7-be9e80f0b640"
    );

    // Required by Marten
    public Guid Id { get; private set; } = DocumentIdentifier;
    public Dictionary<Guid, Distance> RiderTotalDistance { get; set; } = new();
}

public class ScorePerRiderProjection : MultiStreamProjection<ScorePerRider, Guid>
{
    public ScorePerRiderProjection()
    {
        Identity<RideLogged>(_ => ScorePerRider.DocumentIdentifier);
    }

    public void Apply(RideLogged e, ScorePerRider score)
    {
        if (!score.RiderTotalDistance.TryGetValue(e.RiderId, out var current))
        {
            score.RiderTotalDistance[e.RiderId] = e.Distance;
        }
        else
        {
            score.RiderTotalDistance[e.RiderId] = current + e.Distance;
        }
    }
}
