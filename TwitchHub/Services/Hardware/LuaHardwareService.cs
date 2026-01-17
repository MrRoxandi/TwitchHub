
using SharpHook;
using TwitchHub.Lua.Services;

namespace TwitchHub.Services.Hardware;

public sealed class LuaHardwareService : IHostedService, IDisposable
{
    private readonly SimpleGlobalHook _hook;
    private readonly LuaReactionsService _reactions;
    private readonly ILogger<LuaHardwareService> _logger;
    private readonly LuaBlockedKeys _blockedKeys;
    public bool IsRunning => _hook.IsRunning;
    public bool IsDisposed => _hook.IsDisposed;
    public LuaHardwareService(
        ILogger<LuaHardwareService> logger,
        LuaReactionsService reactions,
        LuaBlockedKeys blockedKeys
        )
    {
        _hook = new SimpleGlobalHook();
        _logger = logger;
        _reactions = reactions;
        _blockedKeys = blockedKeys;
        HookEvents();
    }

    private void KeyDown(object? sender, KeyboardHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.KeyCode))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.KeyDown, (int)e.Data.KeyCode));
    }

    private void KeyUp(object? sender, KeyboardHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.KeyCode))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.KeyUp, (int)e.Data.KeyCode));
    }
    private void KeyTyped(object? sender, KeyboardHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.KeyCode))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.KeyType, (int)e.Data.KeyCode));
    }
    private void MousePressed(object? sender, MouseHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.Button))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.MouseDown, (int)e.Data.Button));
    }

    private void MouseReleased(object? sender, MouseHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.Button))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.MouseUp, (int)e.Data.Button));
    }
    private void MouseClicked(object? sender, MouseHookEventArgs e)
    {
        if (_blockedKeys.IsBlocked(e.Data.Button))
        {
            e.SuppressEvent = true;
        }

        _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.MouseClick, (int)e.Data.Button));
    }

    private void MouseMoved(object? sender, MouseHookEventArgs e) => _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.MouseMove, e.Data.X, e.Data.Y));
    private void MouseWheel(object? sender, MouseWheelHookEventArgs e) => _ = Task.Run(async () => await _reactions.CallAsync(LuaReactionKind.MouseWheel, e.Data.Delta, e.Data.Direction.ToString()));
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(_hook.RunAsync, cancellationToken);
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _hook.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        _hook.Dispose();
        UnhookEvents();
    }

    // Internal

    private void HookEvents()
    {
        _hook.KeyReleased += KeyUp;
        _hook.KeyPressed += KeyDown;
        _hook.KeyTyped += KeyTyped;
        _hook.MousePressed += MousePressed;
        _hook.MouseReleased += MouseReleased;
        _hook.MouseClicked += MouseClicked;
        _hook.MouseMoved += MouseMoved;
        _hook.MouseWheel += MouseWheel;
    }

    private void UnhookEvents()
    {
        _hook.KeyReleased -= KeyUp;
        _hook.KeyPressed -= KeyDown;
        _hook.KeyTyped -= KeyTyped;
        _hook.MousePressed -= MousePressed;
        _hook.MouseReleased -= MouseReleased;
        _hook.MouseClicked -= MouseClicked;
        _hook.MouseMoved -= MouseMoved;
        _hook.MouseWheel -= MouseWheel;
    }
}
