using Lua;
using TwitchHub.Services.LuaMedia;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaMediaLib(
    ILogger<LuaMediaLib> logger,
    LuaMediaService service
    )
{
    private readonly ILogger<LuaMediaLib> _logger = logger;
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
    public void Add(string channel, string filepath)
    {
        _service.Add(channel, filepath);
        _logger.LogDebug("Add: {channel} -> {filepath}", channel, filepath);
    }

    [LuaMember("start")]
    public void Start(string channel)
    {
        _service.Start(channel);
        _logger.LogDebug("Start: {channel}", channel);
    }

    [LuaMember("stop")]
    public void Stop(string channel)
    {
        _service.Stop(channel);
        _logger.LogDebug("Stop: {channel}", channel);
    }

    [LuaMember("skip")]
    public void Skip(string channel)
    {
        _service.Skip(channel);
        _logger.LogDebug("Skip: {channel}", channel);
    }

    [LuaMember("pause")]
    public void Pause(string channel)
    {
        _service.Pause(channel);
        _logger.LogDebug("Pause: {channel}", channel);
    }

    [LuaMember("setvolume")]
    public void SetVolume(string channel, int volume)
    {
        _service.SetVolume(channel, volume);
        _logger.LogDebug("SetVolume: {channel} - {volume}", channel, volume);
    }

    [LuaMember("setspeed")]
    public void SetSpeed(string channel, float speed)
    {
        _service.SetSpeed(channel, speed);
        _logger.LogDebug("SetSpeed: {channel} - {speed}", channel, speed);
    }

    [LuaMember("getvolume")]
    public int GetVolume(string channel)
    {
        var result = _service.GetVolume(channel);
        _logger.LogDebug("GetVolume: {channel} -> {result}", channel, result);
        return result;
    }

    [LuaMember("getspeed")]
    public float GetSpeed(string channel)
    {
        var result = _service.GetSpeed(channel);
        _logger.LogDebug("GetSpeed: {channel} -> {result}", channel, result);
        return result;
    }

    [LuaMember("ispaused")]
    public bool IsPaused(string channel)
    {
        var result = _service.IsPaused(channel);
        _logger.LogDebug("IsPaused: {channel} -> {result}", channel, result);
        return result;
    }

    [LuaMember("isplaying")]
    public bool IsPlaying(string channel)
    {
        var result = _service.IsPlaying(channel);
        _logger.LogDebug("IsPlaying: {channel} -> {result}", channel, result);
        return result;
    }

    [LuaMember("isstopped")]
    public bool IsStopped(string channel)
    {
        var result = _service.IsStopped(channel);
        _logger.LogDebug("IsStopped: {channel} -> {result}", channel, result);
        return result;
    }
}
