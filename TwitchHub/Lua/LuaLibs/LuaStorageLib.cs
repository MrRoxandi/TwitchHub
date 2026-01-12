using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaStorageLib(HubDataContainer container)
{
    private readonly HubDataContainer _container = container;

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
    public LuaValue GetTable(string key) => _container.GetLuaTable(key) is not { } table
        ? LuaValue.Nil
        : table;

    [LuaMember]
    public string GetString(string key)
    {
        var v = _container.Get<string>(key);
        return string.IsNullOrEmpty(v) ? string.Empty : v;
    }

    [LuaMember]
    public double GetNumber(string key) => _container.Get<double>(key);

    [LuaMember]
    public bool GetBool(string key) => _container.Get<bool>(key);

    // ================= SET =================

    [LuaMember]
    public void Set(string key, LuaValue value)
    {
        switch (value.Type)
        {
            case LuaValueType.Nil:
                _ = _container.Remove(key);
                break;

            case LuaValueType.Boolean:
                _container.Set(key, value.Read<bool>());
                break;

            case LuaValueType.Number:
                _container.Set(key, value.Read<double>());
                break;

            case LuaValueType.Table:
                _container.SetLuaTable(key, value.Read<LuaTable>());
                break;
            case LuaValueType.String:
                _container.Set(key, value.Read<string>());
                break;
            default:
                _container.Set(key, value.ToString());
                break;
        }
    }

    [LuaMember]
    public void SetTable(string key, LuaTable table) => _container.SetLuaTable(key, table);

    [LuaMember]
    public void SetString(string key, string value) => _container.Set(key, value);

    [LuaMember]
    public void SetNumber(string key, double value) => _container.Set(key, value);

    [LuaMember]
    public void SetBool(string key, bool value) => _container.Set(key, value);

    // ================= REMOVE & CLEAR =================

    [LuaMember]
    public bool Remove(string key) => _container.Remove(key);

    [LuaMember]
    public void Clear() => _container.Clear();

    // ================= CONVERSION HELPERS =================

    private static LuaValue ObjectToLua(object? value) => value switch
    {
        null => LuaValue.Nil,
        bool b => LuaValue.FromObject(b),
        string s => LuaValue.FromObject(s),
        double d => LuaValue.FromObject(d),
        float f => LuaValue.FromObject(f),
        int i => LuaValue.FromObject((double)i),
        long l => LuaValue.FromObject((double)l),

        IDictionary<string, object?> dict
            => LuaValue.FromObject(DictToLuaTable(dict)),

        IList<object?> list
            => LuaValue.FromObject(ListToLuaTable(list)),

        _ => LuaValue.Nil
    };

    private static object? LuaValueToObject(LuaValue value) => value.Type switch
    {
        LuaValueType.Nil => null,
        LuaValueType.Boolean => value.Read<bool>(),
        LuaValueType.Number => value.Read<double>(),
        LuaValueType.String => value.Read<string>(),
        LuaValueType.Table => LuaTableToObject(value.Read<LuaTable>()),
        _ => null
    };

    private static LuaTable DictToLuaTable(IDictionary<string, object?> dict)
    {
        var table = new LuaTable();

        foreach (var (key, value) in dict)
            table[key] = ObjectToLua(value);

        return table;
    }

    private static LuaTable ListToLuaTable(IList<object?> list)
    {
        var table = new LuaTable();

        for (var i = 0; i < list.Count; i++)
            table[i + 1] = ObjectToLua(list[i]);

        return table;
    }

    private static object LuaTableToObject(LuaTable table)
    {
        if (IsArrayTable(table))
        {
            var list = new List<object?>();
            var arraySpan = table.GetArraySpan();

            foreach (var value in arraySpan)
                list.Add(LuaValueToObject(value));

            return list;
        }

        var dict = new Dictionary<string, object?>();

        foreach (var (key, value) in table)
        {
            var keyStr = key.Type == LuaValueType.String
                ? key.Read<string>() ?? string.Empty
                : key.ToString();

            dict[keyStr] = LuaValueToObject(value);
        }

        return dict;
    }

    private static bool IsArrayTable(LuaTable table) => table.ArrayLength > 0 && table.HashMapCount == 0;
}