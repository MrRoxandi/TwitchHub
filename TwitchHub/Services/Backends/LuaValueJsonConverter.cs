using Lua;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TwitchHub.Services.Backends;

public sealed class LuaValueJsonConverter : JsonConverter<LuaValue>
{
    public override LuaValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var node = JsonNode
            .Parse(ref reader, new JsonNodeOptions { 
                PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive 
            });
        return LuaJsonConverter.FromJson(node);
    }
    public override void Write(
        Utf8JsonWriter writer, 
        LuaValue value, 
        JsonSerializerOptions options)
    {
        var node = LuaJsonConverter.ToJson(value);
        node?.WriteTo(writer, options);
    }
}
