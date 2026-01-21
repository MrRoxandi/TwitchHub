using Lua;

namespace TwitchHub.Lua.Services;

public sealed class LuaScript(
    string filePath,
    LuaState state)
{
    public string Name = Path.GetFileNameWithoutExtension(filePath);
    public string FilePath = filePath;

    public bool IsEnabled = true;
    private readonly LuaState _state = state;

    public async Task<LuaCallResult> CallAsync()
    {
        var CallResult = new LuaCallResult
        {
            Success = true,
            Result = LuaValue.Nil
        };

        if (!IsEnabled)
            return CallResult;

        try
        {
            var result = await _state.DoFileAsync(FilePath);
            CallResult.Result = result.FirstOrDefault(LuaValue.Nil);
        }
        catch (Exception ex)
        {
            CallResult.Success = false;
            CallResult.ErrorMessage = ex.Message;
        }

        return CallResult;
    }
}
