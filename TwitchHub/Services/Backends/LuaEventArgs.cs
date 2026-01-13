namespace TwitchHub.Services.Backends;

public class MediaAddedEventArgs : EventArgs
{
    public string Source { get; }
    public int QueuePosition { get; }

    public MediaAddedEventArgs(string source, int queuePosition)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        QueuePosition = queuePosition;
    }
}
public class MediaStartedEventArgs : EventArgs
{
    public string MediaItem { get; }
    public DateTime StartTime { get; }

    public MediaStartedEventArgs(string mediaItem)
    {
        MediaItem = mediaItem ?? string.Empty;
        StartTime = DateTime.UtcNow;
    }
}

public class MediaPausedEventArgs : EventArgs
{
    public string MediaItem { get; }
    public long CurrentTime { get; }
    public DateTime PauseTime { get; }

    public MediaPausedEventArgs(string mediaItem, long currentTime)
    {
        MediaItem = mediaItem ?? string.Empty;
        CurrentTime = currentTime;
        PauseTime = DateTime.UtcNow;
    }
}

public class MediaStoppedEventArgs : EventArgs
{
    public string MediaItem { get; }
    public DateTime StopTime { get; }

    public MediaStoppedEventArgs(string mediaItem)
    {
        MediaItem = mediaItem ?? string.Empty;
        StopTime = DateTime.UtcNow;
    }
}

public class MediaSkippedEventArgs : EventArgs
{
    public string SkippedItem { get; }
    public DateTime SkipTime { get; }

    public MediaSkippedEventArgs(string skippedItem)
    {
        SkippedItem = skippedItem ?? string.Empty;
        SkipTime = DateTime.UtcNow;
    }
}

public class MediaEndReachedEventArgs : EventArgs
{
    public string CompletedItem { get; }
    public DateTime CompletionTime { get; }

    public MediaEndReachedEventArgs(string completedItem)
    {
        CompletedItem = completedItem ?? string.Empty;
        CompletionTime = DateTime.UtcNow;
    }
}

public class QueueFinishedEventArgs : EventArgs
{
    public DateTime FinishTime { get; }

    public QueueFinishedEventArgs()
    {
        FinishTime = DateTime.UtcNow;
    }
}

public class MediaErrorEventArgs : EventArgs
{
    public string? MediaItem { get; }
    public Exception Exception { get; }
    public DateTime ErrorTime { get; }

    public MediaErrorEventArgs(string? mediaItem, Exception exception)
    {
        MediaItem = mediaItem;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        ErrorTime = DateTime.UtcNow;
    }
}