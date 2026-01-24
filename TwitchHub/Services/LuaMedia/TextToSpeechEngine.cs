using System.Runtime.Versioning;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using TwitchHub.Configurations;

namespace TwitchHub.Services.LuaMedia;

[SupportedOSPlatform("windows")]
public sealed class TextToSpeechEngine : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer;
    private readonly ILogger<TextToSpeechEngine> _logger;
    private readonly TextToSpeechEngineConfiguration _configuration;
    private readonly IReadOnlyCollection<InstalledVoice> _voices;
    
    private readonly Channel<string> _textQueue;
    private readonly Task _processingTask;
    private readonly CancellationTokenSource _cts;
    
    private readonly List<Regex> _bannedWordPatterns;
    
    private bool _disposed;
    private bool _isPaused;

    public bool IsSpeaking { get; private set; }

    public TextToSpeechEngine(
        ILogger<TextToSpeechEngine> logger,
        IOptions<TextToSpeechEngineConfiguration> config)
    {
        _logger = logger;
        _configuration = config.Value;
        _synthesizer = new SpeechSynthesizer();
        _voices = _synthesizer.GetInstalledVoices();
        _bannedWordPatterns = [];
        
        _textQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        
        _cts = new CancellationTokenSource();
        
        Initialize();
        LoadBannedWords();
        
        _processingTask = Task.Run(() => ProcessQueueAsync(_cts.Token));
    }

    private void Initialize()
    {
        try
        {
            _synthesizer.Volume = Math.Clamp(_configuration.Volume, 0, 100);
            _synthesizer.Rate = Math.Clamp(_configuration.Rate, -10, 10);

            if (!string.IsNullOrWhiteSpace(_configuration.Voice))
            {
                var voice = _voices.FirstOrDefault(v => 
                    v.VoiceInfo.Name.Equals(_configuration.Voice, StringComparison.OrdinalIgnoreCase));
                
                if (voice != null)
                {
                    _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                    _logger.LogInformation(
                        "TextToSpeechEngine initialized (Voice={Voice}, Volume={Volume}, Rate={Rate})",
                        voice.VoiceInfo.Name,
                        _synthesizer.Volume,
                        _synthesizer.Rate
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Voice '{Voice}' not found. Using default. Available: {Voices}",
                        _configuration.Voice,
                        string.Join(", ", _voices.Select(v => v.VoiceInfo.Name))
                    );
                }
            }
            else
            {
                _logger.LogInformation(
                    "TextToSpeechEngine initialized with default voice (Volume={Volume}, Rate={Rate})",
                    _synthesizer.Volume,
                    _synthesizer.Rate
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing TextToSpeechEngine");
            throw;
        }
    }

    private void LoadBannedWords()
    {
        if (string.IsNullOrWhiteSpace(_configuration.BannedWordsFilePath))
        {
            _logger.LogDebug("No banned words file specified");
            return;
        }

        if (!File.Exists(_configuration.BannedWordsFilePath))
        {
            _logger.LogWarning("Banned words file not found: {Path}", _configuration.BannedWordsFilePath);
            return;
        }

        try
        {
            var lines = File.ReadAllLines(_configuration.BannedWordsFilePath);
            var loadedCount = 0;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                    continue;

                try
                {
                    var regex = new Regex(trimmed, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
                    _bannedWordPatterns.Add(regex);
                    loadedCount++;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Invalid regex pattern in banned words file: {Pattern}", trimmed);
                }
            }

            _logger.LogInformation(
                "Loaded {Count} banned word patterns from {Path}",
                loadedCount,
                _configuration.BannedWordsFilePath
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading banned words file: {Path}", _configuration.BannedWordsFilePath);
        }
    }

    private string FilterBannedWords(string text)
    {
        if (_bannedWordPatterns.Count == 0)
            return text;

        var filtered = text;
        var replacedCount = 0;

        foreach (var pattern in _bannedWordPatterns)
        {
            var matches = pattern.Matches(filtered);
            if (matches.Count <= 0) continue;
            replacedCount += matches.Count;
            filtered = pattern.Replace(filtered, "*");
        }

        if (replacedCount > 0)
        {
            _logger.LogDebug("Filtered {Count} banned words from text", replacedCount);
        }

        return filtered;
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("TTS queue processing started");

        try
        {
            await foreach (var text in _textQueue.Reader.ReadAllAsync(cancellationToken))
            {
                if (_isPaused)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                try
                {
                    var filteredText = FilterBannedWords(text);
                    
                    if (string.IsNullOrWhiteSpace(filteredText))
                    {
                        _logger.LogDebug("Text filtered to empty, skipping");
                        continue;
                    }

                    IsSpeaking = true;
                    await SpeakInternalAsync(filteredText, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("TTS cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing TTS text");
                }
                finally
                {
                    IsSpeaking = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("TTS queue processing cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in TTS queue processing");
        }

        _logger.LogDebug("TTS queue processing stopped");
    }

    private Task SpeakInternalAsync(string text, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();

        try
        {
            _synthesizer.SpeakCompleted += OnCompleted;

            var registration = cancellationToken.Register(() =>
            {
                if (!_disposed)
                {
                    _synthesizer.SpeakAsyncCancelAll();
                }
            });

            _synthesizer.SpeakAsync(text);
            
            tcs.Task.ContinueWith(_ => registration.Dispose(), TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            _synthesizer.SpeakCompleted -= OnCompleted;
            _logger.LogError(ex, "Failed to start TTS");
            tcs.TrySetException(ex);
        }

        return tcs.Task;

        void OnCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            _synthesizer.SpeakCompleted -= OnCompleted;

            if (e.Error != null)
            {
                _logger.LogError(e.Error, "TTS completed with error");
                tcs.TrySetException(e.Error);
            }
            else if (e.Cancelled)
            {
                _logger.LogDebug("TTS was cancelled");
                tcs.TrySetCanceled();
            }
            else
            {
                tcs.TrySetResult();
            }
        }
    }

    // ================= PUBLIC API =================

    public void Enqueue(string text)
    {
        ThrowIfDisposed();

        if (!_configuration.Enabled || string.IsNullOrWhiteSpace(text))
            return;

        if (!_textQueue.Writer.TryWrite(text))
        {
            _logger.LogWarning("Failed to enqueue TTS text (queue closed)");
        }
        else
        {
            _logger.LogDebug("Text enqueued for TTS");
        }
    }

    public async Task EnqueueAsync(string text, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_configuration.Enabled || string.IsNullOrWhiteSpace(text))
            return;

        await _textQueue.Writer.WriteAsync(text, cancellationToken);
        _logger.LogDebug("Text enqueued for TTS");
    }

    public void Pause()
    {
        ThrowIfDisposed();
        _isPaused = true;
        _synthesizer.Pause();
        _logger.LogDebug("TTS paused");
    }

    public void Resume()
    {
        ThrowIfDisposed();
        _isPaused = false;
        _synthesizer.Resume();
        _logger.LogDebug("TTS resumed");
    }

    public void Stop()
    {
        ThrowIfDisposed();
        _synthesizer.SpeakAsyncCancelAll();
        _logger.LogDebug("TTS stopped");
    }

    public void ClearQueue()
    {
        ThrowIfDisposed();
        
        // Drain the queue
        while (_textQueue.Reader.TryRead(out _))
        {
            // Discard items
        }
        
        _logger.LogDebug("TTS queue cleared");
    }

    public void Skip()
    {
        ThrowIfDisposed();
        _synthesizer.SpeakAsyncCancelAll();
        _logger.LogDebug("Current TTS skipped");
    }

    // ================= SETTINGS =================

    public void SetVolume(int volume)
    {
        ThrowIfDisposed();
        _synthesizer.Volume = Math.Clamp(volume, 0, 100);
        _logger.LogDebug("TTS volume set to {Volume}", _synthesizer.Volume);
    }

    public int GetVolume()
    {
        ThrowIfDisposed();
        return _synthesizer.Volume;
    }

    public void SetRate(int rate)
    {
        ThrowIfDisposed();
        _synthesizer.Rate = Math.Clamp(rate, -10, 10);
        _logger.LogDebug("TTS rate set to {Rate}", _synthesizer.Rate);
    }

    public int GetRate()
    {
        ThrowIfDisposed();
        return _synthesizer.Rate;
    }

    public IEnumerable<string> GetAvailableVoices()
    {
        ThrowIfDisposed();
        return _voices.Select(v => v.VoiceInfo.Name);
    }

    public void SelectVoice(string voiceName)
    {
        ThrowIfDisposed();
        
        var voice = _voices.FirstOrDefault(v => 
            v.VoiceInfo.Name.Equals(voiceName, StringComparison.OrdinalIgnoreCase));
        
        if (voice == null)
        {
            throw new ArgumentException(
                $"Voice '{voiceName}' not found. Available: {string.Join(", ", GetAvailableVoices())}",
                nameof(voiceName)
            );
        }

        _synthesizer.SelectVoice(voice.VoiceInfo.Name);
        _logger.LogInformation("TTS voice changed to {Voice}", voice.VoiceInfo.Name);
    }

    public void ReloadBannedWords()
    {
        ThrowIfDisposed();
        _bannedWordPatterns.Clear();
        LoadBannedWords();
    }

    // ================= UTILITIES =================

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, nameof(TextToSpeechEngine));

    // ================= DISPOSE =================

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing TextToSpeechEngine");

        // Signal cancellation
        _cts.Cancel();
        
        // Complete the channel
        _textQueue.Writer.Complete();

        try
        {
            // Wait for processing to finish (with timeout)
            _processingTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error waiting for TTS processing task to complete");
        }

        try
        {
            _synthesizer.SpeakAsyncCancelAll();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cancelling speech during disposal");
        }

        _cts.Dispose();
        _synthesizer.Dispose();
        _disposed = true;

        _logger.LogDebug("TextToSpeechEngine disposed");
    }
}