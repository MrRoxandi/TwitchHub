using Lua;
using System.Threading.Tasks;
using TwitchHub.Lua.Services;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaScriptLib(
    LuaScriptsSerivce luaScripts)
{
    private readonly LuaScriptsSerivce _luaScripts = luaScripts;

    [LuaMember("keys")]
    public LuaTable Keys()
    {
        var table = new LuaTable();
        foreach (var (idx, key) in _luaScripts.Keys.Index())
            table[idx + 1] = key;
        return table;
    }

    [LuaMember("contains")]
    public bool Contains(string key) => _luaScripts.Contains(key);
    [LuaMember("remove")]
    public void Remove(string key) => _luaScripts.RemoveScript(key);
    [LuaMember("call")]
    public async Task Call(string key) => await _luaScripts.CallAsync(key);
}
