using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaPointsLib(
    ILogger<LuaPointsLib> logger,
    LuaPointsService pointsService
    )
{
    private readonly LuaPointsService _pointsService = pointsService;
    private readonly ILogger<LuaPointsLib> _logger = logger;

    [LuaMember("get")]
    public async Task<long> Get(string userId)
    {
        var value = await _pointsService.GetBalanceAsync(userId);
        _logger.LogDebug("Get: {userId} -> {value}", userId, value);
        return value;
    }

    [LuaMember("set")]
    public async Task Set(string userId, long amount)
    {
        await _pointsService.SetBalanceAsync(userId, amount);
        _logger.LogDebug("Set: {userid} -> {amount}", userId, amount);
    }

    [LuaMember("add")]
    public async Task Add(string userId, long amount)
    {
        await _pointsService.AddBalanceAsync(userId, amount);
        _logger.LogDebug("Add: {userid} -> {amount}", userId, amount);
    }

    [LuaMember("take")]
    public async Task<bool> Take(string userId, long amount)
    {
        var value = await _pointsService.TakeBalanceAsync(userId, amount);
        _logger.LogDebug("Take: {userid}, {amount} -> {value}", userId, amount, value);
        return value;
    }
}