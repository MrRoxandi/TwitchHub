using Lua;
using TwitchHub.Services.LuaMedia;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaMediaLib(LuaMediaService service)
{
    private readonly LuaMediaService _service = service;

    [LuaMember]
    private LuaTable GetChannels()
    {
        var table = new LuaTable();
        foreach (var (idx, key) in _service.Channels.Index())
        {
            table[idx + 1] = key.Name;
        }

        return table;
    }

    [LuaMember]
    public void Add(string channel, string filepath) => _service.Add(channel, filepath);
    [LuaMember]
    public void Start(string channel) => _service.Start(channel);
    [LuaMember]
    public void Stop(string channel) => _service.Stop(channel);
    [LuaMember]
    public void Skip(string channel) => _service.Skip(channel);
    [LuaMember]
    public void Pause(string channel) => _service.Pause(channel);
    [LuaMember]
    public void SetVolume(string channel, int volume) => _service.SetVolume(channel, volume);
    [LuaMember]
    public void SetSpeed(string channel, float speed) => _service.SetSpeed(channel, speed);
    [LuaMember]
    public int GetVolume(string channel) => _service.GetVolume(channel);
    [LuaMember]
    public float GetSpeed(string channel) => _service.GetSpeed(channel);

}
