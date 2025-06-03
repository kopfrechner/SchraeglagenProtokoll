using Bogus;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Faker;

public class EventFaker
{
    public EventFaker(int seed = 0815)
    {
        _distanceFaker = new DistanceFaker().UseSeed(seed);
        _riderRenamedFaker = new Faker<RiderRenamed>().UseSeed(seed + 2);
        _riderRegisteredFaker = new Faker<RiderRegistered>().UseSeed(seed + 3);
        _deleteRiderFaker = new Faker<RiderDeletedAccount>().UseSeed(seed + 5);
        _rideStartedFaker = new Faker<RideStarted>().UseSeed(seed + 1);
        _rideLocationTrackedFaker = new Faker<RideLocationTracked>().UseSeed(seed + 4);
        _rideFinishedFaker = new Faker<RideFinished>().UseSeed(seed + 6);
    }

    private readonly Faker<Distance> _distanceFaker;
    private readonly Faker<RiderRenamed> _riderRenamedFaker;
    private readonly Faker<RiderRegistered> _riderRegisteredFaker;
    private readonly Faker<RiderDeletedAccount> _deleteRiderFaker;
    private readonly Faker<RideStarted> _rideStartedFaker;
    private readonly Faker<RideLocationTracked> _rideLocationTrackedFaker;
    private readonly Faker<RideFinished> _rideFinishedFaker;

    public RiderRegistered RiderRegistered(
        Guid riderId,
        string? email = null,
        string? fullName = null,
        string? roadName = null
    )
    {
        return _riderRegisteredFaker
            .CustomInstantiator(f => new RiderRegistered(
                riderId,
                email ?? f.Internet.Email(),
                fullName ?? f.Name.FullName(),
                roadName ?? f.PickRandom(FakedValues.RoadNames)
            ))
            .Generate();
    }

    public RiderRenamed RiderRenamed(Guid riderId, string? fullName = null)
    {
        return _riderRenamedFaker
            .CustomInstantiator(f => new RiderRenamed(riderId, fullName ?? f.Name.FullName()))
            .Generate();
    }

    public RiderDeletedAccount RiderDeletedAccount(Guid riderId, string? riderFeedback = null)
    {
        return _deleteRiderFaker
            .CustomInstantiator(f => new RiderDeletedAccount(
                riderId,
                riderFeedback ?? f.Lorem.Sentence()
            ))
            .Generate();
    }

    public RideStarted RideStarted(Guid rideId, Guid? riderId = null, string? startLocation = null)
    {
        return _rideStartedFaker
            .CustomInstantiator(f => new RideStarted(
                rideId,
                riderId ?? f.Random.Guid(),
                startLocation ?? f.Address.City()
            ))
            .Generate();
    }

    public RideLocationTracked RideLocationTracked(Guid rideId, string? location = null)
    {
        return _rideLocationTrackedFaker
            .CustomInstantiator(f => new RideLocationTracked(rideId, location ?? f.Address.City()))
            .Generate();
    }

    public RideFinished RideFinished(Guid rideId, string? destination = null)
    {
        return _rideFinishedFaker
            .CustomInstantiator(f => new RideFinished(
                rideId,
                destination ?? f.Address.City(),
                _distanceFaker.Generate()
            ))
            .Generate();
    }
}
