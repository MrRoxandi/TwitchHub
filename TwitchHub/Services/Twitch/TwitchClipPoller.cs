using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchHub.Configurations;
using TwitchHub.Lua.Services;
using TwitchHub.Services.Backends.Data;
using TwitchHub.Services.Backends.Entities;
using TwitchHub.Services.Twitch.Data;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace TwitchHub.Services.Twitch;

public sealed class TwitchClipPoller(
    TwitchAPI api,
    TwitchTokenProvider tokenProvider,
    LuaReactionsService luaReactions,
    IOptions<TwitchConfiguration> config,
    IDbContextFactory<TwitchClipsDbContext> dbContextFactory,
    ILogger<TwitchClipPoller> logger) : BackgroundService
{
    private readonly TwitchAPI _api = api;
    private readonly TwitchTokenProvider _tokenProvider = tokenProvider;
    private readonly LuaReactionsService _luaReactions = luaReactions;
    private readonly ILogger<TwitchClipPoller> _logger = logger;
    private readonly TwitchConfiguration _config = config.Value;
    private readonly IDbContextFactory<TwitchClipsDbContext> _dbContextFactory = dbContextFactory;

    private string? _broadcasterId;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);

        _logger.LogInformation("Twitch Clip Poller started.");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_config.ClipsPollingIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckForNewClipsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling for clips.");
            }
        }
    }

    private async Task CheckForNewClipsAsync(CancellationToken ct)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        if (string.IsNullOrEmpty(_broadcasterId))
        {
            var users = await _api.Helix.Users.GetUsersAsync(logins: [_config.Channel]);
            _broadcasterId = users.Users.FirstOrDefault()?.Id;
            if (_broadcasterId == null)
            {
                return;
            }
        }

        using var dbContext = _dbContextFactory.CreateDbContext();

        var lastClipTime = await dbContext.TwitchClips
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var startedAt = lastClipTime == default
            ? DateTime.Now.AddYears(-2)
            : lastClipTime.AddSeconds(1);

        GetClipsResponse? response = null;
        string? cursor = null;
        do
        {
            response = await _api.Helix.Clips.GetClipsAsync(
                broadcasterId: _broadcasterId,
                startedAt: startedAt,
                endedAt: DateTime.UtcNow,
                first: 100,
                after: cursor
            );
            if (response.Clips == null || response.Clips.Length == 0)
            {
                return;
            }

            foreach (var clip in response.Clips)
            {
                var exists = await dbContext.TwitchClips.AnyAsync(c => c.ClipId == clip.Id, ct);
                if (exists)
                {
                    continue;
                }

                var entity = new TwitchClipEntity
                {
                    ClipId = clip.Id,
                    UserId = clip.CreatorId,
                    ChannelId = clip.BroadcasterId,
                    Title = clip.Title,
                    CreatedAt = DateTime.Parse(clip.CreatedAt)
                };

                _ = dbContext.TwitchClips.Add(entity);

                _logger.LogInformation("New Clip Detected: {Title} by {Creator}", clip.Title, clip.CreatorName);

                await _luaReactions.CallAsync(LuaReactionKind.Clip,
                    clip.Id,
                    clip.Url,
                    clip.Title,
                    clip.CreatorId,
                    clip.CreatorName,
                    clip.Duration);
            }

            cursor = response.Pagination.Cursor;
        } while (!string.IsNullOrEmpty(response.Pagination.Cursor));

        _ = await dbContext.SaveChangesAsync(ct);
    }
}
