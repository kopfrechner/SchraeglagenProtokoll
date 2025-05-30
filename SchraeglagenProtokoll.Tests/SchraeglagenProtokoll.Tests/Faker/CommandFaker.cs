using Bogus;
using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Features;

namespace SchraeglagenProtokoll.Tests.Faker;

public class CommandFaker
{
    public CommandFaker(int seed = 0815)
    {
        _distanceFaker = new DistanceFaker().UseSeed(seed);
        _logRideFaker = new Faker<LogRide.LogRideCommand>().UseSeed(seed + 1);
        _addCommentFaker = new Faker<AddComment.AddCommentCommand>().UseSeed(seed + 2);
        _registerRiderFaker = new Faker<RegisterRider.RegisterRiderCommand>().UseSeed(seed + 3);
        _renameRiderFaker = new Faker<RenameRider.RenameRiderCommand>().UseSeed(seed + 4);
    }

    private Faker<Distance> _distanceFaker;
    private Faker<LogRide.LogRideCommand> _logRideFaker;
    private Faker<AddComment.AddCommentCommand> _addCommentFaker;
    private Faker<RegisterRider.RegisterRiderCommand> _registerRiderFaker;
    private Faker<RenameRider.RenameRiderCommand> _renameRiderFaker;

    public LogRide.LogRideCommand LogRide(
        Guid? rideId = null,
        DateTimeOffset? date = null,
        string? startLocation = null,
        string? destination = null,
        Distance? distance = null
    )
    {
        return _logRideFaker
            .CustomInstantiator(f => new LogRide.LogRideCommand(
                rideId ?? f.Random.Guid(),
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
            .CustomInstantiator(f => new AddComment.AddCommentCommand(
                commentedById,
                f.Random.Words(20),
                version
            ))
            .Generate();
    }

    public RegisterRider.RegisterRiderCommand RegisterRider(
        Guid? riderId = null,
        string? email = null,
        string? fullName = null,
        string? roadName = null
    )
    {
        return _registerRiderFaker
            .CustomInstantiator(f => new RegisterRider.RegisterRiderCommand(
                riderId ?? f.Random.Guid(),
                email ?? f.Internet.Email(),
                fullName ?? f.Name.FullName(),
                roadName ?? f.PickRandom(FakedValues.RoadNames)
            ))
            .Generate();
    }

    public RenameRider.RenameRiderCommand RenameRider(int version, string? fullName = null)
    {
        return _renameRiderFaker
            .CustomInstantiator(f => new RenameRider.RenameRiderCommand(
                fullName ?? f.Person.FullName,
                version
            ))
            .Generate();
    }
}
