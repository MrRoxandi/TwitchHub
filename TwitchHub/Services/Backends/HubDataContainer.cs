using Lua;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchHub.Services.Backends;

public sealed class HubDataContainer : IAsyncDisposable, IDisposable
{
    private readonly string _dataFilePath;
    private readonly SemaphoreSlim _operationLock;
    private readonly ConcurrentDictionary<string, StoredValue> _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private volatile bool _disposed;

    public HubDataContainer(string? dataPath = null)
    {
        _dataFilePath = dataPath ?? Path.Combine(
            AppContext.BaseDirectory, ".data", "hub-data.json");

        _operationLock = new SemaphoreSlim(1, 1);
        _cache = new ConcurrentDictionary<string, StoredValue>();

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
            return stored.Value.Deserialize<T>(JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error deserializing the key '{key}' to the type {typeof(T).Name}.", ex);
        }
    }

    public LuaTable? GetLuaTable(string key)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        if (!_cache.TryGetValue(key, out var stored))
            return null;

        try
        {
            return JsonToLuaTable(stored.Value);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error converting the key '{key}' to the Lua table.", ex);
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
            var jsonElement = JsonSerializer.SerializeToElement(value, JsonOptions);
            _cache[key] = new StoredValue(jsonElement);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error serializing a value of type {typeof(T).Name} for the key '{key}'.", ex);
        }
    }

    public void SetLuaTable(string key, LuaTable? table)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        if (table is null)
        {
            _ = _cache.TryRemove(key, out _);
            return;
        }

        try
        {
            var jsonElement = LuaTableToJson(table);
            _cache[key] = new StoredValue(jsonElement);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error saving the Lua table for the key '{key}'.", ex);
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
                stream, JsonOptions);

            if (data is null)
                return;

            _cache.Clear();
            foreach (var (key, value) in data)
            {
                if (!string.IsNullOrWhiteSpace(key))
                    _ = _cache.TryAdd(key, new StoredValue(value));
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
            var data = _cache.ToDictionary(x => x.Key, x => x.Value.Value);

            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
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

    // ================= PRIVATE - LUA CONVERSION =================

    private static LuaTable JsonToLuaTable(JsonElement element)
    {
        var table = new LuaTable();

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                    table[prop.Name] = JsonToLuaValue(prop.Value);
                break;

            case JsonValueKind.Array:
                var index = 1;
                foreach (var item in element.EnumerateArray())
                    table[index++] = JsonToLuaValue(item);
                break;

            default:
                throw new InvalidOperationException(
                    $"JSON element of type {element.ValueKind} can't be a table");
        }

        return table;
    }

    private static LuaValue JsonToLuaValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => LuaValue.Nil,
        JsonValueKind.True => LuaValue.FromObject(true),
        JsonValueKind.False => LuaValue.FromObject(false),
        JsonValueKind.Number => ConvertJsonNumber(element),
        JsonValueKind.String => LuaValue.FromObject(element.GetString() ?? string.Empty),
        JsonValueKind.Array or JsonValueKind.Object =>
            LuaValue.FromObject(JsonToLuaTable(element)),
        _ => LuaValue.Nil
    };

    private static LuaValue ConvertJsonNumber(JsonElement element) => element.TryGetDouble(out var d)
            ? LuaValue.FromObject(d)
            : element.TryGetInt64(out var l) ? LuaValue.FromObject((double)l) : LuaValue.Nil;

    private static JsonElement LuaTableToJson(LuaTable table)
    {
        var isArray = IsArrayTable(table);

        if (isArray)
        {
            var list = new List<object?>();
            foreach (var value in table.GetArraySpan())
                list.Add(LuaValueToObject(value));
            return JsonSerializer.SerializeToElement(list, JsonOptions);
        }

        var dict = new Dictionary<string, object?>();
        foreach (var (key, value) in table)
        {
            var keyStr = key.Type == LuaValueType.String
                ? key.Read<string>() ?? string.Empty
                : key.ToString();
            dict[keyStr] = LuaValueToObject(value);
        }

        return JsonSerializer.SerializeToElement(dict, JsonOptions);
    }

    private static object? LuaValueToObject(LuaValue value) => value.Type switch
    {
        LuaValueType.Nil => null,
        LuaValueType.Boolean => value.Read<bool>(),
        LuaValueType.Number => value.Read<double>(),
        LuaValueType.String => value.Read<string>(),
        LuaValueType.Table => LuaTableToNestedObject(value.Read<LuaTable>()),
        _ => null
    };

    private static object? LuaTableToNestedObject(LuaTable table)
    {
        if (IsArrayTable(table))
        {
            var list = new List<object?>();
            foreach (var value in table.GetArraySpan())
                list.Add(LuaValueToObject(value));
            return list;
        }

        var dict = new Dictionary<string, object?>();
        foreach (var (key, value) in table)
        {
            var keyStr = key.Type == LuaValueType.String
                ? key.Read<string>() ?? string.Empty
                : key.ToString();
            dict[keyStr] = LuaValueToObject(value);
        }

        return dict;
    }

    private static bool IsArrayTable(LuaTable table) => table.ArrayLength > 0 && table.HashMapCount == 0;

    // ================= PRIVATE - VALIDATION & DISPOSAL =================

    private static void ValidateKey(string key) => ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, nameof(HubDataContainer));

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

    ~HubDataContainer()
    {
        Dispose();
    }

    // ================= INTERNAL TYPES =================

    private readonly record struct StoredValue(JsonElement Value);
}