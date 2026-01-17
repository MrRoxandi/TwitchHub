namespace TwitchHub.Configurations;

public sealed class TwitchConfiguration
{
    public const string SectionName = "Twitch";
    public required string ClientId { get; init; } = "hd9kavndkos83ujswrqhuffa90kcb6";
    public required string ClientSecret { get; init; } = "3kjlzgpn4mqwmkpyesua03o26ddqtw";
    public required string RedirectUrl { get; init; }
    public required string Channel { get; init; }

    public int ClipsPollingIntervalSeconds { get; init; } = 60;
}
