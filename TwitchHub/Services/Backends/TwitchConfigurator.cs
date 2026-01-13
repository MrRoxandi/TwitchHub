using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using TwitchHub.Configurations;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Enums;

namespace TwitchHub.Services.Backends;

public class TwitchConfigurator
{

    private readonly ILogger<TwitchConfigurator> _logger;
    private readonly TwitchConfiguration _configuration;
    private readonly TwitchAPI _api;

    public string GetAuthorizationLink() => _api.Auth.GetAuthorizationCodeUrl(_configuration.RedirectUrl, _scopes);

    public async Task<AuthCodeResponse> GenerateTokenFromCodeAsync(string code)
    {
        var response = await _api.Auth.GetAccessTokenFromCodeAsync(
            code,
            _configuration.ClientSecret,
            _configuration.RedirectUrl
        ) ?? throw new Exception("Failed to exchange code for token. Twitch API returned null.");

        // TODO: Handle token storage here
        // response contains AccessToken, RefreshToken that should be stored in a secure place for future use
        // for simple case we can use just encrypted Json storage

        _api.Settings.AccessToken = response.AccessToken;

        _logger.LogInformation("Tokens generated successfully. Expires in: {Expires} seconds.", response.ExpiresIn);

        return response;
    }

    // May need some adjustments
    private readonly AuthScopes[] _scopes = [
        AuthScopes.Channel_Manage_Clips,
        AuthScopes.Channel_Manage_Polls,
        AuthScopes.Channel_Manage_Predictions,
        AuthScopes.Channel_Manage_Redemptions,
        AuthScopes.Channel_Moderate,
        AuthScopes.Channel_Read_Subscriptions,
        AuthScopes.Channel_Read_VIPs,
        AuthScopes.Chat_Edit,
        AuthScopes.Chat_Read,
        AuthScopes.Moderation_Read,
        AuthScopes.Moderator_Manage_Announcements,
        AuthScopes.Moderator_Manage_Banned_Users,
        AuthScopes.Moderator_Manage_Chat_Messages,
        AuthScopes.Moderator_Read_Banned_Users,
        AuthScopes.Moderator_Read_Chatters,
        AuthScopes.Moderator_Read_Chat_Messages,
        AuthScopes.Moderator_Read_Followers,
        AuthScopes.Moderator_Read_Moderators,
        AuthScopes.User_Manage_Whispers,
        AuthScopes.User_Read_Chat,
        AuthScopes.User_Write_Chat,
        AuthScopes.Whisper_Edit,
        AuthScopes.Whisper_Read
        ];

    public TwitchConfigurator(ILogger<TwitchConfigurator> logger, TwitchAPI api, IOptions<TwitchConfiguration> configuration)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _api = api;
        // TODO: correct tokens initialization 
        // Need to add refresh token handling later
        // Also chipered json data storage where will be accectoken and refresh token stored for future use
    }
}
