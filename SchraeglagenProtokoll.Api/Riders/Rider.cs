using System.Text.Json.Serialization;

namespace SchraeglagenProtokoll.Api.Riders;

public interface IRiderEvent
{
    Guid RiderId { get; }
};

public record RiderRegistered(Guid RiderId, string Email, string FullName, string RoadName)
    : IRiderEvent;

public record RiderRenamed(Guid RiderId, string FullName) : IRiderEvent;

public record RiderDeletedAccount(Guid RiderId, string? RiderFeedback) : IRiderEvent;

public record Rider(Guid Id, string Email, string FullName, string RoadName)
{
    [JsonInclude]
    public int Version { get; private set; }

    public static Rider Create(RiderRegistered riderRegistered) =>
        new(
            riderRegistered.RiderId,
            riderRegistered.Email,
            riderRegistered.FullName,
            riderRegistered.RoadName
        );

    public Rider Apply(RiderRenamed riderRenamed) => this with { FullName = riderRenamed.FullName };

    private bool ShouldDelete(RiderDeletedAccount riderDeletedAccount) => true;
}
