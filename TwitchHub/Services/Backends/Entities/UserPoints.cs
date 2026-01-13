namespace TwitchHub.Services.Backends.Entities;

public sealed class UserPoints
{
    public string UserId { get; set; } = string.Empty;
    public long Balance { get; set; }
}
