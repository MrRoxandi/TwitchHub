using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaUtilsLib
{
    private readonly Random _random = Random.Shared;

    public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    // ================= RANDOM =================

    [LuaMember]
    public LuaValue RandomNumber(int min, int max) => _random.Next(min, max);

    [LuaMember]
    public LuaValue RandomDouble(double min, double max) => ((max - min) * _random.NextDouble()) + min;
    [LuaMember]
    public LuaValue RandomString(int length) => length > 0
        ? new string(_random.GetItems(Chars.AsSpan(), length))
        : string.Empty;

    [LuaMember]
    public LuaValue RandomPosition(int minx, int maxx, int miny, int maxy)
        => new LuaTable
        {
            ['X'] = RandomNumber(minx, maxx),
            ['Y'] = RandomNumber(miny, maxy)
        };
    // ================= LUA TABLES UTILS =================

    [LuaMember]
    public bool IsLuaArray(LuaTable table) => table.ArrayLength != 0 && table.HashMapCount == 0;
    [LuaMember]
    public bool IsTableEmpty(LuaTable table) => table.ArrayLength == 0 && table.HashMapCount == 0;

    [LuaMember]
    public bool TableContains(LuaTable table, LuaValue value)
    {
        if (IsLuaArray(table))
        {
            foreach (var v in table.GetArraySpan())
            {
                if (v == value)
                    return true;
            }
        }

        foreach (var (_, v) in table)
        {
            if (v == value)
                return true;
        }

        return false;
    }
    [LuaMember]
    public LuaValue TableRandom(LuaTable table)
    {
        if (IsTableEmpty(table))
            return LuaValue.Nil;
        if (IsLuaArray(table))
        {
            var span = table.GetArraySpan();
            return span[_random.Next(span.Length)];
        }

        var index = _random.Next(table.Count());
        return table.ElementAt(index).Value;
    }
    [LuaMember]
    public LuaTable TableCopy(LuaTable table)
    {
        var result = new LuaTable(table.ArrayLength, table.HashMapCount);
        foreach (var (k, v) in table)
        {
            result[k] = v;
        }

        return result;
    }

    [LuaMember]
    public LuaValue TableShuffle(LuaTable table)
    {
        var result = TableCopy(table);
        _random.Shuffle(result.GetArraySpan());
        return result;
    }

    [LuaMember]
    public string TableJoin(LuaTable table, string sep = ", ")
        => IsLuaArray(table)
            ? string.Join(sep, table.Select(e => e.Value.ToString()))
            : string.Join(sep, table.Select(e => $"[{e.Key}]: {e.Value}"));

    [LuaMember]
    public string TableToJson(LuaTable table)
        => LuaJsonConverter.ToJson(table)?.ToJsonString() ?? string.Empty;
}
