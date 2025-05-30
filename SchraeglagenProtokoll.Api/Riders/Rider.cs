namespace SchraeglagenProtokoll.Api.Riders;

public record RiderRegistered(Guid Id, string Email, string FullName, string NerdAlias);

public record RiderRenamed(string FullName);

public record RiderDeletedAccount(string? RiderFeedback);

public record Rider(Guid Id, string Email, string FullName, string NerdAlias)
{
    public int Version { get; private set; }

    public static Rider Create(RiderRegistered riderRegistered) =>
        new Rider(
            riderRegistered.Id,
            riderRegistered.Email,
            riderRegistered.FullName,
            riderRegistered.NerdAlias
        );

    public Rider Apply(RiderRenamed riderRenamed) => this with { FullName = riderRenamed.FullName };

    internal bool ShouldDelete(RiderDeletedAccount riderDeletedAccount) => true;
}
