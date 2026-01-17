using Lua;
using SharpHook;
using SharpHook.Data;
using TwitchHub.Services.Hardware;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaHardwareLib
{
    private readonly LuaBlockedKeys _luaBlocked;

    private static readonly Dictionary<string, KeyCode> _keyboardMap =
        Enum.GetValues<KeyCode>()
            .ToDictionary(k => k.ToString()[2..], StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, MouseButton> _mouseMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["l"] = MouseButton.Button1,
            ["left"] = MouseButton.Button1,

            ["r"] = MouseButton.Button2,
            ["right"] = MouseButton.Button2,

            ["m"] = MouseButton.Button3,
            ["mid"] = MouseButton.Button3,
            ["middle"] = MouseButton.Button3,
        };

    private readonly EventSimulator _simulator = new();
    private readonly ILogger<LuaHardwareLib> _logger;

    private readonly int _minKey;
    private readonly int _maxKey;

    [LuaMember("keycodes")]
    public readonly LuaTable KeyCodes;

    public LuaHardwareLib(ILogger<LuaHardwareLib> logger, LuaBlockedKeys lbk)
    {
        _logger = logger;
        _luaBlocked = lbk;
        _minKey = (int)_keyboardMap.Values.Min();
        _maxKey = (int)_keyboardMap.Values.Max();

        KeyCodes = new LuaTable();
        foreach (var (name, code) in _keyboardMap)
        {
            KeyCodes[name] = (int)code;
            KeyCodes[name.ToLowerInvariant()] = (int)code;
        }
    }

    // ================= KEYBOARD =================

    [LuaMember("parsekeycode")]
    public int ParseKeyCode(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (int)KeyCode.VcUndefined;

        var span = key.AsSpan().Trim();
        if (span.StartsWith("Vc", StringComparison.OrdinalIgnoreCase))
            span = span[2..].Trim();

        return _keyboardMap.TryGetValue(span.ToString(), out var code)
            ? (int)code
            : (int)KeyCode.VcUndefined;
    }
    [LuaMember("keycodetostring")]
    public string KeyCodeToString(int keycode)
    {
        var code = NormalizeKey(keycode);
        return code.ToString();
    }

    [LuaMember("keydown")]
    public void KeyDown(int keyCode)
        => SimKey(keyCode, _simulator.SimulateKeyPress, "KeyDown");

    [LuaMember("keyup")]
    public void KeyUp(int keyCode)
        => SimKey(keyCode, _simulator.SimulateKeyRelease, "KeyUp");

    [LuaMember("keytap")]
    public void KeyTap(int keyCode)
        => SimKey(keyCode, k => _simulator.SimulateKeyStroke([k]), "KeyTap");

    [LuaMember("keyhold")]
    public async Task KeyHold(int keyCode, int durationMs)
    {
        if (durationMs < 300)
        {
            KeyTap(keyCode);
            return;
        }

        var key = NormalizeKey(keyCode);
        var end = Environment.TickCount64 + durationMs;

        while (Environment.TickCount64 < end)
        {
            _ = _simulator.SimulateKeyPress(key);
            await Task.Delay(100);
            _ = _simulator.SimulateKeyRelease(key);
        }

        _logger.LogDebug("KeyHold ({key}) {duration}ms", key, durationMs);
    }

    [LuaMember("typetext")]
    public void TypeText(string text)
    {
        var result = _simulator.SimulateTextEntry(text);
        _logger.LogDebug("TypeText ({text}): {result}", text, result);
    }

    [LuaMember("keyisblocked")]
    public bool KeyIsBlocked(int keyCode)
    {
        var code = NormalizeKey(keyCode);
        return _luaBlocked.IsBlocked(code);
    }

    [LuaMember("keyblock")]
    public void KeyBlock(int keyCode)
    {
        var code = NormalizeKey(keyCode);
        _luaBlocked.Block(code);
    }

    [LuaMember("keyunblock")]
    public void KeyUnBlock(int keyCode)
    {
        var code = NormalizeKey(keyCode);
        _luaBlocked.Unblock(code);
    }

    [LuaMember("keytoggle")]
    public void KeyToggle(int keyCode)
    {
        var code = NormalizeKey(keyCode);
        _luaBlocked.ToggleBlock(code);
    }

    // ================= MOUSE =================

    [LuaMember("parsemousebutton")]
    public int ParseMouseButton(string button)
        => _mouseMap.TryGetValue(button?.Trim() ?? "", out var b)
            ? (int)b
            : (int)MouseButton.NoButton;
    [LuaMember("buttontostring")]
    public string ButtonToString(int button)
    {
        var code = NormalizeButton(button);
        return code.ToString();
    }

    [LuaMember("mousedown")]
    public void MouseDown(int buttonCode)
        => SimMouse(buttonCode, _simulator.SimulateMousePress, "MouseDown");

    [LuaMember("mouseup")]
    public void MouseUp(int buttonCode)
        => SimMouse(buttonCode, _simulator.SimulateMouseRelease, "MouseUp");

    [LuaMember("mouseclick")]
    public void MouseClick(int buttonCode)
    {
        var button = NormalizeButton(buttonCode);
        var result = _simulator.Sequence()
            .AddMousePress(button)
            .AddMouseRelease(button)
            .Simulate();

        _logger.LogDebug("MouseClick ({button}): {result}", button, result);
    }

    [LuaMember("mousehold")]
    public async Task MouseHold(int buttonCode, int durationMs)
    {
        if (durationMs < 300)
        {
            MouseClick(buttonCode);
            return;
        }

        var button = NormalizeButton(buttonCode);
        var end = Environment.TickCount64 + durationMs;

        while (Environment.TickCount64 < end)
        {
            _ = _simulator.SimulateMousePress(button);
            await Task.Delay(100);
            _ = _simulator.SimulateMouseRelease(button);
        }

        _logger.LogDebug("MouseHold ({button}) {duration}ms", button, durationMs);
    }

    [LuaMember("scrollvertical")]
    public void ScrollVertical(int delta)
        => _simulator.SimulateMouseWheel((short)delta, MouseWheelScrollDirection.Vertical);

    [LuaMember("scrollhorizontal")]
    public void ScrollHorizontal(int delta)
        => _simulator.SimulateMouseWheel((short)delta, MouseWheelScrollDirection.Horizontal);

    [LuaMember("setmouseposition")]
    public void SetMousePosition(int x, int y)
        => _simulator.SimulateMouseMovement((short)x, (short)y);

    [LuaMember("movemouse")]
    public void MoveMouse(int dx, int dy)
        => _simulator.SimulateMouseMovementRelative((short)dx, (short)dy);

    [LuaMember("buttonisblocked")]
    public bool ButtonIsBlocked(int keyCode)
    {
        var code = NormalizeButton(keyCode);
        return _luaBlocked.IsBlocked(code);
    }

    [LuaMember("buttonblock")]
    public void ButtonBlock(int keyCode)
    {
        var code = NormalizeButton(keyCode);
        _luaBlocked.Block(code);
    }

    [LuaMember("buttonunblock")]
    public void ButtonUnBlock(int keyCode)
    {
        var code = NormalizeButton(keyCode);
        _luaBlocked.Unblock(code);
    }

    [LuaMember("buttontoggle")]
    public void ButtonToggle(int keyCode)
    {
        var code = NormalizeKey(keyCode);
        _luaBlocked.ToggleBlock(code);
    }

    // ================= HELPERS =================

    private KeyCode NormalizeKey(int code)
        => code is < 0 or > int.MaxValue || code < _minKey || code > _maxKey
            ? KeyCode.VcUndefined
            : (KeyCode)code;

    private MouseButton NormalizeButton(int code)
        => code is < 1 or > 5
            ? MouseButton.NoButton
            : (MouseButton)code;

    private void SimKey(int raw, Func<KeyCode, UioHookResult> action, string name)
    {
        var key = NormalizeKey(raw);
        var result = action(key);
        _logger.LogDebug("{name} ({key}): {result}", name, key, result);
    }

    private void SimMouse(int raw, Func<MouseButton, UioHookResult> action, string name)
    {
        var button = NormalizeButton(raw);
        var result = action(button);
        _logger.LogDebug("{name} ({button}): {result}", name, button, result);
    }
}
