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
    TwitchAPI twitchApi
    )
{
    private readonly TwitchClient _client = client;
    private readonly TwitchConfiguration _config = config.Value;
    private readonly TwitchAPI _api = twitchApi;

    private string _broadcasterId = string.Empty;
    private JoinedChannel? CurrentChannel
        => _client.GetJoinedChannel(_config.Channel);

    // ================= STATE =================

    [LuaMember("isconnected")]
    public bool IsConnected() => _client.IsConnected;

    // ================= CHAT =================

    [LuaMember("sendmessage")]
    public async Task SendMessage(string message)
    {
        EnsureChatReady();
        await _client.SendMessageAsync(CurrentChannel!, message);
    }

    // ================= USERS =================

    [LuaMember("getuserid")]
    public async Task<string> GetUserId(string userName)
        => (await GetUserByLogin(userName)).Id;

    [LuaMember("getusername")]
    public async Task<string> GetUserName(string userId)
        => (await GetUserById(userId)).Login;

    [LuaMember("isbroadcaster")]
    public async Task<bool> IsBroadcaster(string userId)
        => string.Equals(userId, await GetBroadcasterId(), StringComparison.Ordinal);

    [LuaMember("ismoderator")]
    public async Task<bool> IsModerator(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var result = await _api.Helix.Moderation
            .GetModeratorsAsync(broadcasterId, [userId]);

        return result.Data.Length > 0;
    }

    [LuaMember("issubscriber")]
    public async Task<bool> IsSubscriber(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var result = await _api.Helix.Subscriptions
            .GetUserSubscriptionsAsync(broadcasterId, [userId]);
        return result.Data.Length > 0;
    }

    [LuaMember("isvip")]
    public async Task<bool> IsVIP(string userId)
    {
        var broadcasterId = await GetBroadcasterId();
        var result = await _api.Helix.Channels
            .GetVIPsAsync(broadcasterId, [userId]);

        return result.Data.Length > 0;
    }

    [LuaMember("isfollower")]
    public async Task<bool> IsFollower(string userId)
        => await GetFollower(userId) is not null;

    /// <summary>UTC Unix timestamp or -1</summary>
    [LuaMember("getfollowdate")]
    public async Task<long> GetFollowDate(string userId)
    {
        var follower = await GetFollower(userId);
        return follower is null
            ? -1
            : DateTimeOffset.Parse(follower.FollowedAt).Ticks;
    }

    // ================= STREAM =================
    [LuaMember("getstreamtitle")]
    public async Task<string> GetStreamTitle()
    {
        var broadcasterId = await GetBroadcasterId();
        var stream = await GetCurrentStream(broadcasterId);
        return stream.Title;
    }
    [LuaMember("getstreamname")]
    public async Task<string> GetStreamGameName()
    {
        var broadcasterId = await GetBroadcasterId();
        var stream = await GetCurrentStream(broadcasterId);
        return stream.GameName;
    }

    [LuaMember("getstreamstartedat")]
    public async Task<long> GetStreamStartedAt()
    {
        var broadcasterId = await GetBroadcasterId();
        var stream = await GetCurrentStream(broadcasterId);
        return new DateTimeOffset(stream.StartedAt).Ticks;
    }

    [LuaMember("getstreamviewers")]
    public async Task<int> GetStreamViewers()
    {
        var broadcasterId = await GetBroadcasterId();
        var stream = await GetCurrentStream(broadcasterId);
        return stream.ViewerCount;
    }
    [LuaMember("atleast")]
    public async Task<bool> AtLeast(string userid, string thresholdraw)
    {
        if (!Enum.TryParse<TwitchRank>(thresholdraw, true, out var threshold))
        {
            return false;
        }

        var rank = await GetUserRank(userid);
        return rank.HasFlag(threshold);
    }

    // ================= INTERNAL =================

    private async Task<string> GetBroadcasterId()
    {
        if (_broadcasterId is not null)
        {
            return _broadcasterId;
        }

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

    private async Task<TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream> GetCurrentStream(string userId)
    {
        var streams = await _api.Helix.Streams.GetStreamsAsync(userIds: [userId]);
        return streams.Streams.FirstOrDefault()
            ?? throw new Exception($"Not found stream for user : {userId}");
    }

    private void EnsureChatReady()
    {
        if (!IsConnected())
        {
            throw new Exception("Unable to send message, Twitch client is not connected.");
        }

        if (CurrentChannel is null)
        {
            throw new Exception($"Unable to send message, Twitch client is not joined to channel {_config.Channel}.");
        }
    }

    [Flags]
    public enum TwitchRank
    {
        Viewer, Follower, Vip, Subscriber = 4, Moderator = 8, Broadcaster = 16
    }

    private async Task<TwitchRank> GetUserRank(string userId)
    {
        var rank = TwitchRank.Viewer;
        var isfollower = IsFollower(userId);
        var isvip = IsVIP(userId);
        var issub = IsSubscriber(userId);
        var ismoderator = IsModerator(userId);
        var isbroadcaster = IsBroadcaster(userId);
        _ = await Task.WhenAll(isfollower, isvip, issub, ismoderator, isbroadcaster);
        if (await isfollower)
        {
            rank |= TwitchRank.Viewer;
        }

        if (await isvip)
        {
            rank |= TwitchRank.Vip;
        }

        if (await issub)
        {
            rank |= TwitchRank.Subscriber;
        }

        if (await ismoderator)
        {
            rank |= TwitchRank.Moderator;
        }

        if (await isbroadcaster)
        {
            rank |= TwitchRank.Broadcaster;
        }

        return rank;
    }
}
