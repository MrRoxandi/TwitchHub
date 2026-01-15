using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchLib.Api;

namespace TwitchHub.Services.Twitch.Data;

public sealed class TwitchTokenProvider(
    FileTwitchTokenStorage ftts,
    ILogger<TwitchTokenProvider> logger,
    IOptions<TwitchConfiguration> config,
    TwitchAPI api)
{
    private readonly FileTwitchTokenStorage _fileStorage = ftts;
    private readonly ILogger<TwitchTokenProvider> _logger = logger;
    private readonly TwitchConfiguration _config = config.Value;
    private readonly TwitchAPI _api = api;

    public event Func<string, Task>? OnTokenRefreshed;
    private TwitchTokenStore? _store;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public bool HasTokens => _store is not null;
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_store is null)
            {
                _store = await _fileStorage.LoadAsync(ct);
                if (_store is not null)
                {
                    _api.Settings.AccessToken = _store.AccessToken;
                }
            }

            return _store is null ? null : !_store.IsExpired ? _store.AccessToken : await RefreshInternalAsync(ct);
        }
        finally
        {
            _ = _lock.Release();
        }
    }

    public async Task<bool> ForceRefreshAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return await RefreshInternalAsync(ct) != null;
        }
        finally
        {
            _ = _lock.Release();
        }
    }

    private async Task<string?> RefreshInternalAsync(CancellationToken ct)
    {
        if (_store?.RefreshToken is null)
            return null;

        try
        {
            var response = await _api.Auth.RefreshAuthTokenAsync(
                _store.RefreshToken,
                _config.ClientSecret,
                _config.ClientId
            );

            if (response == null)
            {
                _logger.LogError("Twitch API returned null on token refresh.");
                return null;
            }

            _store.Update(response);
            await _fileStorage.SaveAsync(_store, ct);

            _api.Settings.AccessToken = _store.AccessToken;

            _logger.LogInformation("Token refreshed successfully.");

            _ = OnTokenRefreshed?.Invoke(_store.AccessToken);

            return _store.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token.");
            return null;
        }
    }
    public void SetNewToken(TwitchTokenStore newStore)
    {
        _store = newStore;
        _api.Settings.AccessToken = newStore.AccessToken;
        _ = (OnTokenRefreshed?.Invoke(newStore.AccessToken));
    }
}
