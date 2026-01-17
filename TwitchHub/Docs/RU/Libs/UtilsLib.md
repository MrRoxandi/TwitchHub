# UtilsLib

## Доступные функции

Случайные велечины

| Имя              | Аргументы                                      | Описание                                                                           |
| ---------------- | ---------------------------------------------- | ---------------------------------------------------------------------------------- |
| `randomnumber`   | `int min`, `int max`                           | Функция позволяет получить случайное целое число в диапазоне `[min, max)`          |
| `randomdouble`   | `double min`, `double max`                     | Функция позволяет получить случайное действительное число в диапазоне `[min, max)` |
| `randomstring`   | `int length`                                   | Функция позволяет получить случайную строчку длинной `length`                      |
| `randomposition` | `int minx`, `int maxx`, `int miny`, `int maxy` | Функция позволяет получить случайную таблицу координат `{x = rand, y = rand}`      |
| `delay`          | `int duration`                                 | Функция позволяет создать задержу перед на `duration` мс                           |

Взаимодействие с Lua массивами и таблицами (`LuaTable`)

| Имя             | Аргументы                          | Описание                                                                              |
| --------------- | ---------------------------------- | ------------------------------------------------------------------------------------- |
| `isluaarray`    | `LuaTable table`                   | Функция позволяет проверить является ли Lua таблица простым массивом                  |
| `istableempty`  | `LuaTable table`                   | Функция позволяет проверить являтеся ли Lua таблица пустой                            |
| `tablecontains` | `LuaTable table`, `LuaValue value` | Функция позволяет проверить содержит ли таблица значение `value` (не ключ)            |
| `tablerandom`   | `LuaTable table`                   | Функция позволяет получить случайный элемент таблицы `table`                          |
| `tablecopy`     | `LuaTable table`                   | Функция позволяет получить полную копию таблицы `table`                               |
| `tableshuffle`  | `LuaTable table`                   | Функция позволяет получить перемешанную копию таблицы (только массивы) `table`.       |
| `tablejoin`     | `LuaTable table`, `string sep`     | Функция позволяет перевести таблицу в строковое представление разделив элементы `sep` |
| `tabletojson`   | `LuaTable table`                   | Функция позволяет перевести таблицу в `json` строку                                   |

Взаимодействие с Lua строками

| Имя            | Аргументы                       | Описание                                                                                              |
| -------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `stringsplit`  | `string str`, `string delim`    | Функция позволяет разделить строку по `delim` на отдельные строки и вернуть в виде `LuaTable` массива |
| `stringfmt`    | `string strf`, `LuaTable table` | Функция позволяет отформатировать строку                                                              |

Взаимодействие со временем

| Имя                           | Аргументы                      | Описание                                                                                |
| ----------------------------- | ------------------------------ | --------------------------------------------------------------------------------------- |
| `getcurrenttime`              | -                              | Функция позволяет получить текущее время                                                |
| `getcurrenttimeutc`           | -                              | Функция позволяет получить текущее время в формате Utc                                  |
| `formatdatetime`              | `long ticks`, `string format`  | Функция позволяет получить строковое представление времени, например дату: `dd.MM.yyyy` |
| `formatdatetimeUtc`           | `long ticks`, `string format`  | Функция аналогична `formatdatetime`, но работает с Utc временем                         |
| `parsedatetime`               | `string datestring`            | Функция позволяет получить время из строкового представления                            |
| `getdatetimecomponents`       | `long ticks`                   | Функция позволяет получить таблицу компонентов точки во времени. Пример таблицы ниже    |
| `gettimedifference`           | `long lticks, rticks`          | Функция позволяте получить разницу между 2 точками во времени                           |
| `gettimedifferencecomponents` | `long lticks, rticks`          | Функция позволяет получить таблицу компонентов разныци 2 точек во времени.              |
| `addseconds`                  | `long ticks`, `double secodns` | Функция позволяте добавить `seconds` секунд к точке времени                             |
| `addminutes`                  | `long ticks`, `double minutes` | Функция позволяте добавить `minutes` минут к точке времени                              |
| `addhours`                    | `long ticks`, `double hours`   | Функция позволяте добавить `hours` часов к точке времени                                |
| `adddays`                     | `long ticks`, `double days`    | Функция позволяте добавить `days` дней к точке времени                                  |
| `isafter`                     | `long lticks`, `long rticks`   | Функция позволяте проверить позже ли во времени точка `lticks` чем `rticks`             |
| `isbefore`                    | `long lticks`, `long rticks`   | Функция позволяте проверить раньше ли во времени точка `lticks` чем `rticks`            |

Таблица компонентов времени:

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

-- пример использования
local time = utilslib:getcurrenttime()
local table = utilslib:getdatetimecomponents(time)
local day = table['Days']

```
