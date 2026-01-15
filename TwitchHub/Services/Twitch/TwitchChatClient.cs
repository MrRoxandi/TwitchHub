using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchHub.Lua.Services;
using TwitchHub.Services.Twitch.Data;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchHub.Services.Twitch;

public sealed class TwitchChatClient : IHostedService, IDisposable
{
    private readonly TwitchClient _client;
    private readonly ILogger<TwitchChatClient> _logger;
    private readonly TwitchConfiguration _configuration;
    private readonly TwitchTokenProvider _tokenProvider;
    private readonly LuaReactionsService _reactions;

    private readonly CancellationTokenSource _connectionCts = new();
    private bool _isDisposed;
    public TwitchChatClient(
        ILoggerFactory loggerFactory,
        LuaReactionsService reactions,
        IOptions<TwitchConfiguration> config,
        TwitchClient client,
        TwitchTokenProvider tokenProvider)
    {
        _client = client;
        _logger = loggerFactory.CreateLogger<TwitchChatClient>();
        _configuration = config.Value;
        _tokenProvider = tokenProvider;
        _reactions = reactions;
        _tokenProvider.OnTokenRefreshed += OnTokenRefreshedAsync;
    }

    public bool IsConnected => _client.IsConnected;

    // ---------- START ----------

    public async Task StartAsync(CancellationToken ct)
    {
        HookEvents();
        await ConnectAsync(ct);
    }

    // ---------- CONNECT ----------

    private async Task ConnectAsync(CancellationToken ct)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Cannot connect to Chat: No token available.");
            return;
        }

        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }

        var credentials = new ConnectionCredentials(_configuration.Channel, token);
        _client.Initialize(credentials);

        try
        {
            _ = await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Twitch Chat.");
        }
    }

    private async Task OnTokenRefreshedAsync(string newToken)
    {
        _logger.LogInformation("Token refreshed. Reconnecting Chat...");
        await ConnectAsync(CancellationToken.None);
    }

    // ---------- HOOKS ----------
    private void HookEvents()
    {
        _client.OnConnected += OnConnected;
        _client.OnMessageReceived += OnMessageReceived;
        _client.OnChatCommandReceived += OnChatCommandReceived;
        _client.OnDisconnected += OnDisconnected;
        _client.OnError += OnError;
    }

    private void UnhookEvents()
    {
        _client.OnConnected -= OnConnected;
        _client.OnMessageReceived -= OnMessageReceived;
        _client.OnChatCommandReceived -= OnChatCommandReceived;
        _client.OnDisconnected -= OnDisconnected;
        _client.OnError -= OnError;
    }

    // ---------- STOP ----------

    public async Task StopAsync(CancellationToken ct)
    {
        _connectionCts.Cancel();
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }

        UnhookEvents();
    }

    // ---------- EVENTS ----------

    private async Task OnConnected(object? sender, OnConnectedEventArgs e)
    {
        _logger.LogInformation("Connected to Twitch chat as {Username}", e.BotUsername);
        await _client.JoinChannelAsync(_configuration.Channel);
    }

    private async Task OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        _logger.LogDebug("Recieved message '{message}' from '{username}'", e.ChatMessage.Message, e.ChatMessage.Username);
        try
        {
            await _reactions.CallAsync(LuaReactionKind.Message, e.ChatMessage.Username, e.ChatMessage.UserId, e.ChatMessage.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message reaction");
        }
    }

    private async Task OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        _logger.LogDebug("Recieved command '{command}' from '{username}'", e.Command.Name, e.ChatMessage.Username);
        if (!_reactions.Get(LuaReactionKind.Command)
            .Any(r => r.Name.Equals(e.Command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        await _reactions.CallAsync(e.Command.Name, LuaReactionKind.Command,
            e.ChatMessage.Username, e.ChatMessage.UserId, e.Command.ArgumentsAsString);
    }

    private async Task OnDisconnected(object? sender, OnDisconnectedArgs e)
    {
        _logger.LogWarning("Disconnected from Twitch chat");
        if (!_connectionCts.IsCancellationRequested)
        {
            await ConnectAsync(_connectionCts.Token);
        }
    }

    private Task OnError(object? sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Twitch Client Error");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _tokenProvider.OnTokenRefreshed -= OnTokenRefreshedAsync;
        _isDisposed = true;
    }
}
