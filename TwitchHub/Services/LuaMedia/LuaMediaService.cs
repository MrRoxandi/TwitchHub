using LibVLCSharp.Shared;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using TwitchHub.Configurations;

namespace TwitchHub.Services.LuaMedia;

public sealed class LuaMediaService : IDisposable
{
    private readonly LuaMediaServiceConfiguration _configuration;
    private readonly LibVLC _libVlc;

    private readonly ConcurrentDictionary<string, LuaMediaChannel> _channels 
        = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    // ================= STATES =================
    public IEnumerable<string> Channels => _channels.Keys;
    
    // ================= EVENTS =================

    public event EventHandler<MediaAddedEventArgs>? OnMediaAdded;
    public event EventHandler<MediaStartedEventArgs>? OnMediaStarted;
    public event EventHandler<MediaSkippedEventArgs>? OnMediaSkipped;
    public event EventHandler<MediaPausedEventArgs>? OnMediaPaused;
    public event EventHandler<MediaStoppedEventArgs>? OnMediaStopped;
    public event EventHandler<MediaEndReachedEventArgs>? OnMediaEndReached;
    public event EventHandler<QueueFinishedEventArgs>? OnQueueFinished;
    public event EventHandler<MediaErrorEventArgs>? OnError;

    // ================= INIT =================

    public LuaMediaService(IOptions<LuaMediaServiceConfiguration> options)
    {
        Core.Initialize();
        _configuration = options.Value;
        _libVlc = new LibVLC();
        InitializeChannels();
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

            channel.OnAdded += (s, e) => OnMediaAdded?.Invoke(this, e);
            channel.OnStarted += (s, e) => OnMediaStarted?.Invoke(this, e);
            channel.OnPaused += (s, e) => OnMediaPaused?.Invoke(this, e);
            channel.OnSkipped += (s, e) => OnMediaSkipped?.Invoke(this, e);
            channel.OnStopped += (s, e) => OnMediaStopped?.Invoke(this, e);
            channel.OnEndReached += (s, e) => OnMediaEndReached?.Invoke(this, e);
            channel.OnError += (s, e) => OnError?.Invoke(this, e);
            channel.OnQueueFinished += (s, e) => OnQueueFinished?.Invoke(this, e);
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
            OnError?.Invoke(this, new(channel, pathOrUrl, new ArgumentException($"Channel with name {channel} doesn't exist")));
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
            // OnError invoke
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
            // OnError invoke
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
            // OnError invoke
        }
    }

    public void Skip(string channel)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.Start();
        }
        else
        {
            // OnError invoke
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
            // OnError invoke
        }
    }

    
    // ================= SETTINGS =================

    public void SetVolume(string channel, int volume)
    {
        ThrowIfDisposed();
        if (_channels.TryGetValue(channel, out var ch))
        {
            ch.SetVolume(volume);
        }
        else
        {
            // OnError invoke
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
            // OnError invoke
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
            // OnError invoke
            
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
            // OnError invoke
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
            return;
        foreach(var ch in _channels.Values)
        {
            ch.Dispose();
        }
        _channels.Clear();
        _libVlc.Dispose();
        _disposed = true;
    }
}
