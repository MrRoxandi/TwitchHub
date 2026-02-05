namespace TwitchHub.Configurations;

public sealed class TwitchConfiguration
{
    public const string SectionName = "Twitch";
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUrl { get; init; }
    public required string Channel { get; init; }

    public int ClipsPollingIntervalSeconds { get; init; } = 60;
}
