namespace TwitchHub.Lua.Services;

public enum LuaReactionKind
{
    None,
    // ReactionKind:
    // - Twitch:
    Command, Reward, Message, Follow, Subscribe, GiftSubscribe, Cheer, StreamOn, StreamOff, Clip,
    // - Hardware:
    KeyDown, KeyUp, KeyType, MouseDown, MouseUp, MouseClick, MouseMove, MouseWheel,
    // - Media:
    MediaAdd, MediaStart, MediaSkip, MediaPause, MediaStop, MediaEnd, MediaQueueFinish, MediaError 
}
