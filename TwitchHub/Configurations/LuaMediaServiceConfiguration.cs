namespace TwitchHub.Configurations;

public class LuaMediaServiceConfiguration
{
    public const string SectionName = "MediaService";
    public bool KeepOnSystem { get; init; } = true;
    public string StreamName { get; init; } = "stream";
    public bool PortEnabled { get; init; } = false;
    public int Port { get; init; } = 8080;
}
