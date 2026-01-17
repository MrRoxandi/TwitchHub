using Lua;

namespace TwitchHub.Lua.Services;

public sealed class LuaReaction(
    string filePath,
    LuaReactionKind kind,
    LuaState state,
    LuaFunction oncall,
    LuaFunction? onerror,
    long cooldownMili = 0)
{
    public string Name = Path.GetFileNameWithoutExtension(filePath);
    public string FilePath { get; init; } = filePath;
    public LuaReactionKind Kind { get; init; } = kind;
    public double CoolDown { get; init; } = cooldownMili;
    public bool IsEnabled { get; set; } = true;

    public DateTimeOffset LastExecuted { get; private set; } = DateTimeOffset.MinValue;
    private readonly LuaFunction _action = oncall;
    private readonly LuaFunction? _onError = onerror;
    private readonly LuaState _luaState = state;

    public async Task<LuaCallResult> CallAsync(params LuaValue[] args)
    {
        var CallResult = new LuaCallResult
        {
            Success = true,
            Result = LuaValue.Nil
        };

        if (!IsEnabled || LastExecuted.AddMilliseconds(CoolDown) >= DateTimeOffset.UtcNow)
            return CallResult;
        try
        {
            var result = await _luaState.CallAsync(_action, args);
            CallResult.Result = result.FirstOrDefault(LuaValue.Nil);
            LastExecuted = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            var callTime = DateTimeOffset.UtcNow.Ticks;
            CallResult.Success = false;
            CallResult.ErrorMessage = ex.Message;
            if (_onError is not null)
            {
                var callres = await _luaState.CallAsync(_onError, [Name, ex.Message, callTime]);
                CallResult.Result = callres.FirstOrDefault(LuaValue.Nil);
            }
            else
            {
                CallResult.Result = LuaValue.Nil;
            }
        }

        return CallResult;
    }
}
