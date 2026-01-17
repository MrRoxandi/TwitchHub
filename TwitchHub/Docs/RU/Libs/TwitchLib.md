# StorageLib

## Доступные функции

| Имя                  | Аргументы                         | Описание                                                                                 |
| -------------------- | --------------------------------- | ---------------------------------------------------------------------------------------- |
| `isconnected`        | -                                 | Функция позволяет проверить есть ли подключение к чату                                   |
| `sendmessage`        | `string message`                  | Функция позволяет отправить сообщение в чат от имени стримера                            |
| `sendreply`          | `string userid`, `string message` | Функция позволяет отправить ответ пользователю в чат от имени стримера                   |
| `getuserid`          | `string username`                 | Функция позволяет получить `userid` по имени пользователя                                |
| `getusername`        | `string userid`                   | Функция позволяет получить `username` по его `userid`                                    |
| `isbroadcaster`      | `string userid`                   | Функция позволяет проверить является ли `userid` `broadcaster`                           |
| `ismoderator`        | `string userid`                   | Функция позволяет проверить является ли `userid` `moderator`                             |
| `isvip`              | `string userid`                   | Функция позволяет проверить является ли `userid` `vip`                                   |
| `isfollower`         | `string userid`                   | Функция позволяет проверить является ли `userid` `follower`                              |
| `atleast`			   | `string userid`, `string rank`    | Функция позволяет проверить является ли `userid` хотябы `rank` (follower, vip и т.д.)    |
| `getfollowedate`     | `string userid`                   | Функция позволяет получить дату, когда пользователь зафолловился на стримера, иначе `-1` |
| `getstreamtitle`     | -                                 | Функция позволяет получить название текущего стрима                                      |
| `getstreamgamename`  | -                                 | Функция позволяет получить название текущей игры на стриме                               |
| `getstreamstartedat` | -                                 | Функция позволяет получить время в которое стрим был запущен                             |
| `getstreamviewers`   | -                                 | Функция позволяет получить текущее количество зрителей на стриме                         |

Пример простого метода реализации получения продолжительности трансляции

```lua
local start = twitchlib:getstreamstartedat()
local now = utilslib:getcurrenttimeutc()
local duration = utilslib:gettimedifference(start, now)
local formated = utilslib:formatdatetimeutc(duration, 'HH ч. mm мин.')

```
