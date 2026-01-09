using Lua;
using SharpHook;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaHadwareLib
{
    private readonly EventSimulator _sim;
    private readonly ILogger<LuaHadwareLib> _logger;
    private static SharpHook.Data.KeyCode MapKeyCode(int KeyCode) => (SharpHook.Data.KeyCode)KeyCode;

    [LuaMember]
    public readonly LuaTable KeyCodes;

    public LuaHadwareLib(ILogger<LuaHadwareLib> logger)
    {
        _sim = new();
        KeyCodes = new LuaTable();
        _logger = logger;
        foreach (var key in Enum.GetValues<SharpHook.Data.KeyCode>())
        {
            var strValue = key.ToString()[2..];
            var value = (int)key;
            KeyCodes[strValue] = value;
        }

        _logger.LogDebug("LuaInputSimulator lib constucted");
    }

    [LuaMember]
    public void PressKey(int rawKey)
    {
        var key = MapKeyCode(rawKey);
        var result = _sim.SimulateKeyPress(key);
        _logger.LogDebug("SimulateKeyPress ({key}): {result}", key, result);
    }
}
