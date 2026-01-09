using Lua;
using System.Threading.Tasks;
using TwitchLib.Client;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]public sealed partial class LuaTwitchLib
{
    private readonly ILogger<LuaTwitchLib> _logger;
    private readonly TwitchClient _client;
    public LuaTwitchLib(TwitchClient client, ILogger<LuaTwitchLib> logger)
    {
        _logger = logger;
        _client = client;
        _logger.LogDebug("LuaTwitchLib lib constucted");
    }

    [LuaMember]
    public async Task SendMessage(string message)
    {
        var channel = _client.JoinedChannels[0];
        await _client.SendMessageAsync(channel, message);
    }
}
