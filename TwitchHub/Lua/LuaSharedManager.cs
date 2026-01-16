using Lua;
using Lua.Standard;
using System.Collections.Concurrent;
using TwitchHub.Lua.LuaLibs;
using TwitchHub.Lua.Services;

namespace TwitchHub.Lua;

public class LuaSharedManager : IDisposable, IHostedService
{
    private readonly ILogger<LuaSharedManager> _logger;
    private readonly LuaReactionsService _luaReactions;
    private readonly FileSystemWatcher _watcher;
    private readonly string _reactionsPath;
    private readonly LuaState _state;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = [];
    public LuaSharedManager(
        ILogger<LuaSharedManager> logger,
        IServiceProvider serviceProvider,
        IWebHostEnvironment env,
        LuaReactionsService luaReactions
        //LuaState state
        )
    {
        _logger = logger;
        _luaReactions = luaReactions;
        _state = LuaState.Create();

        _reactionsPath = Path.Combine(env.ContentRootPath, "configs", "reactions");
        _ = Directory.CreateDirectory(_reactionsPath);

        _state.Environment["hardwarelib"] = serviceProvider.GetRequiredService<LuaHardwareLib>();
        _state.Environment["loggerlib"] = serviceProvider.GetRequiredService<LuaLoggerLib>();
        _state.Environment["medialib"] = serviceProvider.GetRequiredService<LuaMediaLib>();
        _state.Environment["pointslib"] = serviceProvider.GetRequiredService<LuaPointsLib>();
        _state.Environment["storagelib"] = serviceProvider.GetRequiredService<LuaStorageLib>();
        _state.Environment["twitchlib"] = serviceProvider.GetRequiredService<LuaTwitchLib>();
        _state.Environment["utilslib"] = serviceProvider.GetRequiredService<LuaUtilsLib>();
        _state.OpenStandardLibraries();

        _watcher = new FileSystemWatcher(_reactionsPath, "*.lua")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = false
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileDeleted;
        _watcher.Renamed += OnFileRenamed;

    }

    // ================= IHostedService =================

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LuaSharedManager starting. Loading existing reactions...");

        var files = Directory.GetFiles(_reactionsPath, "*.lua");
        foreach (var file in files)
        {
            await ProcessFileAsync(file);
        }

        _watcher.EnableRaisingEvents = true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher.EnableRaisingEvents = false;
        return Task.CompletedTask;
    }

    // ================= File System Events =================

    private void OnFileChanged(object sender, FileSystemEventArgs e) => DebounceFileEvent(e.FullPath, () => ProcessFileAsync(e.FullPath));

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("File deleted: {Name}", e.Name);
        _luaReactions.RemoveReaction(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("File renamed: {OldName} -> {Name}", e.OldName, e.Name);
        _luaReactions.RemoveReaction(e.OldFullPath);
        DebounceFileEvent(e.FullPath, () => ProcessFileAsync(e.FullPath));
    }

    // ================= Logic =================

    private void DebounceFileEvent(string filePath, Func<Task> action)
    {
        if (_debounceTokens.TryRemove(filePath, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _debounceTokens[filePath] = cts;

        _ = Task.Delay(250, cts.Token).ContinueWith(async t =>
        {
            if (t.IsCanceled)
                return;

            try
            {
                await action();
            }
            finally
            {
                _ = _debounceTokens.TryRemove(filePath, out _);
                cts.Dispose();
            }
        });
    }

    private async Task ProcessFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var content = string.Empty;

        for (var i = 0; i < 3; i++)
        {
            try
            {
                content = await File.ReadAllTextAsync(filePath);
                break;
            }
            catch (IOException)
            {
                await Task.Delay(100);
            }
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("File is empty or could not be read: {filePath}", filePath);
            return;
        }

        try
        {
            var result = await _state.DoStringAsync(content);

            if (result.Length > 0 && result[0].Type == LuaValueType.Table)
            {
                var configTable = result[0].Read<LuaTable>();

                _luaReactions.UpdateReaction(filePath, configTable, _state);

                _logger.LogInformation("Successfully loaded reaction: {fileName}", Path.GetFileName(filePath));
            }
            else
            {
                _logger.LogWarning("File {filename} did not return a configuration table.", Path.GetFileName(filePath));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Lua script: {filePath}", filePath);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        foreach (var cts in _debounceTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
