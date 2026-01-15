namespace TwitchHub.Configurations;

public class LuaMediaServiceConfiguration
{
    public const string SectionName = "MediaService";
    public Dictionary<string, LuaMediaChannelConfiguration> Channels { get; init; } = [];
}
