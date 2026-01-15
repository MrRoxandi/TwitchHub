using Lua;
using System.Collections.Concurrent;

namespace TwitchHub.Lua.Services;

// CallResult Call(string name, ReactionKind kind)
//      CallResult -> bool Success, string? ErrorMessage, LuaValue Result
// CallResult Call(LuaReaction)
// Span<LuaReaction> Get(ReactionKind)
// LuaReaction Get(string name)
// ReactionKind:
// - Twitch: Command, Reward, Message, Follow, Subscribe, GiftSubscribe, Cheers, Clip
// - Hardware: KeyDown, KeyUp, KeyType, MouseDown, MouseUp, MouseClick, MouseMove, MouseWheel
// - Media: OnMediaAdded, OnMediaStarted, OnMediaSkipped, OnMediaPaused, OnMediaStopped, OnMediaEndReached, QueueFinished, OnError 
public sealed class LuaReactionsService(ILogger<LuaReactionsService> logger)
{
    private readonly ConcurrentDictionary<string, LuaReaction> _reactions = [];
    private readonly ILogger<LuaReactionsService> _logger = logger;
    public void RemoveReaction(string filePath)
    {
        var keyValue = Path.GetFileNameWithoutExtension(filePath);
        if (_reactions.TryRemove(keyValue, out var removed))
        {
            _logger.LogInformation("Removed reaction: {Name}", removed.Name);
        }
    }

    public void UpdateReaction(string filePath, LuaTable config, LuaState state)
    {
        try
        {
            var kind = config["kind"].Type switch
            {
                LuaValueType.String => Enum.Parse<LuaReactionKind>(config["kind"].Read<string>(), true),
                LuaValueType.Number => (LuaReactionKind)config["kind"].Read<int>(),
                _ => throw new ArgumentException($"Invalid 'kind' property on reaction in file: {filePath}")
            };

            if (config["oncall"].Type is not LuaValueType.Function)
            {
                throw new ArgumentException($"Invalid 'oncall' property on reaction in file: {filePath}");
            }

            if (config["onerror"].Type is not LuaValueType.Function)
            {
                throw new ArgumentException($"Invalid 'onerror' property on reaction in file: {filePath}");
            }

            var oncall = config["oncall"].Read<LuaFunction>();
            var onerror = config["onerror"].Read<LuaFunction>();
            var cooldown = 0L;
            if (config.ContainsKey("cooldown"))
            {
                cooldown = config["cooldown"].Read<long>();
            }

            var reaction = new LuaReaction(filePath, kind, state, oncall, onerror, cooldown);

            _reactions[Path.GetFileNameWithoutExtension(filePath)] = reaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map LuaTable to LuaReaction");
        }
    }

    public bool Contains(LuaReactionKind kind)
        => _reactions.Values.Any(v => v.Kind == kind);
    public bool Contains(string key) => _reactions.ContainsKey(key);
    public LuaReaction? Get(string key)
        => _reactions.TryGetValue(key, out var reaction) ? reaction : null;
    public IEnumerable<LuaReaction> Get(LuaReactionKind kind)
        => _reactions.Values.Where(v => v.Kind == kind);
    public async Task CallAsync(string key, LuaReactionKind kind, params LuaValue[] args)
    {
        try
        {
            var reaction = Get(key);
            if (reaction is not { })
            {
                _logger.LogWarning("Attepted to call not existing {name} reaction", key);
                return;
            }

            if (reaction.Kind.Equals(kind) is false)
            {
                _logger.LogWarning("Attepted to call {name} reaction with invalid kind {expected} != {actual}", key, reaction.Kind, kind);
                return;
            }

            var result = await reaction.CallAsync(args);
            if (!result.Success)
            {
                _logger.LogInformation("Call to {name} reaction failed due: {message}", key, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call reaction: {name}", key);
        }
    }

    public async Task CallAsync(LuaReactionKind kind, params LuaValue[] args)
    {
        var reactions = Get(kind);
        foreach (var reaction in reactions)
        {
            try
            {
                var result = await reaction.CallAsync(args);
                if (!result.Success)
                {
                    _logger.LogInformation("Call to {name} reaction failed due: {message}", reaction.Name, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call reaction: {name}", reaction.Name);
            }
        }
    }
}
