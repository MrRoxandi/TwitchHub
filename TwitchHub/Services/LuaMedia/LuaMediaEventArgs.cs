namespace TwitchHub.Services.LuaMedia;

public class MediaEventArgs(string channelName) : EventArgs
{
    public string ChannelName { get; } = channelName;
}

public class MediaAddedEventArgs(string channel, string source, int pos)
    : MediaEventArgs(channel)
{
    public string Source { get; } = source;
    public int QueuePosition { get; } = pos;
}

public class MediaStartedEventArgs(string channel, string item)
    : MediaEventArgs(channel)
{
    public string Source { get; } = item;
    public DateTime StartTime { get; } = DateTime.UtcNow;

}

public class MediaPausedEventArgs(string channel, string item, long movieTime)
    : MediaEventArgs(channel)
{
    public string Source { get; } = item;
    public long MovieTime { get; } = movieTime;
    public DateTime PauseTime { get; } = DateTime.UtcNow;
}

public class MediaStoppedEventArgs(string channel, string item)
    : MediaEventArgs(channel)
{
    public string Source { get; } = item;
    public DateTime StopTime { get; } = DateTime.UtcNow;
}

public class MediaSkippedEventArgs(string channel, string item)
    : MediaEventArgs(channel)
{
    public string Source { get; } = item;
    public DateTime SkipTime { get; } = DateTime.UtcNow;
}

public class MediaEndReachedEventArgs(string channel, string item)
    : MediaEventArgs(channel)
{
    public string Source { get; } = item;
    public DateTime EndTime { get; } = DateTime.UtcNow;
}

public class QueueFinishedEventArgs(string channel)
    : MediaEventArgs(channel)
{
    public DateTime FinishTime { get; } = DateTime.UtcNow;
}

public class MediaErrorEventArgs(string channel, string? item, Exception ex)
    : MediaEventArgs(channel)
{
    public string? Source { get; } = item;
    public Exception Exception { get; } = ex;
    public DateTime ErrorTime { get; } = DateTime.UtcNow;
}