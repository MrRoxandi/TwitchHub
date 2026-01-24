using LibVLCSharp.Shared;
using System.Collections.Concurrent;
using TwitchHub.Configurations;

namespace TwitchHub.Services.LuaMedia;

public sealed class LuaMediaChannel : IDisposable
{
    public string Name { get; }

    private readonly LuaMediaChannelConfiguration _configuration;
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _player;
    private readonly ConcurrentQueue<Media> _queue = [];
    private readonly Lock _sync = new();
    private bool _disposed;

    // ================= EVENTS =================

    public event EventHandler<MediaAddedEventArgs>? OnAdded;
    public event EventHandler<MediaStartedEventArgs>? OnStarted;
    public event EventHandler<MediaSkippedEventArgs>? OnSkipped;
    public event EventHandler<MediaPausedEventArgs>? OnPaused; 
    public event EventHandler<MediaStoppedEventArgs>? OnStopped;
    public event EventHandler<MediaEndReachedEventArgs>? OnEndReached;
    public event EventHandler<QueueFinishedEventArgs>? OnQueueFinished;
    public event EventHandler<MediaErrorEventArgs>? OnError;

    // ================= STATE =================
    public string CurrentItem { get; private set; } = string.Empty;
    public bool IsPlaying => _player.IsPlaying;
    public bool IsPaused => _player.State == VLCState.Paused;
    public bool IsStopped => _player.State == VLCState.Stopped;
    public int QueueCount => _queue.Count;
    public bool PortEnabled => _configuration.PortEnabled;
    public string Stream => _configuration.Stream;
    public int Port => _configuration.Port;

    public int Volume
    {
        get => _player.Volume;
        set => SetVolume(value);
    }

    public float Speed
    {
        get => _player.Rate;
        set => SetSpeed(value);
    }

    // ================= INIT =================
    public LuaMediaChannel(string name, LuaMediaChannelConfiguration config, LibVLC libVLC)
    {
        Name = name;
        _configuration = config;
        _libVLC = libVLC;
        _player = new(_libVLC);
        HookEvents();
    }

    private void HookEvents()
    {
        _player.Playing += Player_Playing;
        _player.Paused += Player_Paused;
        _player.Stopped += Player_Stopped;
        _player.EndReached += Player_EndReached;
        _player.EncounteredError += Player_EncounteredError;
    }

    // ================= EVENT HANDLERS =================
    private void Player_Playing(object? sender, EventArgs e) => OnStarted?.Invoke(this, new MediaStartedEventArgs(Name, CurrentItem));

    private void Player_Paused(object? sender, EventArgs e) => OnPaused?.Invoke(this, new MediaPausedEventArgs(Name, CurrentItem, _player.Time));

    private void Player_Stopped(object? sender, EventArgs e) => OnStopped?.Invoke(this, new MediaStoppedEventArgs(Name, CurrentItem));

    private void Player_EndReached(object? sender, EventArgs e)
    {
        OnEndReached?.Invoke(this, new MediaEndReachedEventArgs(Name, CurrentItem));
        PlayNext();
    }

    private void Player_EncounteredError(object? sender, EventArgs e)
    {
        var error = new MediaErrorEventArgs(Name, CurrentItem, new InvalidOperationException("Media player error"));
        OnError?.Invoke(this, error);
        PlayNext();
    }
    // ================= QUEUE =================

    public void Add(string pathOrUrl)
    {
        ThrowIfDisposed();

        try
        {
            var media = CreateMedia(pathOrUrl);
            _queue.Enqueue(media);

            OnAdded?.Invoke(this, new MediaAddedEventArgs(Name, pathOrUrl, QueueCount));

            lock (_sync)
            {
                if (_player is { IsPlaying: false, CanPause: false })
                    PlayNext();
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, new MediaErrorEventArgs(Name, pathOrUrl, ex));
        }
    }

    // ================= PLAY =================

    public void Start()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            _ = _player.Play();
        }
    }

    public void Stop()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            _player.Stop();
        }
    }

    public void Pause()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            _player.Pause();
        }
    }

    public void Skip()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            var skippedItem = CurrentItem;
            _player.Stop();
            OnSkipped?.Invoke(this, new MediaSkippedEventArgs(Name, skippedItem));
            PlayNext();
        }
    }

    public void Resume()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            if (_player.CanPause)
                _ = _player.Play();
        }
    }

    // ================= PLAYBACK =================

    private void PlayNext()
    {
        lock (_sync)
        {
            if (_queue.TryDequeue(out var next))
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        _ = _player.Play(next);
                    }
                    catch (Exception ex)
                    {
                        CurrentItem = string.Empty;
                        OnError?.Invoke(this, new MediaErrorEventArgs(Name, next.Mrl, ex));
                        PlayNext();
                    }
                });
            }
            else
            {
                CurrentItem = string.Empty;
                OnQueueFinished?.Invoke(this, new QueueFinishedEventArgs(Name));
            }
        }
    }

    // ================= MEDIA FACTORY =================

    private Media CreateMedia(string source)
    {
        source = source.Trim();
        ArgumentException.ThrowIfNullOrEmpty(source);
        var web = Uri.IsWellFormedUriString(source, UriKind.Absolute);
        var media = web
            ? new Media(_libVLC, new Uri(source))
            : new Media(_libVLC, source);

        ConfigureOutputOptions(media);

        return media;
    }

    private void ConfigureOutputOptions(Media media)
    {
        var portEnabled = _configuration.PortEnabled;
        var keepOnSystem = _configuration.KeepOnSystem;

        switch (portEnabled)
        {
            case true when !keepOnSystem:
            {
                var soutOption = $":sout=#http{{mux=ts,dst=127.0.0.1:{_configuration.Port}/{_configuration.Stream}}}";
                media.AddOption(soutOption);
                media.AddOption(":sout-keep");
                break;
            }
            case true when keepOnSystem:
            {
                var httpOutput = $"http{{mux=ts,dst=127.0.0.1:{_configuration.Port}/{_configuration.Stream}}}";
                var soutOption = $":sout=#duplicate{{dst={httpOutput},dst=display}}";
                media.AddOption(soutOption);
                break;
            }
        }
    }

    // ================= SETTINGS =================

    public void SetVolume(int volume)
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            _player.Volume = Math.Clamp(volume, 0, 100);
        }
    }

    public int GetVolume()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            return _player.Volume;
        }
    }

    public void SetSpeed(float speed)
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            _ = _player.SetRate(Math.Clamp(speed, 0.25f, 4f));
        }
    }

    public float GetSpeed()
    {
        ThrowIfDisposed();
        lock (_sync)
        {
            return _player.Rate;
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

        lock (_sync)
        {
            _player.Playing -= Player_Playing;
            _player.Paused -= Player_Paused;
            _player.Stopped -= Player_Stopped;
            _player.EndReached -= Player_EndReached;
            _player.EncounteredError -= Player_EncounteredError;

            _player.Stop();
            _player.Dispose();
            _queue.Clear();
            _disposed = true;
        }
    }
}
