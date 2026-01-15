namespace TwitchHub.Services.Backends.Entities;

public sealed class TwitchClipEntity
{
    public long Id { get; set; }
    public string ClipId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Title { get; set; } = string.Empty;

}
