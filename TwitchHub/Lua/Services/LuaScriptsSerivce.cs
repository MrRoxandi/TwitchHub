using Lua;
using System.Collections.Concurrent;

namespace TwitchHub.Lua.Services;

public sealed class LuaScriptsSerivce(ILogger<LuaScriptsSerivce> logger)
{
    private readonly ILogger<LuaScriptsSerivce> _logger = logger;
    private readonly ConcurrentDictionary<string, LuaScript> _scripts = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Keys => _scripts.Keys;

    public void RemoveScript(string filepath)
    {
        var name = Path.GetFileNameWithoutExtension(filepath);
        if (_scripts.TryRemove(name, out var _))
        {
            _logger.LogInformation("Successfylly removed script: {script}", name);
        }
    }

    public void UpdateScript(string filePath, LuaState state)
    {
        var script = new LuaScript(filePath, state);
        _scripts[script.Name] = script;
    }

    public LuaScript? Get(string key)
        => _scripts.TryGetValue(key, out var script) ? script : null;

    public bool Contains(string key) => _scripts.ContainsKey(key);
    public async Task CallAsync(string key)
    {
        try
        {
            var script = Get(key);
            if (script is not { })
            {
                _logger.LogWarning("Attepted to call not existing {name} script", key);
                return;
            }

            var result = await script.CallAsync();
            if (!result.Success)
            {
                _logger.LogInformation("Call to {name} script failed due: {message}", key, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call script: {name}", key);
        }
    }
}
