using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace TwitchHub.Services.Twitch.Data;

public sealed class FileTwitchTokenStorage(
    IDataProtectionProvider provider,
    IWebHostEnvironment env
    )
{
    private readonly IDataProtector _protector = provider.CreateProtector("twitch.tokens.v1");
    private readonly string _path = Path.Combine(env.ContentRootPath, "data", "tokens.dat");

    public async Task SaveAsync(TwitchTokenStore tokens, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(tokens);
        var protectedData = _protector.Protect(json);
        await File.WriteAllTextAsync(_path, protectedData, ct);
    }

    public async Task<TwitchTokenStore?> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
            return null;

        try
        {
            var protectedData = await File.ReadAllTextAsync(_path, ct);
            var json = _protector.Unprotect(protectedData);
            return JsonSerializer.Deserialize<TwitchTokenStore>(json);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearAsync(CancellationToken ct)
    {
        if (File.Exists(_path))
            File.Delete(_path);

        return Task.CompletedTask;
    }
}
