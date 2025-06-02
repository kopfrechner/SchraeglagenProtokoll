using System.Text.Json.Serialization;

namespace SchraeglagenProtokoll.Api.Riders;

public interface IRiderEvent;

public record RiderRegistered(Guid Id, string Email, string FullName, string RoadName)
    : IRiderEvent;

public record RiderRenamed(string FullName) : IRiderEvent;

public record RiderDeletedAccount(string? RiderFeedback) : IRiderEvent;

public record Rider(Guid Id, string Email, string FullName, string RoadName)
{
    [JsonInclude]
    public int Version { get; private set; }

    public static Rider Create(RiderRegistered riderRegistered) =>
        new(
            riderRegistered.Id,
            riderRegistered.Email,
            riderRegistered.FullName,
            riderRegistered.RoadName
        );

    public Rider Apply(RiderRenamed riderRenamed) => this with { FullName = riderRenamed.FullName };

    private bool ShouldDelete(RiderDeletedAccount riderDeletedAccount) => true;
}
