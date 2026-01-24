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
    private readonly LuaScriptsSerivce _luaScripts;
    private readonly FileSystemWatcher _reactionsWatcher;
    private readonly FileSystemWatcher _scriptsWatcher;
    private readonly string _reactionsPath;
    private readonly string _scriptsPath;
    private readonly LuaState _state;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = [];
    public LuaSharedManager(
        ILogger<LuaSharedManager> logger,
        IServiceProvider serviceProvider,
        IWebHostEnvironment env,
        LuaReactionsService luaReactions,
        LuaScriptsSerivce luaScripts
        )
    {
        _logger = logger;
        _luaReactions = luaReactions;
        _luaScripts = luaScripts;
        _state = LuaState.Create();

        _reactionsPath = Path.Combine(env.ContentRootPath, "configs", "reactions");
        _scriptsPath = Path.Combine(env.ContentRootPath, "configs", "scripts");
        _ = Directory.CreateDirectory(_reactionsPath);
        _ = Directory.CreateDirectory(_scriptsPath);

        _state.Environment["hardwarelib"] = serviceProvider.GetRequiredService<LuaHardwareLib>();
        _state.Environment["loggerlib"] = serviceProvider.GetRequiredService<LuaLoggerLib>();
        _state.Environment["medialib"] = serviceProvider.GetRequiredService<LuaMediaLib>();
        _state.Environment["pointslib"] = serviceProvider.GetRequiredService<LuaPointsLib>();
        _state.Environment["scriptlib"] = serviceProvider.GetRequiredService<LuaScriptLib>();
        _state.Environment["speechlib"] = serviceProvider.GetRequiredService<LuaSpeechLib>();
        _state.Environment["storagelib"] = serviceProvider.GetRequiredService<LuaStorageLib>();
        _state.Environment["twitchlib"] = serviceProvider.GetRequiredService<LuaTwitchLib>();
        _state.Environment["utilslib"] = serviceProvider.GetRequiredService<LuaUtilsLib>();
        _state.OpenStandardLibraries();

        _reactionsWatcher = new FileSystemWatcher(_reactionsPath, "*.lua")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = false
        };
        _scriptsWatcher = new FileSystemWatcher(_scriptsPath, "*.lua")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = false
        };

        _reactionsWatcher.Changed += OnReactionFileChanged;
        _reactionsWatcher.Created += OnReactionFileChanged;
        _reactionsWatcher.Deleted += OnReactionFileDeleted;
        _reactionsWatcher.Renamed += OnReactionFileRenamed;

        _scriptsWatcher.Created += OnScriptFileCreated;
        _scriptsWatcher.Deleted += OnScripFileDeleted;
        _scriptsWatcher.Renamed += OnScripFileRenamed;
    }

    // ================= IHostedService =================

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LuaSharedManager starting. Loading existing reactions...");

        var reactionFiles = Directory.GetFiles(_reactionsPath, "*.lua");
        foreach (var file in reactionFiles)
        {
            await ProcessFileAsync(file);
        }

        var scriptsFiles = Directory.GetFiles(_scriptsPath, "*.lua");
        foreach(var file in scriptsFiles)
        {
            _luaScripts.UpdateScript(file, _state);
        }

        _reactionsWatcher.EnableRaisingEvents = true;
        _scriptsWatcher.EnableRaisingEvents = true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _reactionsWatcher.EnableRaisingEvents = false;
        _scriptsWatcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    // ================= File System Events =================

    // ================= Reactions =================
    private void OnReactionFileChanged(object sender, FileSystemEventArgs e) => DebounceFileEvent(e.FullPath, () => ProcessFileAsync(e.FullPath));

    private void OnReactionFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("File deleted: {Name}", e.Name);
        _luaReactions.RemoveReaction(e.FullPath);
    }

    private void OnReactionFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("File renamed: {OldName} -> {Name}", e.OldName, e.Name);
        _luaReactions.RemoveReaction(e.OldFullPath);
        DebounceFileEvent(e.FullPath, () => ProcessFileAsync(e.FullPath));
    }

    // ================= Scripts =================

    private void OnScriptFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Script file created: {Name}", e.Name);
        _luaScripts.UpdateScript(e.FullPath, _state);
    }

    private void OnScripFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Script file deleted: {Name}", e.Name);
        _luaScripts.RemoveScript(e.FullPath);
    }

    private void OnScripFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("Scrip file was renamed: {OldName} -> {Name}", e.OldName, e.Name);
        _luaScripts.RemoveScript(e.OldFullPath);
        _luaScripts.UpdateScript(e.FullPath, _state);
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
        _reactionsWatcher.Dispose();
        foreach (var cts in _debounceTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
