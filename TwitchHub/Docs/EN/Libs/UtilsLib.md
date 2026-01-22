# UtilsLib

## Available Functions

### Randomization

| Name             | Arguments                                      | Description                                                       |
| :--------------- | :--------------------------------------------- | :---------------------------------------------------------------- |
| `randomnumber`   | `int min`, `int max`                           | Returns a random integer in the range `[min, max)`.               |
| `randomdouble`   | `double min`, `double max`                     | Returns a random floating-point number in the range `[min, max)`. |
| `randomstring`   | `int length`                                   | Returns a random string of the specified `length`.                |
| `randomposition` | `int minx`, `int maxx`, `int miny`, `int maxy` | Returns a random coordinate table `{x = rand, y = rand}`.         |
| `delay`          | `int duration`                                 | Pauses execution for `duration` milliseconds.                     |

### Lua Arrays and Tables (`LuaTable`)

| Name            | Arguments                          | Description                                                                     |
| :-------------- | :--------------------------------- | :------------------------------------------------------------------------------ |
| `isluaarray`    | `LuaTable table`                   | Checks if the Lua table is a simple array (indexed numerically).                |
| `istableempty`  | `LuaTable table`                   | Checks if the Lua table is empty.                                               |
| `tablecontains` | `LuaTable table`, `LuaValue value` | Checks if the table contains the specified `value` (searches values, not keys). |
| `tablerandom`   | `LuaTable table`                   | Returns a random element from the table.                                        |
| `tablecopy`     | `LuaTable table`                   | Creates a deep copy of the table.                                               |
| `tableshuffle`  | `LuaTable table`                   | Returns a shuffled copy of the table (arrays only).                             |
| `tablejoin`     | `LuaTable table`, `string sep`     | Joins table elements into a string separated by `sep`.                          |
| `tabletojson`   | `LuaTable table`                   | Converts the table into a JSON string.                                          |

### String Manipulation

| Name          | Arguments                       | Description                                                          |
| :------------ | :------------------------------ | :------------------------------------------------------------------- |
| `stringsplit` | `string str`, `string delim`    | Splits `str` by `delim` and returns the parts as a `LuaTable` array. |
| `stringfmt`   | `string strf`, `LuaTable table` | Formats a string using the provided arguments.                       |

### Time Management

| Name                          | Arguments                      | Description                                                                        |
| :---------------------------- | :----------------------------- | :--------------------------------------------------------------------------------- |
| `getcurrenttime`              | -                              | Returns the current local time (ticks).                                            |
| `getcurrenttimeutc`           | -                              | Returns the current UTC time (ticks).                                              |
| `formatdatetime`              | `long ticks`, `string format`  | Converts time ticks to a string based on `format` (e.g., `dd.MM.yyyy`).            |
| `formatdatetimeUtc`           | `long ticks`, `string format`  | Similar to `formatdatetime`, but for UTC time.                                     |
| `parsedatetime`               | `string datestring`            | Parses a date string into time ticks.                                              |
| `getdatetimecomponents`       | `long ticks`                   | Returns a table of time components (Day, Hour, etc.) for a specific time.          |
| `gettimedifference`           | `long lticks, rticks`          | Returns the duration difference between two time points.                           |
| `gettimedifferencecomponents` | `long lticks, rticks`          | Returns a table of components representing the difference between two time points. |
| `addseconds`                  | `long ticks`, `double secodns` | Adds `seconds` to the time point.                                                  |
| `addminutes`                  | `long ticks`, `double minutes` | Adds `minutes` to the time point.                                                  |
| `addhours`                    | `long ticks`, `double hours`   | Adds `hours` to the time point.                                                    |
| `adddays`                     | `long ticks`, `double days`    | Adds `days` to the time point.                                                     |
| `isafter`                     | `long lticks`, `long rticks`   | Checks if `lticks` is chronologically after `rticks`.                              |
| `isbefore`                    | `long lticks`, `long rticks`   | Checks if `lticks` is chronologically before `rticks`.                             |

### Time Components Table Structure

```lua
{
  Days = 'int number',
  Hours = 'int number',
  Minutes = 'int number',
  Seconds = 'int number',
  Milliseconds = 'int number',
  TotalSeconds = 'int number',
  TotalMinutes = 'int number',
  TotalHours = 'int number',
  TotalDays = 'int number',
}

-- Usage Example
local time = utilslib:getcurrenttime()
local table = utilslib:getdatetimecomponents(time)
local day = table['Days']
```
