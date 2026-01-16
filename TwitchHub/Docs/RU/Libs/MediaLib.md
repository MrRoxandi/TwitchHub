# MediaLib

## Доступные функции

| Имя         | Аргументы                           | Описание                                                                                                 |
| ----------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `channels`  | -                                   | Функция позволяет получить таблицу содержащую список всех настроенных медиа каналов в `appsettings.json` |
| `add`       | `string channel`, `string filepath` | Функция позволяет добавить в очередь воспроизведения канала `channel` медиафайл `filepath`               |
| `start`     | `string channel`                    | Функция позволяет запустить вопспроизведение в канале `channel`, если оно было остановлено               |
| `stop`      | `string channel`                    | Функция позволяет остановить вопспроизведение в канале `channel`, если оно было запущено                 |
| `skip`      | `string channel`                    | Функция позволяет пропустить текущее воспроизводимое медиа в канале `channel`                            |
| `pause`     | `string channel`                    | Функция позволяет переключить состояние паузы медиа в канале `channel`                                   |
| `setvolume` | `string channel`, `int volume`      | Функция позволяет установить громкость (от 0 до 100) в канале `channel`                                  |
| `setspeed`  | `string channel`, `float speed`     | Функция позволяет установить скорость воспроизведения (от 0.0 до 1.0) в канале `channel`                 |
| `getvolume` | `string channel`                    | Функция позволяет получить громкость в канале `channel`                                                  |
| `getspeed`  | `string channel`                    | Функция позволяет получить скорость воспроизведения в канале `channel`                                   |
| `ispaused`  | `string channel`                    | Функция позволяте получить состояние `ispaused` для определенного канала `channel`                       |
| `isplaying` | `string channel`                    | Функция позволяте получить состояние `isplaying` для определенного канала `channel`                      |
| `isstopped` | `string channel`                    | Функция позволяте получить состояние `isstopped` для определенного канала `channel`                      |

Пример проверки, установки состояния и запуска условного трека на стандартном канале `main`

```lua
local mediaFile = "C:/files/music/example.mp3"
if medialib:isplaying('main') then
  medialib:skip('main')
elseif medialib:isstopped('main')
  medialib:start('main')
end
if medialib:getvolume('main') > 40 then
  medialib:setvolume('main', 40)
end
medialib:add('main', mediaFile)
```
