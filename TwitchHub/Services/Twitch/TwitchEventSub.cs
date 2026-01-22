using Lua;
using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchHub.Lua.Services;
using TwitchHub.Services.Twitch.Data;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Core.EventArgs.Stream;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace TwitchHub.Services.Twitch;

public sealed class TwitchEventSub : IHostedService, IDisposable
{
    private readonly EventSubWebsocketClient _client;
    private readonly LuaReactionsService _luaReactions;
    private readonly TwitchAPI _api;
    private readonly TwitchTokenProvider _tokenProvider;
    private readonly TwitchConfiguration _config;
    private readonly ILogger<TwitchEventSub> _logger;

    public bool IsConnected { get; private set; }
    private string? _broadcasterId;
    private bool _disposed;

    public TwitchEventSub(
        ILogger<TwitchEventSub> logger,
        IOptions<TwitchConfiguration> config,
        LuaReactionsService luaReactions,
        EventSubWebsocketClient client,
        TwitchTokenProvider tokenProvider,
        TwitchAPI api)
    {
        _api = api;
        _logger = logger;
        _client = client;
        _config = config.Value;
        _luaReactions = luaReactions;
        _tokenProvider = tokenProvider;
        HookEvents();
    }

    private async Task OnTokenRefreshed(string arg)
    {
        _logger.LogInformation("Token refreshed. Reconnecting EventSub Websocket...");
        if (IsConnected)
        {
            await _client.DisconnectAsync();
            IsConnected = false;
        }
        await Task.Delay(TimeSpan.FromSeconds(5));
        IsConnected = await _client.ConnectAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IsConnected = await _client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        await _client.DisconnectAsync();
        IsConnected = false;
    }

    private async Task OnWebsocketConnected(object? sender, WebsocketConnectedArgs e)
    {
        _logger.LogInformation("Websocket {sessionId} connected!", _client.SessionId);

        if (e.IsRequestedReconnect)
        {
            return;
        }

        try
        {
            var token = await _tokenProvider.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Can't create subscriptions: token is null or empty");
                return;
            }

            if (string.IsNullOrEmpty(_broadcasterId))
            {
                var users = await _api.Helix.Users.GetUsersAsync(logins: [_config.Channel]);
                _broadcasterId = users.Users.FirstOrDefault()?.Id;
            }

            if (_broadcasterId == null)
            {
                _logger.LogError("Could not resolve ID for channel {Channel}", _config.Channel);
                return;
            }

            await SubscribeToEvents(_client.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing EventSub subscriptions");
        }
    }

    private async Task SubscribeToEvents(string sessionId)
    {
        var bId = _broadcasterId!;
        static Dictionary<string, string> BroadcasterOnly(string id) =>
            new() { ["broadcaster_user_id"] = id };

        List<Task> subs =
        [
            CreateSub("channel.follow", "2", new Dictionary<string, string>
            {
                { "broadcaster_user_id", bId },
                { "moderator_user_id", bId }
            }, sessionId),

            CreateSub("channel.subscribe", "1", BroadcasterOnly(bId), sessionId),

            CreateSub("channel.subscription.gift", "1", BroadcasterOnly(bId), sessionId),

            CreateSub("channel.cheer", "1", BroadcasterOnly(bId), sessionId),

            CreateSub("channel.channel_points_custom_reward_redemption.add", "1", BroadcasterOnly(bId), sessionId),

            CreateSub("stream.online", "1", BroadcasterOnly(bId), sessionId),

            CreateSub("stream.offline", "1", BroadcasterOnly(bId), sessionId)
        ];

        await Task.WhenAll(subs);
        _logger.LogInformation("EventSub subscriptions sent.");
    }

    private async Task CreateSub(string type, string version, Dictionary<string, string> condition, string sessionId)
    {
        try
        {
            _ = await _api.Helix.EventSub.CreateEventSubSubscriptionAsync(
                type, version, condition, EventSubTransportMethod.Websocket, sessionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to {Type}", type);
        }
    }

    private async Task OnWebsocketDisconnected(object? sender, WebsocketDisconnectedArgs e)
    {
        _logger.LogError("Websocket {SessionId} disconnected!", _client.SessionId);
        if (!_disposed)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _ = await _client.ReconnectAsync();
        }
    }

    private Task OnWebsocketReconnected(object? sender, WebsocketReconnectedArgs e)
    {
        _logger.LogWarning("Websocket {SessionId} reconnected", _client.SessionId);
        return Task.CompletedTask;
    }

