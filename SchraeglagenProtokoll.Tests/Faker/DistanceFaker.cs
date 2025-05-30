using Bogus;
using SchraeglagenProtokoll.Api.Rides;

namespace SchraeglagenProtokoll.Tests.Faker;

public sealed class DistanceFaker : Faker<Distance>
{
    public DistanceFaker()
    {
        CustomInstantiator(f => new Distance(
            f.Random.Double(0, 1000),
            f.Random.Enum<DistanceUnit>()
        ));
    }
}
