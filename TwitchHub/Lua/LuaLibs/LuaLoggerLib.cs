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

    [LuaMember("loginfo")]
    public void LogInfo(string message) => _logger.LogInformation("{Message}", message);

    [LuaMember("logdebug")]
    public void LogDebug(string message) => _logger.LogDebug("{Message}", message);

    [LuaMember("logwarning")]
    public void LogWarning(string message) => _logger.LogWarning("{Message}", message);

    [LuaMember("logerror")]
    public void LogError(string message) => _logger.LogError("{Message}", message);

    // ================= FORMATTED LOGGING =================

    [LuaMember("loginfofmt")]
    public void LogInfoFmt(string message, LuaTable args)
        => _logger.LogInformation(message, ToLogArgs(args));

    [LuaMember("logdebugfmt")]
    public void LogDebugFmt(string message, LuaTable args)
        => _logger.LogDebug(message, ToLogArgs(args));

    [LuaMember("logwarningfmt")]
    public void LogWarnFmt(string message, LuaTable args)
        => _logger.LogWarning(message, ToLogArgs(args));

    [LuaMember("logerrorfmt")]
    public void LogErrorFmt(string message, LuaTable args)
        => _logger.LogError(message, ToLogArgs(args));

    // ================= HELPERS =================

    private static object?[] ToLogArgs(LuaTable table)
        => table.ArrayLength != 0 || table.HashMapCount != 0
        ? [.. table.Select(kvp => kvp.Value.ToString())]
        : [];
}