    private Task OnErrorOccurred(object? sender, ErrorOccuredArgs e)
    {
        _logger.LogError(e.Exception, "Websocket Error: {Message}", e.Message);
        return Task.CompletedTask;
    }

    private async Task OnChannelFollow(object? sender, ChannelFollowArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("New Follower: {User}", evt.UserName);
        await _luaReactions.CallAsync(LuaReactionKind.Follow, evt.UserName, evt.UserId);
    }

    private async Task OnChannelSubscribe(object? sender, ChannelSubscribeArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("New Subscriber: {User} (Tier {Tier})", evt.UserName, evt.Tier);
        await _luaReactions.CallAsync(LuaReactionKind.Subscribe, evt.UserName, evt.UserId, int.TryParse(evt.Tier, out var tier) ? tier : 1000, evt.IsGift);
    }

    private async Task OnChannelSubscriptionGift(object? sender, ChannelSubscriptionGiftArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("{User} gifted {Total} subs!", evt.UserName, evt.Total);
        await _luaReactions.CallAsync(LuaReactionKind.GiftSubscribe,
            evt.UserName ?? LuaValue.Nil, evt.UserId ?? LuaValue.Nil, evt.Total, int.TryParse(evt.Tier, out var tier) ? tier : 1000, evt.CumulativeTotal ?? evt.Total);
    }

    private async Task OnChannelCheer(object? sender, ChannelCheerArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("{User} cheered {Bits} bits!", evt.UserName, evt.Bits);
        await _luaReactions.CallAsync(LuaReactionKind.Cheer,
            evt.UserName ?? LuaValue.Nil, evt.UserId ?? LuaValue.Nil, evt.Bits, evt.Message ?? LuaValue.Nil);
    }

    private async Task OnChannelPointsRedemption(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("Reward Redeemed: {Reward} by {User}", evt.Reward.Title, evt.UserName);
        await _luaReactions.CallAsync(LuaReactionKind.Reward, evt.UserName, evt.UserId,
            evt.Reward.Title, evt.Reward.Id, evt.UserInput, evt.Reward.Cost);
    }

    private async Task OnStreamOnline(object? sender, StreamOnlineArgs e)
    {
        var evt = e.Payload.Event;
        _logger.LogInformation("Stream is Online! Type: {Type}", evt.Type);
        await _luaReactions.CallAsync(LuaReactionKind.StreamOn, evt.StartedAt.Ticks, evt.Type);
    }

    private async Task OnStreamOffline(object? sender, StreamOfflineArgs e)
    {
        _logger.LogInformation("Stream is Offline.");
        await _luaReactions.CallAsync(LuaReactionKind.StreamOff);
    }


    private void HookEvents()
    {
        _tokenProvider.OnTokenRefreshed += OnTokenRefreshed;
        _client.WebsocketConnected += OnWebsocketConnected;
        _client.WebsocketDisconnected += OnWebsocketDisconnected;
        _client.ChannelFollow += OnChannelFollow;
        _client.ChannelSubscribe += OnChannelSubscribe;
        _client.ChannelSubscriptionGift += OnChannelSubscriptionGift;
        _client.ChannelCheer += OnChannelCheer;
        _client.ChannelPointsCustomRewardRedemptionAdd += OnChannelPointsRedemption;
        _client.StreamOnline += OnStreamOnline;
        _client.StreamOffline += OnStreamOffline;
        _client.ErrorOccurred += OnErrorOccurred;
    }
    private void UnHookEvents()
    {
        _tokenProvider.OnTokenRefreshed -= OnTokenRefreshed;
        _client.WebsocketConnected -= OnWebsocketConnected;
        _client.WebsocketDisconnected -= OnWebsocketDisconnected;
        _client.ChannelFollow -= OnChannelFollow;
        _client.ChannelSubscribe -= OnChannelSubscribe;
        _client.ChannelSubscriptionGift -= OnChannelSubscriptionGift;
        _client.ChannelCheer -= OnChannelCheer;
        _client.ChannelPointsCustomRewardRedemptionAdd -= OnChannelPointsRedemption;
        _client.StreamOnline -= OnStreamOnline;
        _client.StreamOffline -= OnStreamOffline;
        _client.ErrorOccurred -= OnErrorOccurred;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        UnHookEvents();
    }

}
