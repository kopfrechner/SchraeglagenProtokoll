using Bogus;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Faker;

public class EventFaker
{
    public EventFaker(int seed = 0815)
    {
        _distanceFaker = new DistanceFaker().UseSeed(seed);
        _rideLoggedFaker = new Faker<RideLogged>().UseSeed(seed + 1);
        _riderRenamedFaker = new Faker<RiderRenamed>().UseSeed(seed + 2);
        _riderRegisteredFaker = new Faker<RiderRegistered>().UseSeed(seed + 3);
        _commentAddedFaker = new Faker<CommentAdded>().UseSeed(seed + 4);
        _deleteRiderFaker = new Faker<RiderDeletedAccount>().UseSeed(seed + 5);
    }

    private readonly Faker<Distance> _distanceFaker;
    private readonly Faker<RideLogged> _rideLoggedFaker;
    private readonly Faker<RiderRenamed> _riderRenamedFaker;
    private readonly Faker<RiderRegistered> _riderRegisteredFaker;
    private readonly Faker<CommentAdded> _commentAddedFaker;
    private readonly Faker<RiderDeletedAccount> _deleteRiderFaker;

    public RideLogged RideLogged(
        Guid? rideId = null,
        Guid? riderId = null,
        DateTimeOffset? date = null,
        string? startLocation = null,
        string? destination = null,
        Distance? distance = null
    )
    {
        return _rideLoggedFaker
            .CustomInstantiator(f => new RideLogged(
                rideId ?? f.Random.Guid(),
                riderId ?? f.Random.Guid(),
                date ?? f.Date.Recent(3),
                startLocation ?? f.Address.City(),
                destination ?? f.Address.City(),
                distance ?? _distanceFaker.Generate()
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
