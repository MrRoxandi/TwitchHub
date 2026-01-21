using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaStorageLib(
    ILogger<LuaStorageLib> logger,
    LuaDataContainer container
    )
{
    private readonly LuaDataContainer _container = container;
    private readonly ILogger<LuaStorageLib> _logger = logger;

    // ================= FILE OPERATIONS =================

    [LuaMember("load")]
    public async Task Load()
    {
        await _container.LoadAsync();
        _logger.LogDebug("Load: storage loaded");
    }

    [LuaMember("save")]
    public async Task Save()
    {
        await _container.SaveAsync();
        _logger.LogDebug("Save: storage saved");
    }

    [LuaMember("backup")]
    public async Task Backup(string? suffix = null)
    {
        await _container.BackupAsync(suffix);
        _logger.LogDebug("Backup: saved backup with suffix: {suffix}", suffix);
    }

    // ================= QUERY =================

    [LuaMember("contains")]
    public bool Contains(string key)
    {
        var result = _container.Contains(key);
        _logger.LogDebug("Contains: {key} -> {value}", key, result);
        return result;
    }

    [LuaMember("count")]
    public int Count() => _container.Count;

    [LuaMember("keys")]
    public LuaValue Keys()
    {
        var table = new LuaTable();
        var index = 1;

        foreach (var key in _container.Keys)
            table[index++] = LuaValue.FromObject(key);

        return table;
    }

    // ================= GET =================

    [LuaMember("get")]
    public LuaValue Get(string key)
    {
        var value = _container.Get<LuaValue>(key);
        _logger.LogDebug("Get: {key} -> {value}", key, value.ToString());
        return value;
    }

    // ================= SET =================

    [LuaMember("set")]
    public void Set(string key, LuaValue value)
    {
        _container.Set(key, value);
        _logger.LogDebug("Set: {key} -> {value}", key, value.ToString());
    }

    // ================= REMOVE & CLEAR =================

    [LuaMember("remove")]
    public bool Remove(string key)
    {
        var result = _container.Remove(key);
        _logger.LogDebug("Remove: {key} -> {result}", key, result);
        return result;
    }

    [LuaMember("clear")]
    public void Clear()
    {
        _container.Clear();
        _logger.LogDebug("Clear: storage cleared");
    }
}