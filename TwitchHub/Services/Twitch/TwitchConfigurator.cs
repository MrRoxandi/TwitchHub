using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchHub.Services.Twitch.Data;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;

namespace TwitchHub.Services.Twitch;

public class TwitchConfigurator(
    ILogger<TwitchConfigurator> logger,
    IOptions<TwitchConfiguration> configuration,
    FileTwitchTokenStorage ftts,
    TwitchTokenProvider provider,
    TwitchAPI api)
{

    private readonly TwitchConfiguration _configuration = configuration.Value;
    private readonly ILogger<TwitchConfigurator> _logger = logger;
    private readonly FileTwitchTokenStorage _fileStorage = ftts;
    private readonly TwitchTokenProvider _tokenProvider = provider;
    private readonly TwitchAPI _api = api;
    public string GetAuthorizationLink() => _api.Auth.GetAuthorizationCodeUrl(_configuration.RedirectUrl, _scopes);

    public async Task CompleteAuthorizationAsync(string code, CancellationToken ct = default)
    {
        var response = await _api.Auth.GetAccessTokenFromCodeAsync(
            code,
            _configuration.ClientSecret,
            _configuration.RedirectUrl,
            _configuration.ClientId
        ) ?? throw new Exception("Failed to exchange code for token. Twitch API returned null.");

        var store = TwitchTokenStore.From(response);

        _tokenProvider.SetNewToken(store);
        await _fileStorage.SaveAsync(store, ct);

        _logger.LogInformation("Tokens generated successfully. Expires in: {Expires} seconds.", response.ExpiresIn);
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
        AuthScopes.Whisper_Read,
        AuthScopes.Bits_Read,
        ];
}
