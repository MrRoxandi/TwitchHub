using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchHub.Services.Twitch.Data;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Interfaces;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace TwitchHub.Services.Twitch;

public sealed class TwitchEventSub : IHostedService
{
    private readonly EventSubWebsocketClient _client;
    private readonly TwitchAPI _api;
    private readonly TwitchTokenProvider _tokenProvider;
    private readonly TwitchConfiguration _config;
    private readonly ILogger<TwitchEventSub> _logger;

    private string? _broadcasterId;

    public TwitchEventSub(
        ILogger<TwitchEventSub> logger,
        EventSubWebsocketClient client,
        TwitchAPI api,
        TwitchTokenProvider tokenProvider,
        IOptions<TwitchConfiguration> config)
    {
        _logger = logger;
        _client = client;
        _api = api;
        _tokenProvider = tokenProvider;
        _config = config.Value;

        _client.WebsocketConnected += OnWebsocketConnected;
        _client.WebsocketDisconnected += OnWebsocketDisconnected;
        _client.ErrorOccurred += OnErrorOccurred;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync();
    }

    private async Task OnWebsocketConnected(object? sender, WebsocketConnectedArgs e)
    {
        _logger.LogInformation("Websocket {sessionId} connected!", _client.SessionId);
        if (e.IsRequestedReconnect) 
            return;
        try
        {
            var token = await _tokenProvider.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("EventSub: No token to create subscriptions.");
                return;
            }

            if (string.IsNullOrEmpty(_broadcasterId))
            {
                var users = await _api.Helix.Users.GetUsersAsync(logins: [_config.Channel]);
                _broadcasterId = users.Users.FirstOrDefault()?.Id;
            }

            if (_broadcasterId == null)
            {
                _logger.LogError("EventSub: Could not resolve ID for channel {Channel}", _config.Channel);
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
        var condition = new Dictionary<string, string> { { "broadcaster_user_id", _broadcasterId! }, { "user_id", _broadcasterId! } };

        // Пример: подписка на фолловы
        await _api.Helix.EventSub.CreateEventSubSubscriptionAsync(
            "channel.follow", "2",
            new Dictionary<string, string> { { "broadcaster_user_id", _broadcasterId! }, { "moderator_user_id", _broadcasterId! } },
            TwitchLib.Api.Core.Enums.EventSubTransportMethod.Websocket,
            sessionId
        );

        _logger.LogInformation("EventSub subscriptions sent.");
    }

    private async Task OnWebsocketDisconnected(object? sender, WebsocketDisconnectedArgs e)
    {
        _logger.LogError("Websocket {SessionId} disconnected!", _client.SessionId);

    }

    private async Task OnWebsocketReconnected(object? sender, WebsocketReconnectedArgs e)
    {
        _logger.LogWarning("Websocket {SessionId} reconnected", _client.SessionId);
    }

    private async Task OnErrorOccurred(object? sender, ErrorOccuredArgs e)
    {
        _logger.LogError(e.Exception, "Websocket {SessionId} - Error occurred!", _client.SessionId);
    }

    // Subscribed events goes down here
}
