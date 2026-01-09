namespace TwitchHub.Configurations;

public sealed class TwitchConfig
{
    public static readonly string SectionName = "Twitch";
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUrl { get; init; }
}
