using System.Collections.Concurrent;
using System.Text.Json;

namespace TwitchHub.Services.Backends;

public sealed class LuaDataContainer : IAsyncDisposable, IDisposable
{
    private readonly string _dataFilePath;
    private readonly SemaphoreSlim _operationLock;
    private readonly ConcurrentDictionary<string, JsonElement> _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    private volatile bool _disposed;

    public LuaDataContainer(JsonSerializerOptions options, string? dataPath = null)
    {
        _dataFilePath = dataPath ?? Path.Combine(
            AppContext.BaseDirectory, "data", "data.json");
        _jsonOptions = options;
        _operationLock = new SemaphoreSlim(1, 1);
        _cache = [];

        EnsureDirectory();
        LoadSync();
    }

    public int Count => _cache.Count;
    public IEnumerable<string> Keys => _cache.Keys;

    // ================= GET =================

    public T? Get<T>(string key)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        if (!_cache.TryGetValue(key, out var stored))
            return default;

        try
        {
            return stored.Deserialize<T>(_jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error deserializing the key '{key}' to the type {typeof(T).Name}.", ex);
        }
    }
    public bool Contains(string key)
    {
        ValidateKey(key);
        return _cache.ContainsKey(key);
    }

    // ================= SET =================

    public void Set<T>(string key, T? value)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        if (value is null)
        {
            _ = _cache.TryRemove(key, out _);
            return;
        }

        try
        {
            var jsonElement = JsonSerializer.SerializeToElement(value, _jsonOptions);
            _cache[key] = jsonElement;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error serializing a value of type {typeof(T).Name} for the key '{key}'.", ex);
        }
    }

    // ================= REMOVE & CLEAR =================

    public bool Remove(string key)
    {
        ValidateKey(key);
        return _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        ThrowIfDisposed();
        _cache.Clear();
    }

    // ================= FILE OPERATIONS =================

    public async Task SaveAsync()
    {
        ThrowIfDisposed();
        await _operationLock.WaitAsync();
        try
        {
            await SaveToFileAsync(_dataFilePath);
        }
        finally
        {
            _ = _operationLock.Release();
        }
    }

    public async Task LoadAsync()
    {
        ThrowIfDisposed();
        await _operationLock.WaitAsync();
        try
        {
            await LoadFromFileAsync();
        }
        finally
        {
            _ = _operationLock.Release();
        }
    }

    public async Task BackupAsync(string? suffix = null)
    {
        suffix ??= $".backup-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
        ThrowIfDisposed();
        await _operationLock.WaitAsync();
        try
        {
            await SaveToFileAsync(_dataFilePath + suffix);
        }
        finally
        {
            _ = _operationLock.Release();
        }
    }

    // ================= PRIVATE - FILE I/O =================

    private void LoadSync()
    {
        try
        {
            LoadFromFileAsync().GetAwaiter().GetResult();
        }
        catch
        {

        }
    }

    private async Task LoadFromFileAsync()
    {
        if (!File.Exists(_dataFilePath))
            return;

        try
        {
            await using var stream = File.OpenRead(_dataFilePath);
            var data = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(
                stream, _jsonOptions);

            if (data is null)
                return;

            _cache.Clear();
            foreach (var (key, value) in data)
            {
                if (!string.IsNullOrWhiteSpace(key))
                    _ = _cache.TryAdd(key, value);
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid JSON in the file '{_dataFilePath}'.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Error reading the file '{_dataFilePath}'.", ex);
        }
    }

    private async Task SaveToFileAsync(string path)
    {
        if (_cache.IsEmpty)
        {
            TryDeleteFile(path);
            return;
        }

        var tempPath = path + ".tmp";
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            _ = Directory.CreateDirectory(directory);

        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, _cache, _jsonOptions);
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path, overwrite: true);
        }
        catch (IOException ex)
        {
            TryDeleteFile(tempPath);
            throw new InvalidOperationException(
                $"Error saving to '{path}'.", ex);
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {

        }
    }

    private void EnsureDirectory()
    {
        var dir = Path.GetDirectoryName(_dataFilePath);
        if (!string.IsNullOrEmpty(dir))
            _ = Directory.CreateDirectory(dir);
    }

    // ================= PRIVATE - VALIDATION & DISPOSAL =================

    private static void ValidateKey(string key) => ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, nameof(LuaDataContainer));

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            await SaveAsync();
        }
        catch
        {

        }

        _operationLock?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    ~LuaDataContainer()
    {
        Dispose();
    }
}