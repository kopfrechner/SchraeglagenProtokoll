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
        _addCommentFaker = new Faker<AddComment.AddCommentCommand>().UseSeed(seed + 2);
        _registerRiderFaker = new Faker<RegisterRider.RegisterRiderCommand>().UseSeed(seed + 3);
        _renameRiderFaker = new Faker<RenameRider.RenameRiderCommand>().UseSeed(seed + 4);
        _startRideFaker = new Faker<LogRide.StartRideCommand>().UseSeed(seed + 1);
    }

    private Faker<Distance> _distanceFaker;
    private Faker<AddComment.AddCommentCommand> _addCommentFaker;
    private Faker<RegisterRider.RegisterRiderCommand> _registerRiderFaker;
    private Faker<RenameRider.RenameRiderCommand> _renameRiderFaker;
    private Faker<LogRide.StartRideCommand> _startRideFaker;

    public LogRide.StartRideCommand StartRide(Guid? rideId = null, string? startLocation = null)
    {
        return _startRideFaker
            .CustomInstantiator(f => new LogRide.StartRideCommand(
                rideId ?? f.Random.Guid(),
                startLocation ?? f.Address.City()
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
