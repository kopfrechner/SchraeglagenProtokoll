using Bogus;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Riders.Projections;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Faker;

public class EventFaker
{
    public EventFaker(int seed = 0815)
    {
        _distanceFaker = new DistanceFaker().UseSeed(seed);
        _riderRenamedFaker = new Faker<RiderRenamed>().UseSeed(seed + 2);
        _riderRegisteredFaker = new Faker<RiderRegistered>().UseSeed(seed + 3);
        _commentAddedFaker = new Faker<CommentAdded>().UseSeed(seed + 4);
        _deleteRiderFaker = new Faker<RiderDeletedAccount>().UseSeed(seed + 5);
        _rideStartedFaker = new Faker<RideStarted>().UseSeed(seed + 1);
    }

    private readonly Faker<Distance> _distanceFaker;
    private readonly Faker<RideStarted> _rideStartedFaker;
    private readonly Faker<RiderRenamed> _riderRenamedFaker;
    private readonly Faker<RiderRegistered> _riderRegisteredFaker;
    private readonly Faker<CommentAdded> _commentAddedFaker;
    private readonly Faker<RiderDeletedAccount> _deleteRiderFaker;

    public RideStarted RideStarted(
        Guid? rideId = null,
        Guid? riderId = null,
        string? startLocation = null
    )
    {
        return _rideStartedFaker
            .CustomInstantiator(f => new RideStarted(
                rideId ?? f.Random.Guid(),
                riderId ?? f.Random.Guid(),
                startLocation ?? f.Address.City()
            ))
            .Generate();
    }

    public RiderRenamed RiderRenamed(string? fullName = null)
    {
        return _riderRenamedFaker
            .CustomInstantiator(f => new RiderRenamed(fullName ?? f.Name.FullName()))
            .Generate();
    }

    public RiderRegistered RiderRegistered(
        Guid? id = null,
        string? email = null,
        string? fullName = null,
        string? roadName = null
    )
    {
        return _riderRegisteredFaker
            .CustomInstantiator(f => new RiderRegistered(
                id ?? f.Random.Guid(),
                email ?? f.Internet.Email(),
                fullName ?? f.Name.FullName(),
                roadName ?? f.PickRandom(FakedValues.RoadNames)
            ))
            .Generate();
    }

    public CommentAdded CommentAdded(Guid? commentedBy = null, string? text = null)
    {
        return _commentAddedFaker
            .CustomInstantiator(f => new CommentAdded(
                commentedBy ?? f.Random.Guid(),
                text ?? f.Lorem.Sentence()
            ))
            .Generate();
    }

    public RiderDeletedAccount RiderDeletedAccount(string? riderFeedback = null)
    {
        return _deleteRiderFaker
            .CustomInstantiator(f => new RiderDeletedAccount(riderFeedback ?? f.Lorem.Sentence()))
            .Generate();
    }
}
