using SharpHook.Data;

namespace TwitchHub.Services.Hardware;

public sealed class LuaBlockedKeys
{
    private readonly Dictionary<KeyCode, bool> _keys = [];
    private readonly Dictionary<MouseButton, bool> _buttons = [];

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
