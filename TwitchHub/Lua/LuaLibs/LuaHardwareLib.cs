using Lua;
using SharpHook;
using SharpHook.Data;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaHardwareLib
{
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

    [LuaMember]
    public readonly LuaTable KeyCodes;

    public LuaHardwareLib(ILogger<LuaHardwareLib> logger)
    {
        _logger = logger;

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

    [LuaMember]
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

    [LuaMember]
    public void KeyDown(int keyCode)
        => SimKey(keyCode, _simulator.SimulateKeyPress, "KeyDown");

    [LuaMember]
    public void KeyUp(int keyCode)
        => SimKey(keyCode, _simulator.SimulateKeyRelease, "KeyUp");

    [LuaMember]
    public void KeyTap(int keyCode)
        => SimKey(keyCode, k => _simulator.SimulateKeyStroke([k]), "KeyTap");

    [LuaMember]
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

    [LuaMember]
    public void TypeText(string text)
    {
        var result = _simulator.SimulateTextEntry(text);
        _logger.LogDebug("TypeText ({text}): {result}", text, result);
    }

    // ================= MOUSE =================

    [LuaMember]
    public int ParseMouseButton(string button)
        => _mouseMap.TryGetValue(button?.Trim() ?? "", out var b)
            ? (int)b
            : (int)MouseButton.NoButton;

    [LuaMember]
    public void MouseDown(int buttonCode)
        => SimMouse(buttonCode, _simulator.SimulateMousePress, "MouseDown");

    [LuaMember]
    public void MouseUp(int buttonCode)
        => SimMouse(buttonCode, _simulator.SimulateMouseRelease, "MouseUp");

    [LuaMember]
    public void MouseClick(int buttonCode)
    {
        var button = NormalizeButton(buttonCode);
        var result = _simulator.Sequence()
            .AddMousePress(button)
            .AddMouseRelease(button)
            .Simulate();

        _logger.LogDebug("MouseClick ({button}): {result}", button, result);
    }

    [LuaMember]
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

    [LuaMember]
    public void ScrollVertical(int delta)
        => _simulator.SimulateMouseWheel((short)delta, MouseWheelScrollDirection.Vertical);

    [LuaMember]
    public void ScrollHorizontal(int delta)
        => _simulator.SimulateMouseWheel((short)delta, MouseWheelScrollDirection.Horizontal);

    [LuaMember]
    public void SetMousePosition(int x, int y)
        => _simulator.SimulateMouseMovement((short)x, (short)y);

    [LuaMember]
    public void MoveMouse(int dx, int dy)
        => _simulator.SimulateMouseMovementRelative((short)dx, (short)dy);

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
