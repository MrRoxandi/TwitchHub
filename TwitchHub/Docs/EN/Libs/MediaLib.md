# MediaLib

## Available Functions

| Name        | Arguments                           | Description                                                                                 |
| :---------- | :---------------------------------- | :------------------------------------------------------------------------------------------ |
| `channels`  | -                                   | Returns a table containing the list of all media channels configured in `appsettings.json`. |
| `add`       | `string channel`, `string filepath` | Adds the media file at `filepath` to the playback queue of the specified `channel`.         |
| `start`     | `string channel`                    | Resumes playback on `channel` if it was stopped.                                            |
| `stop`      | `string channel`                    | Stops playback on `channel` if it is running.                                               |
| `skip`      | `string channel`                    | Skips the currently playing media on `channel`.                                             |
| `pause`     | `string channel`                    | Toggles the pause state of `channel`.                                                       |
| `setvolume` | `string channel`, `int volume`      | Sets the volume (0 to 100) for `channel`.                                                   |
| `setspeed`  | `string channel`, `float speed`     | Sets the playback speed (0.0 to 1.0) for `channel`.                                         |
| `getvolume` | `string channel`                    | Returns the current volume of `channel`.                                                    |
| `getspeed`  | `string channel`                    | Returns the current playback speed of `channel`.                                            |
| `ispaused`  | `string channel`                    | Returns `true` if `channel` is currently paused.                                            |
| `isplaying` | `string channel`                    | Returns `true` if `channel` is currently playing.                                           |
| `isstopped` | `string channel`                    | Returns `true` if `channel` is currently stopped.                                           |

### Usage Example

Checking state, setting volume, and queuing a track on the 'main' channel:

```lua
local mediaFile = "C:/files/music/example.mp3"

if medialib:isplaying('main') then
  medialib:skip('main')
elseif medialib:isstopped('main') then
  medialib:start('main')
end

if medialib:getvolume('main') > 40 then
  medialib:setvolume('main', 40)
end

medialib:add('main', mediaFile)
```
