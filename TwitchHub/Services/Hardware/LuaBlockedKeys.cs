using SharpHook.Data;
using System.Collections.Concurrent;

namespace TwitchHub.Services.Hardware;

public sealed class LuaBlockedKeys
{
    private readonly ConcurrentDictionary<KeyCode, bool> _keys = [];
    private readonly ConcurrentDictionary<MouseButton, bool> _buttons = [];

    public IEnumerable<KeyCode> BlockedKeys => _keys
        .Where(kvp => kvp.Value)
        .Select(kvp => kvp.Key);

    public bool IsBlocked(KeyCode keyCode) => _keys.TryGetValue(keyCode, out var v) && v;
    public void ToggleBlock(KeyCode code) => _keys[code] = !(_keys.TryGetValue(code, out var v) && v);
    public void Block(KeyCode code) => _keys[code] = true;
    public void Unblock(KeyCode code) => _keys[code] = false;

    public bool IsBlocked(MouseButton code) => _buttons.TryGetValue(code, out var v) && v;
    public void ToggleBlock(MouseButton code) => _buttons[code] = !(_buttons.TryGetValue(code, out var v) && v);
    public void Block(MouseButton code) => _buttons[code] = true;
    public void Unblock(MouseButton code) => _buttons[code] = false;

}
