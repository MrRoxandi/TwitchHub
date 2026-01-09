using Lua;
using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaTwitchLib(
    TwitchClient client,
    IOptions<TwitchConfig> config,
    TwitchAPI twitchApi,
    ILogger<LuaTwitchLib> logger
    )
{
    private readonly ILogger<LuaTwitchLib> _logger = logger;
    private readonly TwitchClient _client = client;
    private readonly TwitchConfig _config = config.Value;   
    private readonly TwitchAPI _twitchApi = twitchApi;
    private string _broadcasterId = string.Empty;

    private JoinedChannel? JoinedChannel => _client.GetJoinedChannel(_config.Channel);
    [LuaMember]
    public bool IsConnected => _client.IsConnected;

    // Sending message block
    [LuaMember]
    public async Task SendMessage(string message)
    {
        AssertCanSendMessage();
        await _client.SendMessageAsync(JoinedChannel!, message);
    }

    [LuaMember]
    public async Task SendReply(string userId, string message)
    {
        AssertCanSendMessage();
        await _client.SendReplyAsync(JoinedChannel!, userId, message);
    }

    [LuaMember]
    public async Task<string> GetUserId(string username)
    {
        var response = await _twitchApi.Helix.Users.GetUsersAsync(logins: [username]);
        return response.Users.FirstOrDefault() is not { } user
            ? throw new Exception("Unable to find user with username: " + username)
            : user.Id;
    }

    [LuaMember]
    public async Task<string> GetUserName(string userid)
    {
        var response = await _twitchApi.Helix.Users.GetUsersAsync(ids: [userid]);
        return response.Users.FirstOrDefault() is not { } user
            ? throw new Exception("Unable to find user with user id: " + userid)
            : user.Login;
    }

    [LuaMember]
    public async Task<bool> IsBroadcaster(string userid)
    {
        var broadcasterId = await GetBroadcasterId();
        return broadcasterId.Equals(userid, StringComparison.Ordinal);
    }

    [LuaMember]
    public async Task<bool> IsModerator(string userid)
    {
        var broadcasterId = await GetBroadcasterId();
        var response = await _twitchApi.Helix.Moderation.GetModeratorsAsync(broadcasterId, [userid]);
        return response.Data.Length != 0;
    }

    [LuaMember]
    public async Task<bool> IsVip(string userid)
    {
        var broadcasterId = await GetBroadcasterId();
        var response = await _twitchApi.Helix.Channels.GetVIPsAsync(broadcasterId, [userid]);
        return response.Data.Length != 0;
    }

    [LuaMember]
    public async Task<bool> IsFollower(string userid)
    {
        var broadcasterId = await GetBroadcasterId();
        var response = await _twitchApi.Helix.Channels.GetChannelFollowersAsync(broadcasterId, userid);
        return response.Data.Length != 0;
    }

    // Should return UTC timestamp
    [LuaMember]
    public async Task<int> GetFollowDate(string userid)
    {
        var broadcasterId = await GetBroadcasterId();
        var response = await _twitchApi.Helix.Channels.GetChannelFollowersAsync(broadcasterId, userid);
        if (response.Data.Length == 0)
            return -1;
        var channelFollower = response.Data.First();
        return int.Parse(channelFollower.FollowedAt);
    }

    private async Task<string> GetBroadcasterId()
    {
        if (!string.IsNullOrEmpty(_broadcasterId))
        {
            return _broadcasterId;
        }

        var response = await _twitchApi.Helix.Users.GetUsersAsync(logins: [_config.Channel]);
        if (response.Users.FirstOrDefault() is not { } user)
        {
            throw new Exception("Unable to find broadcaster with channel name: " + _config.Channel);
        }

        _broadcasterId = user.Id;
        return user.Id;

    }

    private void AssertCanSendMessage()
    {
        if (!IsConnected)
        {
            throw new Exception("Unable to send message, Twitch client is not connected.");
        }

        if (JoinedChannel is null)
        {
            throw new Exception($"Unable to send message, Twitch client is not joined to channel {_config.Channel}.");
        }
    }
}
