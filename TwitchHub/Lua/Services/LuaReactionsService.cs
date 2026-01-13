namespace TwitchHub.Lua.Services;

// will parse files in configs/reactions/*.lua with coresponding method
// state will be provided by LSM
// will store all parse results in ConcurentDictionary<string, LuaReaction>
// PraseResult ParseAsync(LuaState, string filePath)
//      ParseResult -> bool Success, string? ErrorMessage 
// CallResult Call(string name, ReactionKind kind)
//      CallResult -> bool Success, string? ErrorMessage, LuaValue Result
// CallResult Call(LuaReaction)
// Span<LuaReaction> Get(ReactionKind)
// LuaReaction Get(string name)
// ReactionKind:
// - Twitch: Command, Reward, Message, Follow, Subscribe, GiftSubscribe, Cheers, Clip
// - Hardware: KeyDown, KeyUp, KeyType, MouseDown, MouseUp, MouseClick, MouseMove, MouseWheel
// - Media: OnMediaAdded, OnMediaStarted, OnMediaSkipped, OnMediaPaused, OnMediaStopped, OnMediaEndReached, QueueFinished, OnError 
public sealed class LuaReactionsService
{

}
