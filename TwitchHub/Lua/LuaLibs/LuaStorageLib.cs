using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaStorageLib(LuaDataContainer container)
{
    private readonly LuaDataContainer _container = container;

    // ================= FILE OPERATIONS =================

    [LuaMember("load")]
    public async Task Load() => await _container.LoadAsync();

    [LuaMember("save")]
    public async Task Save() => await _container.SaveAsync();

    [LuaMember("backup")]
    public async Task Backup(string? suffix = null) => await _container.BackupAsync(suffix);

    // ================= QUERY =================

    [LuaMember("contains")]
    public bool Contains(string key) => _container.Contains(key);

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
    public LuaValue Get(string key) => _container.Get<LuaValue>(key);

    // ================= SET =================

    [LuaMember("set")]
    public void Set(string key, LuaValue value) => _container.Set(key, value);
    
    // ================= REMOVE & CLEAR =================

    [LuaMember("remove")]
    public bool Remove(string key) => _container.Remove(key);

    [LuaMember("clear")]
    public void Clear() => _container.Clear();

    
}