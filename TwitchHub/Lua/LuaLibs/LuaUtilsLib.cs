using Lua;
using TwitchHub.Services.Backends;

namespace TwitchHub.Lua.LuaLibs;

[LuaObject]
public sealed partial class LuaUtilsLib
{
    private readonly Random _random = Random.Shared;

    public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    // ================= RANDOM =================

    [LuaMember]
    public LuaValue RandomNumber(int min, int max) => _random.Next(min, max);

    [LuaMember]
    public LuaValue RandomDouble(double min, double max) => ((max - min) * _random.NextDouble()) + min;
    [LuaMember]
    public LuaValue RandomString(int length) => length > 0
        ? new string(_random.GetItems(Chars.AsSpan(), length))
        : string.Empty;

    [LuaMember]
    public LuaValue RandomPosition(int minx, int maxx, int miny, int maxy)
        => new LuaTable
        {
            ['X'] = RandomNumber(minx, maxx),
            ['Y'] = RandomNumber(miny, maxy)
        };
    // ================= OTHER UTILS =================

    [LuaMember]
    public async Task Delay(int delay) => await Task.Delay(TimeSpan.FromMilliseconds(delay));

    // ================= LUA TABLES UTILS =================

    [LuaMember]
    public bool IsLuaArray(LuaTable table) => table.ArrayLength != 0 && table.HashMapCount == 0;
    [LuaMember]
    public bool IsTableEmpty(LuaTable table) => table.ArrayLength == 0 && table.HashMapCount == 0;

    [LuaMember]
    public bool TableContains(LuaTable table, LuaValue value)
    {
        if (IsLuaArray(table))
        {
            foreach (var v in table.GetArraySpan())
            {
                if (v == value)
                    return true;
            }
        }

        foreach (var (_, v) in table)
        {
            if (v == value)
                return true;
        }

        return false;
    }
    [LuaMember]
    public LuaValue TableRandom(LuaTable table)
    {
        if (IsTableEmpty(table))
            return LuaValue.Nil;
        if (IsLuaArray(table))
        {
            var span = table.GetArraySpan();
            return span[_random.Next(span.Length)];
        }

        var index = _random.Next(table.Count());
        return table.ElementAt(index).Value;
    }
    [LuaMember]
    public LuaTable TableCopy(LuaTable table)
    {
        var result = new LuaTable(table.ArrayLength, table.HashMapCount);
        foreach (var (k, v) in table)
        {
            result[k] = v;
        }

        return result;
    }

    [LuaMember]
    public LuaValue TableShuffle(LuaTable table)
    {
        var result = TableCopy(table);
        _random.Shuffle(result.GetArraySpan());
        return result;
    }

    [LuaMember]
    public string TableJoin(LuaTable table, string sep = ", ")
        => IsLuaArray(table)
            ? string.Join(sep, table.Select(e => e.Value.ToString()))
            : string.Join(sep, table.Select(e => $"[{e.Key}]: {e.Value}"));

    [LuaMember]
    public string TableToJson(LuaTable table)
        => LuaJsonConverter.ToJson(table)?.ToJsonString() ?? string.Empty;

    // ================= DATETIME UTILS =================

    [LuaMember]
    public LuaValue GetCurrentTime() => DateTime.Now.Ticks;

    [LuaMember]
    public LuaValue GetCurrentTimeUtc() => DateTime.UtcNow.Ticks;

    [LuaMember]
    public LuaValue GetCurrentTimeOffset() => DateTimeOffset.Now.Ticks;

    [LuaMember]
    public LuaValue GetCurrentTimeOffsetUtc() => DateTimeOffset.UtcNow.Ticks;

    [LuaMember]
    public string FormatDateTime(long ticks, string format = "yyyy-MM-dd HH:mm:ss")
    {
        try
        {
            return new DateTime(ticks).ToString(format);
        }
        catch
        {
            return string.Empty;
        }
    }

    [LuaMember]
    public string FormatDateTimeUtc(long ticks, string format = "yyyy-MM-dd HH:mm:ss")
    {
        try
        {
            return new DateTime(ticks, DateTimeKind.Utc).ToString(format);
        }
        catch
        {
            return string.Empty;
        }
    }

    [LuaMember]
    public string FormatDateTimeOffset(long ticks, string format = "yyyy-MM-dd HH:mm:ss zzz")
    {
        try
        {
            return new DateTimeOffset(ticks, TimeSpan.Zero).ToString(format);
        }
        catch
        {
            return string.Empty;
        }
    }

    [LuaMember]
    public LuaValue ParseDateTime(string dateString)
    {
        try
        {
            return DateTime.Parse(dateString).Ticks;
        }
        catch
        {
            return LuaValue.Nil;
        }
    }

    [LuaMember]
    public LuaValue ParseDateTimeOffset(string dateString)
    {
        try
        {
            return DateTimeOffset.Parse(dateString).Ticks;
        }
        catch
        {
            return LuaValue.Nil;
        }
    }

    [LuaMember]
    public LuaTable GetDateTimeComponents(long ticks)
    {
        var dt = new DateTime(ticks);
        return new LuaTable
        {
            ["Year"] = dt.Year,
            ["Month"] = dt.Month,
            ["Day"] = dt.Day,
            ["Hour"] = dt.Hour,
            ["Minute"] = dt.Minute,
            ["Second"] = dt.Second,
            ["Millisecond"] = dt.Millisecond,
            ["DayOfWeek"] = (int)dt.DayOfWeek,
            ["DayOfYear"] = dt.DayOfYear
        };
    }

    [LuaMember]
    public LuaValue GetTimeDifference(long ticks1, long ticks2)
    {
        var dt1 = new DateTime(ticks1);
        var dt2 = new DateTime(ticks2);
        return Math.Abs((dt2 - dt1).Ticks);
    }

    [LuaMember]
    public LuaTable GetTimeDifferenceComponents(long ticks1, long ticks2)
    {
        var dt1 = new DateTime(ticks1);
        var dt2 = new DateTime(ticks2);
        var diff = dt2 - dt1;
        return new LuaTable
        {
            ["Days"] = diff.Days,
            ["Hours"] = diff.Hours,
            ["Minutes"] = diff.Minutes,
            ["Seconds"] = diff.Seconds,
            ["Milliseconds"] = diff.Milliseconds,
            ["TotalSeconds"] = diff.TotalSeconds,
            ["TotalMinutes"] = diff.TotalMinutes,
            ["TotalHours"] = diff.TotalHours,
            ["TotalDays"] = diff.TotalDays
        };
    }

    [LuaMember]
    public LuaValue AddSeconds(long ticks, double seconds)
        => new DateTime(ticks).AddSeconds(seconds).Ticks;

    [LuaMember]
    public LuaValue AddMinutes(long ticks, double minutes)
        => new DateTime(ticks).AddMinutes(minutes).Ticks;

    [LuaMember]
    public LuaValue AddHours(long ticks, double hours)
        => new DateTime(ticks).AddHours(hours).Ticks;

    [LuaMember]
    public LuaValue AddDays(long ticks, double days)
        => new DateTime(ticks).AddDays(days).Ticks;

    [LuaMember]
    public bool IsAfter(long ticks1, long ticks2) => ticks1 > ticks2;

    [LuaMember]
    public bool IsBefore(long ticks1, long ticks2) => ticks1 < ticks2;

    [LuaMember]
    public bool IsEqual(long ticks1, long ticks2) => ticks1 == ticks2;
}
