using System.Text.Json.Serialization;
using TwitchLib.Api.Auth;

namespace TwitchHub.Services.Twitch.Data;

public sealed class TwitchTokenStore
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    [JsonIgnore]
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(-1);

    public TwitchTokenStore() { }

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
