using LibVLCSharp.Shared;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using TwitchHub.Configurations;
using TwitchHub.Lua.Services;

namespace TwitchHub.Services.LuaMedia;

public sealed class LuaMediaService : IDisposable
{
    private readonly LuaMediaServiceConfiguration _configuration;
    private readonly LuaReactionsService _luaReactions;
    private readonly LibVLC _libVlc;

    private readonly ConcurrentDictionary<string, LuaMediaChannel> _channels
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<LuaMediaService> _logger;

    // ================= STATES =================

    private bool _disposed;
    public IEnumerable<LuaMediaChannel> Channels => _channels.Values;

    // ================= INIT =================

    public LuaMediaService(
        IOptions<LuaMediaServiceConfiguration> options,
        ILogger<LuaMediaService> logger,
        LuaReactionsService luaReactions)
    {
        Core.Initialize();
        _configuration = options.Value;
        _libVlc = new LibVLC("--http-host=localhost", "--quiet");
        _logger = logger;
        _luaReactions = luaReactions;
        _libVlc.Log += OnLibVlcLog;
        InitializeChannels();
    }

    private void OnLibVlcLog(object? sender, LogEventArgs e)
    {
        var level = e.Level;
        var mappedLevel = level switch
        {
            LibVLCSharp.Shared.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LibVLCSharp.Shared.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            _ => Microsoft.Extensions.Logging.LogLevel.Debug,
        };
        _logger.Log(mappedLevel, "{message}", e.Message);
    }

    private void InitializeChannels()
    {
        // If in configuration no channels presented add a default "Main" one
        if (_configuration.Channels.Count == 0)
        {
            _configuration.Channels.Add("Main", new LuaMediaChannelConfiguration());
        }

        foreach (var (name, config) in _configuration.Channels)
        {
            var channel = new LuaMediaChannel(name, config, _libVlc);

            channel.OnAdded += async (s, e) => await OnMediaAdded(s, e);
            channel.OnStarted += async (s, e) => await OnMediaStarted(s, e);
            channel.OnPaused += async (s, e) => await OnMediaPaused(s, e);
            channel.OnSkipped += async (s, e) => await OnMediaSkipped(s, e);
            channel.OnStopped += async (s, e) => await OnMediaStopped(s, e);
            channel.OnEndReached += async (s, e) => await OnMediaEndReached(s, e);
            channel.OnError += async (s, e) => await OnError(s, e);
            channel.OnQueueFinished += async (s, e) => await OnQueueFinished(s, e);
            _ = _channels.TryAdd(name, channel);
        }
    }

    // ================= QUEUE =================

    public void Add(string channel, string pathOrUrl)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Add(pathOrUrl);
        }
        else
        {
            _ = OnError(this, new(channel, pathOrUrl, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    // ================= PLAY =================

    public void Start(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Start();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public void Stop(string channel)
    {

        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Stop();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public void Pause(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Pause();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public void Skip(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Skip();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public void Resume(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Resume();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    // ================= SETTINGS =================

    public bool IsPaused(string channel) => _channels.TryGetValue(channel, out var mediachannel) && mediachannel.IsPaused;
    public bool IsPlaying(string channel) => _channels.TryGetValue(channel, out var mediachannel) && mediachannel.IsPlaying;
    public bool IsStopped(string channel) => _channels.TryGetValue(channel, out var mediachannel) && mediachannel.IsStopped;

    public void SetVolume(string channel, int volume)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.SetVolume(volume);
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public int GetVolume(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            return ch.GetVolume();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
            return -1;
        }
    }

    public void SetSpeed(string channel, float speed)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.SetSpeed(speed);
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
        }
    }

    public float GetSpeed(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            return ch.GetSpeed();
        }
        else
        {
            _ = OnError(this, new(channel, null, new ArgumentException($"Channel with name {channel} doesn't exist")));
            return -1.0f;
        }
    }

    // ================= UTILITIES =================
    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, nameof(LuaMediaService));

    // ================= DISPOSE =================
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var ch in _channels.Values)
        {
            ch.Dispose();
        }

        _channels.Clear();
        _libVlc.Dispose();
        _libVlc.Log -= OnLibVlcLog;
        _disposed = true;
    }

    // ================= EVENTS HANDLERS =================

    public async Task OnMediaAdded(object? sender, MediaAddedEventArgs args)
    {
        _logger.LogDebug("Media added to channel {Channel}: {Source} at position {position}", args.ChannelName, args.Source, args.QueuePosition);
        await _luaReactions.CallAsync(LuaReactionKind.MediaAdd, args.ChannelName, args.Source, args.QueuePosition);
    }
    public async Task OnMediaStarted(object? sender, MediaStartedEventArgs args)
    {
        _logger.LogDebug("Media started in channel {Channel}: {Source} at {starttime}", args.ChannelName, args.Source, args.StartTime);
        await _luaReactions.CallAsync(LuaReactionKind.MediaStart, args.ChannelName, args.Source, args.StartTime.Ticks);
    }
    public async Task OnMediaSkipped(object? sender, MediaSkippedEventArgs args)
    {
        _logger.LogDebug("Media skipped in channel {Channel}: {Source} at {skiptime}", args.ChannelName, args.Source, args.SkipTime);
        await _luaReactions.CallAsync(LuaReactionKind.MediaSkip, args.ChannelName, args.Source, args.SkipTime.Ticks);
    }
    public async Task OnMediaPaused(object? sender, MediaPausedEventArgs args)
    {
        _logger.LogDebug("Media paused in channel {Channel}: {Source} at {pausetime}", args.ChannelName, args.Source, args.PauseTime);
        await _luaReactions.CallAsync(LuaReactionKind.MediaPause, args.ChannelName, args.Source, args.PauseTime.Ticks);
    }
    public async Task OnMediaStopped(object? sender, MediaStoppedEventArgs args)
    {
        _logger.LogDebug("Media stopped in channel {Channel}: {Source} at {stoptime}", args.ChannelName, args.Source, args.StopTime);
        await _luaReactions.CallAsync(LuaReactionKind.MediaStop, args.ChannelName, args.Source, args.StopTime.Ticks);
    }
    public async Task OnMediaEndReached(object? sender, MediaEndReachedEventArgs args)
    {
        _logger.LogDebug("Media ended in channel {Channel}: {Source} at {endtime}", args.ChannelName, args.Source, args.EndTime);
        await _luaReactions.CallAsync(LuaReactionKind.MediaEnd, args.ChannelName, args.Source, args.EndTime.Ticks);
    }
    public async Task OnQueueFinished(object? sender, QueueFinishedEventArgs args)
    {
        _logger.LogDebug("Queue finished in channel {Channel}", args.ChannelName);
        await _luaReactions.CallAsync(LuaReactionKind.MediaQueueFinish, args.ChannelName);
    }
    public Task OnError(object? sender, MediaErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in channel {Channel} for source {Source} at {ErrorTime}", args.ChannelName, args.Source, args.ErrorTime);
        return Task.CompletedTask;
    }
}
