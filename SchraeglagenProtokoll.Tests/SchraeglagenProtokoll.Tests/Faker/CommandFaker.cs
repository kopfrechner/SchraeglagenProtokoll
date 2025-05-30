using Bogus;
using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Features;

namespace SchraeglagenProtokoll.Tests.Faker;

public class CommandFaker
{
    private static int Seed => 0815;

    private Faker<Distance> _distanceFaker = new DistanceFaker().UseSeed(Seed);
    private Faker<LogRide.LogRideCommand> _logRideFaker = new Faker<LogRide.LogRideCommand>().UseSeed(Seed + 1);
    private Faker<AddComment.AddCommentCommand> _addCommentFaker = new Faker<AddComment.AddCommentCommand>().UseSeed(Seed + 2);
    private Faker<RegisterRider.RegisterRiderCommand> _registerRiderFaker = new Faker<RegisterRider.RegisterRiderCommand>().UseSeed(Seed + 3);

    public LogRide.LogRideCommand LogRide(
        Guid? rideId = null,
        Guid? riderId = null,
        DateTimeOffset? date = null,
        string? startLocation = null,
        string? destination = null,
        Distance? distance = null
    )
    {
        return _logRideFaker
            .CustomInstantiator(f => new LogRide.LogRideCommand(
                rideId ?? f.Random.Guid(),
                riderId ?? f.Random.Guid(),
                date ?? f.Date.Recent(3),
                startLocation ?? f.Address.City(),
                destination ?? f.Address.City(),
                distance ?? _distanceFaker.Generate()
            ))
            .Generate();
    }

    public AddComment.AddCommentCommand AddComment(Guid commentedById, int version)
    {
        return _addCommentFaker
            .CustomInstantiator(f => new AddComment.AddCommentCommand(commentedById, f.Random.Words(20), version))
            .Generate();
    }

    public RegisterRider.RegisterRiderCommand RegisterRider(
        Guid? riderId = null,
        string? email = null,
        string? fullName = null,
        string? nerdAlias = null
    )
    {
        return _registerRiderFaker
            .CustomInstantiator(f => new RegisterRider.RegisterRiderCommand(
                riderId ?? f.Random.Guid(),
                email ?? f.Internet.Email(),
                fullName ?? f.Name.FullName(),
                nerdAlias ?? f.Hacker.Noun()
            ))
            .Generate();
    }
}
