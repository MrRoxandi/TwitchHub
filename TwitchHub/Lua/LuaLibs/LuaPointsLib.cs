using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaPointsLib(LuaPointsService pointsService)
{
    private readonly LuaPointsService _pointsService = pointsService;

    [LuaMember]
    public async Task<long> Get(string userId)
        => await _pointsService.GetBalanceAsync(userId);

    [LuaMember]
    public async Task Set(string userId, long amount) => await _pointsService.SetBalanceAsync(userId, amount);

    [LuaMember]
    public async Task Add(string userId, long amount) => await _pointsService.AddBalanceAsync(userId, amount);

    [LuaMember]
    public async Task<bool> Take(string userId, long amount) => await _pointsService.TakeBalanceAsync(userId, amount);

}