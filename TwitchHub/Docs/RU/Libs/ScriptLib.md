# ScripLib

## Доступные функции

| Имя        | Аргументы    | Описание                                                                  |
| :--------- | :----------- | :------------------------------------------------------------------------ |
| `keys`     | -            | Позволяет получить `LuaTable` содержащую все доступные для вызова скрипты |
| `contains` | `string key` | Проверить содержание скрипта по его ключу                                 |
| `remove`   | `string key` | Удалить существующий скрипт                                               |
| `call`     | `string key` | Выполнить существующий скрипт                                             |

### Пример использования

Простой пример использования системы скриптов

`../configs/scripts/example.lua`

```lua
local n = utilslib:randomnumber(0, 8000)
loggerlib:loginfofmt("Сгенерировано случангое ({value}) число", {n})
return n
```

`../configs/reactions/gamble.lua`

```lua

function callfn(uname, uid, args)
  local ex = scriptlib:call('example')
  local message = utilslib:strinfmt("@{uname} выпало - {value}", {uname, ex})
  twitchlib:sendmessage(message)
end

return {
  kind = 'command',
  oncall = callfn,
  cooldown = 3000
}
```
