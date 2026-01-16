using Lua;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaLoggerLib(
    ILogger<LuaLoggerLib> logger,
    LuaUtilsLib utils)
{
    private readonly ILogger<LuaLoggerLib> _logger = logger;
    private readonly LuaUtilsLib _utils = utils;

    // ================= SIMPLE LOGGING =================

    [LuaMember]
    public void LogInfo(string message) => _logger.LogInformation("{Message}", message);

    [LuaMember]
    public void LogDebug(string message) => _logger.LogDebug("{Message}", message);

    [LuaMember]
    public void LogWarning(string message) => _logger.LogWarning("{Message}", message);

    [LuaMember]
    public void LogError(string message) => _logger.LogError("{Message}", message);

    // ================= FORMATTED LOGGING =================

    [LuaMember]
    public void LogInfoFmt(string message, LuaTable args)
        => _logger.LogInformation(message, ToLogArgs(args));

    [LuaMember]
    public void LogDebugFmt(string message, LuaTable args)
        => _logger.LogDebug(message, ToLogArgs(args));

    [LuaMember]
    public void LogWarnFmt(string message, LuaTable args)
        => _logger.LogWarning(message, ToLogArgs(args));

    [LuaMember]
    public void LogErrorFmt(string message, LuaTable args)
        => _logger.LogError(message, ToLogArgs(args));

    // ================= HELPERS =================

    private object[] ToLogArgs(LuaTable table)
    {
        if (table.ArrayLength == 0 && table.HashMapCount == 0)
        {
            return [];
        }

        LuaValue[] luaValues = _utils.IsLuaArray(table)
            ? [.. table.GetArraySpan()]
            : [.. table.Select(kvp => kvp.Value)];

        var result = new object[luaValues.Length];
        for (var i = 0; i < luaValues.Length; i++)
        {
            result[i] = luaValues[i].ToString();
        }

        return result;
    }
}