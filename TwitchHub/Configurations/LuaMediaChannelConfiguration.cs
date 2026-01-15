namespace TwitchHub.Configurations;

public sealed class LuaMediaChannelConfiguration
{
    public bool PortEnabled { get; init; } = false;
    public bool KeepOnSystem { get; init; } = true;
    public int Port { get; init; } = 8000;
    public string Stream { get; init; } = "stream";

}
