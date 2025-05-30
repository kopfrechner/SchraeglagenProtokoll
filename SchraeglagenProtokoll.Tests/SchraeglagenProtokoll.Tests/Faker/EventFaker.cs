using Bogus;
using SchraeglagenProtokoll.Api.Riders;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Faker;

public class EventFaker
{
    private static int Seed => 0815;

    private Faker<Distance> _distanceFaker = new DistanceFaker().UseSeed(Seed);

    private Faker<RideLogged> _rideLoggedFaker = new Faker<RideLogged>().UseSeed(Seed + 1);

    private Faker<RiderRenamed> _riderRenamedFaker = new Faker<RiderRenamed>().UseSeed(Seed + 2);

    private Faker<RiderRegistered> _riderRegisteredFaker = new Faker<RiderRegistered>().UseSeed(
        Seed + 3
    );

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
        string? nerdAlias = null
    )
    {
        return _riderRegisteredFaker
            .CustomInstantiator(f => new RiderRegistered(
                id ?? f.Random.Guid(),
                email ?? f.Internet.Email(),
                fullName ?? f.Name.FullName(),
                nerdAlias ?? f.Internet.UserName()
            ))
            .Generate();
    }
}
