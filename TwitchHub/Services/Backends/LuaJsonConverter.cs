using Lua;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TwitchHub.Services.Backends;

public static class LuaJsonConverter
{
    public static JsonNode? ToJson(LuaValue value) => value.Type switch
    {
        LuaValueType.Nil => null,
        LuaValueType.Boolean => JsonValue.Create(value.Read<bool>()),
        LuaValueType.Number => JsonValue.Create(value.Read<double>()),
        LuaValueType.String => JsonValue.Create(value.Read<string>()),
        LuaValueType.Table => TableToJson(value.Read<LuaTable>()),
        _ => throw new NotSupportedException($"Lua type {value.Type} is not supported")
    };

    private static JsonNode TableToJson(LuaTable table)
        => table.ArrayLength > 0 && table.HashMapCount == 0
        ? TableToJsonArray(table)
        : TableToJsonMap(table);

    private static JsonArray TableToJsonArray(LuaTable table)
    {
        var arr = new JsonArray();
        foreach (var v in table.GetArraySpan())
            arr.Add(ToJson(v));
        return arr;
    }

    private static JsonObject TableToJsonMap(LuaTable table)
    {
        var obj = new JsonObject();
        foreach (var (k, v) in table)
            obj[k.ToString()] = ToJson(v);
        return obj;
    }

    public static LuaValue FromJson(JsonNode? node) => node is null
            ? LuaValue.Nil
            : node switch
            {
                JsonValue v => FromJsonValue(v),
                JsonArray a => FromJsonArray(a),
                JsonObject o => FromJsonObject(o),
                _ => LuaValue.Nil
            };

    private static LuaValue FromJsonValue(JsonValue value) => value.GetValueKind() switch
    {
        JsonValueKind.True or JsonValueKind.False => value.GetValue<bool>(),
        JsonValueKind.String => value.GetValue<string>(),
        JsonValueKind.Number => value.GetValue<double>(),
        _ => LuaValue.Nil
    };

    private static LuaValue FromJsonArray(JsonArray array)
    {
        var table = new LuaTable(array.Count, 0);
        foreach (var (idx, item) in array.Index())
            table[idx + 1] = FromJson(item);
        return table;
    }

    private static LuaValue FromJsonObject(JsonObject obj)
    {
        var result = new LuaTable(0, obj.Count);
        foreach (var (k, v) in obj)
            result[new LuaValue(k)] = FromJson(v);

        return result;
    }
}
