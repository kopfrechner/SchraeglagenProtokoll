using Bogus;
using SchraeglagenProtokoll.Api.Riders.Features;
using SchraeglagenProtokoll.Api.Riders.Features.Commands;
using SchraeglagenProtokoll.Api.Rides;
using SchraeglagenProtokoll.Api.Rides.Features;
using SchraeglagenProtokoll.Api.Rides.Features.Commands;

namespace SchraeglagenProtokoll.Tests.Faker;

public class CommandFaker
{
    public CommandFaker(int seed = 0815)
    {
        _distanceFaker = new DistanceFaker().UseSeed(seed);
        _registerRiderFaker = new Faker<RegisterRider.RegisterRiderCommand>().UseSeed(seed + 3);
        _renameRiderFaker = new Faker<RenameRider.RenameRiderCommand>().UseSeed(seed + 4);
        _startRideFaker = new Faker<StartRide.StartRideCommand>().UseSeed(seed + 2);
        _addLocationTrackFaker = new Faker<AddLocationTrack.AddLocationTrackCommand>().UseSeed(
            seed + 5
        );
        _finishRideFaker = new Faker<FinishRide.FinishRideCommand>().UseSeed(seed + 6);
    }

    private Faker<Distance> _distanceFaker;
    private Faker<RegisterRider.RegisterRiderCommand> _registerRiderFaker;
    private Faker<RenameRider.RenameRiderCommand> _renameRiderFaker;
    private Faker<StartRide.StartRideCommand> _startRideFaker;
    private Faker<AddLocationTrack.AddLocationTrackCommand> _addLocationTrackFaker;
    private Faker<FinishRide.FinishRideCommand> _finishRideFaker;

    public StartRide.StartRideCommand StartRide(Guid? rideId = null, string? startLocation = null)
    {
        return _startRideFaker
            .CustomInstantiator(f => new StartRide.StartRideCommand(
                rideId ?? f.Random.Guid(),
                startLocation ?? f.Address.City()
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

    public AddLocationTrack.AddLocationTrackCommand AddLocationTrack(
        int version,
        string? location = null
    )
    {
        return _addLocationTrackFaker
            .CustomInstantiator(f => new AddLocationTrack.AddLocationTrackCommand(
                location ?? f.Address.City(),
                version
            ))
            .Generate();
    }

    public FinishRide.FinishRideCommand FinishRide(
        int version,
        string? destination = null,
        Distance? distance = null
    )
    {
        return _finishRideFaker
            .CustomInstantiator(f => new FinishRide.FinishRideCommand(
                destination ?? f.Address.City(),
                distance ?? _distanceFaker.Generate(),
                version
            ))
            .Generate();
    }
}
