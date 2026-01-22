# LoggerLib

## Available Functions

| Name            | Arguments                         | Description                                    |
| :-------------- | :-------------------------------- | :--------------------------------------------- |
| `loginfo`       | `string message`                  | Logs a message with level `Info`.              |
| `logdebug`      | `string message`                  | Logs a message with level `Debug`.             |
| `logwarning`    | `string message`                  | Logs a message with level `Warning`.           |
| `logerror`      | `string message`                  | Logs a message with level `Error`.             |
| `loginfofmt`    | `string message`, `LuaTable args` | Logs a formatted message with level `Info`.    |
| `logdebugfmt`   | `string message`, `LuaTable args` | Logs a formatted message with level `Debug`.   |
| `logwarningfmt` | `string message`, `LuaTable args` | Logs a formatted message with level `Warning`. |
| `logerrorfmt`   | `string message`, `LuaTable args` | Logs a formatted message with level `Error`.   |

### Usage Example

Logging a formatted Info message:

```lua
local arg = 'value'
loggerlib:loginfofmt("Some cool {0}", {arg})
-- Output: Some cool value
```
