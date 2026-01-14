using TwitchLib.Api.Auth;

namespace TwitchHub.Services.Twitch.Data;

public sealed class TwitchTokenStore
{
    public string AccessToken { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(-1);

    private TwitchTokenStore() { }

    public static TwitchTokenStore From(AuthCodeResponse response)
        => new()
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn)
        };

    public void Update(RefreshResponse response)
    {
        AccessToken = response.AccessToken;
        RefreshToken = response.RefreshToken;
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
    }
}
