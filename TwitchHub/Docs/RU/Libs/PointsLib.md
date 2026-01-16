# PointsLib

## Доступные функции

| Имя    | Аргументы                       | Описание                                                                                           |
| ------ | ------------------------------- | -------------------------------------------------------------------------------------------------- |
| `get`  | `string userid`                 | Функция позволяет получить количество очков у пользователя с id `userid`                           |
| `set`  | `string userid`, `long ammount` | Функция позволяет установить `ammount` очков у пользователя с id `userid`                          |
| `add`  | `string userid`, `long ammount` | Функция позволяет добавить `ammount` очков у пользователя с id `userid`                            |
| `take` | `string userid`, `long ammount` | Функция позволяет забрать `ammount` очков у пользователя с id `userid`, и вернуть результат успеха |

Пример проверки и изменения количества очков у пользователя

```lua
local userid = '1298asdanj332'
local upoints = pointslib:get(userid)
if upoints < 150:
  local needed = 150 - upoints
  pointslib:add(userid, needed)
end
```
