using TwitchHub.Services.Backends;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchHub.Services.Twitch;

public sealed class ChatClient(
    ILoggerFactory loggerFactory,
    IConfiguration config,
    TwitchClient client) : IHostedService
{
    private readonly TwitchClient _client = client;
    private readonly ILogger<ChatClient> _logger = loggerFactory.CreateLogger<ChatClient>();
    private readonly IConfiguration _configuration = config;

    public bool IsConnected => _client.IsConnected;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //var creadentials = new ConnectionCredentials(_configuration["Twitch:Channel"]!, _configuration["Twitch:AccessToken"]!);
        //_client.Initialize(creadentials);
        //_client.OnMessageReceived += client_OnMessageReceived;
        //_client.OnConnected += client_OnConnected;
        //_client.OnChatCommandReceived += client_OnChatCommandReceived;
        //var result = await _client.ConnectAsync();
    }

    // Connect by hand, if StartAsync failed connect due to bad accecs token and empty refreshtoken
    // May happen on first run, when user has not authorized the app yet
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {

    }
    // Disconnect by hand, if needed
    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //await _client.DisconnectAsync();
        //_client.OnMessageReceived -= client_OnMessageReceived;
        //_client.OnConnected -= client_OnConnected;
    }
    private Task client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {

        return Task.CompletedTask;
    }

    private async Task client_OnConnected(object? sender, OnConnectedEventArgs e) => await _client.JoinChannelAsync(_configuration["Twitch:Channel"]!); // replace with the channel you want to join


    private Task client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        _logger.LogInformation("{Username} -> {Message}", e.ChatMessage.Username, e.ChatMessage.Message);
        return Task.CompletedTask;
    }

  
}
