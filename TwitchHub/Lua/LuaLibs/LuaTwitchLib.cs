using Lua;
using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaTwitchLib(
    TwitchClient client,
    IOptions<TwitchConfiguration> config,
    TwitchAPI twitchApi,
    ILogger<LuaTwitchLib> logger
    )
{
    private readonly TwitchClient _client = client;
    private readonly TwitchConfiguration _config = config.Value;
    private readonly TwitchAPI _api = twitchApi;
    private readonly ILogger<LuaTwitchLib> _logger = logger;

    private string _broadcasterId = string.Empty;
    private JoinedChannel? CurrentChannel
        => _client.GetJoinedChannel(_config.Channel);

    // ================= STATE =================

    [LuaMember]
    public bool IsConnected => _client.IsConnected;

    // ================= CHAT =================

    [LuaMember]
    public async Task SendMessage(string message)
    {
        EnsureChatReady();
        await _client.SendMessageAsync(CurrentChannel!, message);
    }

    [LuaMember]
    public async Task SendReply(string userId, string message)
    {
        EnsureChatReady();
        await _client.SendReplyAsync(CurrentChannel!, userId, message);
    }

    // ================= USERS =================

    [LuaMember]
    public async Task<string> GetUserId(string userName)
        => (await GetUserByLogin(userName)).Id;

    [LuaMember]
    public async Task<string> GetUserName(string userId)
        => (await GetUserById(userId)).Login;

    [LuaMember]
    public async Task<bool> IsBroadcaster(string userId)
        => string.Equals(userId, await GetBroadcasterId(), StringComparison.Ordinal);

    [LuaMember]
    public async Task<bool> IsModerator(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var result = await _api.Helix.Moderation
            .GetModeratorsAsync(broadcasterId, [userId]);

        return result.Data.Length > 0;
    }

    [LuaMember]
    public async Task<bool> IsVIP(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var result = await _api.Helix.Channels
            .GetVIPsAsync(broadcasterId, [userId]);

        return result.Data.Length > 0;
    }

    [LuaMember]
    public async Task<bool> IsFollower(string userId)
        => await GetFollower(userId) is not null;

    /// <summary>UTC Unix timestamp or -1</summary>
    [LuaMember]
    public async Task<long> GetFollowDate(string userId)
    {
        var follower = await GetFollower(userId);
        return follower is null
            ? -1
            : DateTimeOffset.Parse(follower.FollowedAt).ToUnixTimeSeconds();
    }

    // ================= INTERNAL =================

    private async Task<string> GetBroadcasterId()
    {
        if (_broadcasterId is not null)
            return _broadcasterId;

        var user = await GetUserByLogin(_config.Channel);
        _broadcasterId = user.Id;

        return user.Id;
    }
    private async Task<User> GetUserByLogin(string login)
    {
        var response = await _api.Helix.Users.GetUsersAsync(logins: [login]);
        return response.Users.FirstOrDefault()
            ?? throw new Exception($"User not found: {login}");
    }

    private async Task<User> GetUserById(string id)
    {
        var response = await _api.Helix.Users.GetUsersAsync(ids: [id]);
        return response.Users.FirstOrDefault()
            ?? throw new Exception($"User not found (id): {id}");
    }

    private async Task<ChannelFollower?> GetFollower(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var response = await _api.Helix.Channels
            .GetChannelFollowersAsync(broadcasterId, userId);
        return response.Data.FirstOrDefault();
    }

    private void EnsureChatReady()
    {
        if (!IsConnected)
        {
            throw new Exception("Unable to send message, Twitch client is not connected.");
        }

        if (CurrentChannel is null)
        {
            throw new Exception($"Unable to send message, Twitch client is not joined to channel {_config.Channel}.");
        }
    }
}
