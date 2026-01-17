using Lua;
using TwitchHub.Services.LuaMedia;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaMediaLib(LuaMediaService service)
{
    private readonly LuaMediaService _service = service;

    [LuaMember("channels")]
    private LuaTable Channels()
    {
        var table = new LuaTable();
        foreach (var (idx, key) in _service.Channels.Index())
        {
            table[idx + 1] = key.Name;
        }

        return table;
    }

    [LuaMember("add")]
    public void Add(string channel, string filepath) => _service.Add(channel, filepath);
    [LuaMember("start")]
    public void Start(string channel) => _service.Start(channel);
    [LuaMember("stop")]
    public void Stop(string channel) => _service.Stop(channel);
    [LuaMember("skip")]
    public void Skip(string channel) => _service.Skip(channel);
    [LuaMember("pause")]
    public void Pause(string channel) => _service.Pause(channel);
    [LuaMember("setvolume")]
    public void SetVolume(string channel, int volume) => _service.SetVolume(channel, volume);
    [LuaMember("setspeed")]
    public void SetSpeed(string channel, float speed) => _service.SetSpeed(channel, speed);
    [LuaMember("getvolume")]
    public int GetVolume(string channel) => _service.GetVolume(channel);
    [LuaMember("getspeed")]
    public float GetSpeed(string channel) => _service.GetSpeed(channel);
    [LuaMember("ispaused")]
    public bool IsPaused(string channel) => _service.IsPaused(channel);
    [LuaMember("isplaying")]
    public bool IsPlaying(string channel) => _service.IsPlaying(channel);
    [LuaMember("isstopped")]
    public bool IsStopped(string channel) => _service.IsStopped(channel);

}
