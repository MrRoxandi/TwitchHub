using Lua;

namespace TwitchHub.Lua.Services;

public sealed class LuaCallResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public LuaValue Result { get; set; }
}
