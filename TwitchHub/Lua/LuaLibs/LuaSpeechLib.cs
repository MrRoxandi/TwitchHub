using System.Runtime.Versioning;
using Lua;
using TwitchHub.Services.LuaMedia;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
[SupportedOSPlatform("windows")]
public sealed partial class LuaSpeechLib(LuaMediaService service)
{
    private readonly LuaMediaService _service = service;

    [LuaMember("speak")]
    public async Task SpeakAsync(string text) => await _service.SpeechSpeakAsync(text);
    [LuaMember("pause")]
    public void Pause() => _service.SpeechPause();
    [LuaMember("resume")]
    public void Resume() => _service.SpeechResume();
    [LuaMember("stop")]
    public void Stop() => _service.SpeechStop();
    [LuaMember("clear")]
    public void Clear() => _service.SpeechClear();
    [LuaMember("skip")]
    public void Skip() => _service.SpeechSkip();
    [LuaMember("setvolume")]
    public void SetVolume(int volume) => _service.SpeechSetVolume(volume);
    [LuaMember("getvolume")]
    public int GetVolume() => _service.SpeechGetVolume();
    [LuaMember("setrate")]
    public void SetRate(int rate) => _service.SpeechSetRate(rate);
    [LuaMember("getrate")]
    public int GetRate() => _service.SpeechGetRate();
    [LuaMember("voices")]
    public LuaTable GetVoices()
    {
        var table = new LuaTable();
        foreach (var (idx, voice) in _service.SpeechGetVoices().Index())
        {
            table[idx + 1] = voice;
        }
        return table;
    }

    [LuaMember("selectvoice")]
    public void SelectVoice(string voice) => _service.SpeechSelectVoice(voice);
    [LuaMember("reloadbwords")]
    public void ReloadBannedWords() => _service.SpeechReloadBannedWords();
}
