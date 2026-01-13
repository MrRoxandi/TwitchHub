using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaStorageLib(LuaDataContainer container)
{
    private readonly LuaDataContainer _container = container;

    // ================= FILE OPERATIONS =================

    [LuaMember]
    public async Task Load() => await _container.LoadAsync();

    [LuaMember]
    public async Task Save() => await _container.SaveAsync();

    [LuaMember]
    public async Task Backup(string? suffix = null) => await _container.BackupAsync(suffix);

    // ================= QUERY =================

    [LuaMember]
    public bool Contains(string key) => _container.Contains(key);

    [LuaMember]
    public int Count() => _container.Count;

    [LuaMember]
    public LuaValue Keys()
    {
        var table = new LuaTable();
        var index = 1;

        foreach (var key in _container.Keys)
            table[index++] = LuaValue.FromObject(key);

        return table;
    }

    // ================= GET =================

    [LuaMember]
    public LuaValue Get(string key) => _container.Get<LuaValue>(key);

    // ================= SET =================

    [LuaMember]
    public void Set(string key, LuaValue value) => _container.Set(key, value);
    
    // ================= REMOVE & CLEAR =================

    [LuaMember]
    public bool Remove(string key) => _container.Remove(key);

    [LuaMember]
    public void Clear() => _container.Clear();

    
}