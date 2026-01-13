using LibVLCSharp.Shared;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using TwitchHub.Configurations;

namespace TwitchHub.Services.Backends;

public sealed class LuaMediaService : IDisposable
{
    private readonly LuaMediaServiceConfiguration _configuration;

    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _player;
    private readonly ConcurrentQueue<Media> _mediaList;
    private readonly Lock _sync;
    private bool _disposed;

    // ================= STATE =================
    public string CurrentItem { get; private set; } = string.Empty;
    public bool IsPlaying => _player.IsPlaying;
    public bool IsPaused => _player.State == VLCState.Paused;
    public bool IsStopped => _player.State == VLCState.Stopped;
    public int QueueCount => _mediaList.Count;
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

    // ================= EVENTS =================
    public event EventHandler<MediaAddedEventArgs>? OnMediaAdded;
    public event EventHandler<MediaStartedEventArgs>? OnMediaStarted;
    public event EventHandler<MediaSkippedEventArgs>? OnMediaSkipped;
    public event EventHandler<MediaPausedEventArgs>? OnMediaPaused;
    public event EventHandler<MediaStoppedEventArgs>? OnMediaStopped;
    public event EventHandler<MediaEndReachedEventArgs>? OnMediaEndReached;
    public event EventHandler<QueueFinishedEventArgs>? QueueFinished;
    public event EventHandler<MediaErrorEventArgs>? OnError;

    // ================= INIT =================

    public LuaMediaService(IOptions<LuaMediaServiceConfiguration> options)
    {
        Core.Initialize();
        _configuration = options.Value;
        _libVlc = new LibVLC();
        _player = new MediaPlayer(_libVlc);
        _mediaList = [];
        _sync = new();
        _disposed = false;
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
    private void Player_Playing(object? sender, EventArgs e) => OnMediaStarted?.Invoke(this, new MediaStartedEventArgs(CurrentItem));

    private void Player_Paused(object? sender, EventArgs e) => OnMediaPaused?.Invoke(this, new MediaPausedEventArgs(CurrentItem, _player.Time));

    private void Player_Stopped(object? sender, EventArgs e) => OnMediaStopped?.Invoke(this, new MediaStoppedEventArgs(CurrentItem));

    private void Player_EndReached(object? sender, EventArgs e)
    {
        OnMediaEndReached?.Invoke(this, new MediaEndReachedEventArgs(CurrentItem));
        PlayNext();
    }

    private void Player_EncounteredError(object? sender, EventArgs e)
    {
        var error = new MediaErrorEventArgs(CurrentItem, new InvalidOperationException("Media player error"));
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
            _mediaList.Enqueue(media);

            OnMediaAdded?.Invoke(this, new MediaAddedEventArgs(pathOrUrl, QueueCount));

            lock (_sync)
            {
                if (!_player.IsPlaying && !_player.CanPause)
                    PlayNext();
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, new MediaErrorEventArgs(pathOrUrl, ex));
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
            OnMediaSkipped?.Invoke(this, new MediaSkippedEventArgs(CurrentItem));
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
            if (_mediaList.TryDequeue(out var next))
            {
                try
                {
                    CurrentItem = next.Mrl;
                    _ = _player.Play(next);
                }
                catch (Exception ex)
                {
                    CurrentItem = string.Empty;
                    OnError?.Invoke(this, new MediaErrorEventArgs(next.Mrl, ex));
                    PlayNext();
                }
            }
            else
            {
                CurrentItem = string.Empty;
                QueueFinished?.Invoke(this, new QueueFinishedEventArgs());
            }
        }
    }

    // ================= MEDIA FACTORY =================

    private Media CreateMedia(string source)
    {
        source = source.Trim();
        ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));
        var web = Uri.IsWellFormedUriString(source, UriKind.Absolute);
        var media = web
            ? new Media(_libVlc, new Uri(source))
            : new Media(_libVlc, source, FromType.FromPath);
        
        ConfigureOutputOptions(media);

        return media;
    }

    private void ConfigureOutputOptions(Media media)
    {
        var portEnabled = _configuration.PortEnabled;
        var keepOnSystem = _configuration.KeepOnSystem;

        if (portEnabled && !keepOnSystem)
        {
            var soutOption = $":sout=#http{{mux=ts,dst=127.0.0.1:{_configuration.Port}/{_configuration.StreamName}}}";
            media.AddOption(soutOption);
            media.AddOption(":sout-keep");
        }
        else if (portEnabled && keepOnSystem)
        {
            var httpOutput = $"http{{mux=ts,dst=127.0.0.1:{_configuration.Port}/{_configuration.StreamName}}}";
            var soutOption = $":sout=#duplicate{{dst={httpOutput},dst=display}}";
            media.AddOption(soutOption);
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
            _libVlc.Dispose();
            _mediaList.Clear();
            _disposed = true;
        }
    }
}
