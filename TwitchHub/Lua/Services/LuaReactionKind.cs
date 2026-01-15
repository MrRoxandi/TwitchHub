namespace TwitchHub.Lua.Services;

public enum LuaReactionKind
{
    // ReactionKind:
    // - Twitch:
    Command, Reward, Message, Follow, Subscribe, GiftSubscribe, Cheers, Clip,
    // - Hardware:
    KeyDown, KeyUp, KeyType, MouseDown, MouseUp, MouseClick, MouseMove, MouseWheel,
    // - Media:
    OnMediaAdded, OnMediaStarted, OnMediaSkipped, OnMediaPaused, OnMediaStopped, OnMediaEndReached, QueueFinished, OnError 
}
