# Документация по скриптам в конфигурации

В данном документе описано, как писать `LuaReaction` для различных `Events`.

## Доступные библиотеки в `TwitchHub`

| Название      | Описание                                                                       | Документация               |
| ------------- | ------------------------------------------------------------------------------ | -------------------------- |
| `hardwarelib` | Библиотека позволяет взаимодействовать с оборудованием                         | [DOC](Libs/HardwareLib.md) |
| `loggerlib`   | Библиотека позволяет отправлять `Log` сообщения в консоль и `log file`         | [DOC](Libs/LoggerLib.md)   |
| `medialib`    | Библиотека позволяет взаимодействовать с аудио/видое файлами                   | [DOC](Libs/MediaLib.md)    |
| `pointslib`   | Библиотека позволяет взаимодействовать с системой очков                        | [DOC](Libs/PointsLib.md)   |
| `storagelib`  | Библиотека позволяет взаимодейстовать со специальным хранилищем данных         | [DOC](Libs/StorageLib.md)  |
| `twitchlib`   | Библиотека позволяет взаимодейстовать с Twitch                                 | [DOC](Libs/TwitchLib.md)   |
| `utilslib`    | Библиотека, содержащая в себе полезные инструменты для создания `LuaReactions` | [DOC](Libs/UtilsLib.md)    |

## Требования к файлу `LuaReaction`

- Формат файла: конфигурация должна быть файлом `lua`.
- Расположение: конфигурация должна находиться в каталоге `./configs/reactions`.
- Возвращаемое значение: конфигурация **обязана** возвращать lua-таблицу, содержащую **обязательные** поля.

## Таблица конфигурации `LuaReaction`

Возвращаемая таблица в каждом файле `example.lua` должа выглядеть вот так:

```lua
-- другой ваш код
return {
  oncall = callfn,
  onerror = errorfn,
  kind = 'command',
  cooldown = 10
}
```

| Параметр   | Опсание                                                                    | Стандартное значение | Обязателен |
| ---------- | -------------------------------------------------------------------------- | -------------------- | ---------- |
| `oncall`   | Lua функция, соотвествующая типу `kind`. [Прототипы](doclink)              | -                    | +          |
| `onerror`  | Lua функция, будет вызвана, если произошла ошибка в `oncall`               | -                    | +          |
| `kind`     | Тип события, которому соотвествует данная `LuaReaction`. [Список](doclink) | -                    | +          |
| `cooldown` | Задерка до следующего срабатывания данной `LuaReaction`                    | 0                    | -          |

Пример простейшей `!greet` команды Twitch чата. Файл `greet.lua`:

```lua
function callfn(uname, uid, args)
  local message = utilslib:stringformat("Hello {0} from Lua", {uname})
  twitchlib:sendmessage(message)
end

function errorfn(reactionname, errormsg, errortime)
  local message = utilslib:stringformat("Error in {0}. Reason: {1}", {reactionname, errormsg})
  loggerlib:loginfo(message)
end

return {
  oncall = callfn,
  onerror = errorfn,
  cooldown = 10,
  kind = 'command'
}
```
