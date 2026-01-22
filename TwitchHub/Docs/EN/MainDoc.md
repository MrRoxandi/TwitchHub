# Script Configuration Documentation

Before creating custom scripts, it is **highly recommended** to review the [Main Application Configuration](MainConfig.md).

This document describes how to write `LuaReaction` scripts for various `Events`.

## Available Libraries in `TwitchHub`

| Library       | Description                                             | Documentation              |
| :------------ | :------------------------------------------------------ | :------------------------- |
| `hardwarelib` | Interactions with system hardware (keyboard/mouse).     | [DOC](Libs/HardwareLib.md) |
| `loggerlib`   | Writes `Log` messages to the console and log files.     | [DOC](Libs/LoggerLib.md)   |
| `medialib`    | Controls audio and video playback.                      | [DOC](Libs/MediaLib.md)    |
| `pointslib`   | Manages the points system.                              | [DOC](Libs/PointsLib.md)   |
| `storagelib`  | Interactions with persistent data storage.              | [DOC](Libs/StorageLib.md)  |
| `twitchlib`   | Interactions with the Twitch API and Chat.              | [DOC](Libs/TwitchLib.md)   |
| `utilslib`    | Useful tools and utilities for creating `LuaReactions`. | [DOC](Libs/UtilsLib.md)    |

## `LuaReaction` File Requirements

- **File Format:** Must be a `.lua` file.
- **Location:** Must be placed in the `./configs/reactions` directory.
- **Return Value:** The script **must** return a Lua table containing specific required fields.

## Configuration Table Structure

Every script (e.g., `example.lua`) must return a table formatted as follows:

```lua
-- Your custom code here
return {
  oncall = callfn,
  onerror = errorfn,
  kind = 'command',
  cooldown = 10
}
```

| Parameter  | Description                                                                        | Default Value | Required |
| :--------- | :--------------------------------------------------------------------------------- | :------------ | :------- |
| `oncall`   | The Lua function corresponding to the `kind`. See [Prototypes](KindPrototypes.md). | -             | **Yes**  |
| `kind`     | The event type this reaction handles. See [List](KindPrototypes.md).               | -             | **Yes**  |
| `onerror`  | The Lua function called if an error occurs within `oncall`.                        | -             | No       |
| `cooldown` | Delay (in seconds) before this reaction can be triggered again.                    | 0             | No       |

### Example: Basic `!greet` Command

File: `greet.lua`

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
