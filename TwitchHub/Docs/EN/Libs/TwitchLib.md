# TwitchLib

## Available Functions

| Name                 | Arguments                      | Description                                                                   |
| :------------------- | :----------------------------- | :---------------------------------------------------------------------------- |
| `isconnected`        | -                              | Checks if the bot is currently connected to Twitch Chat.                      |
| `sendmessage`        | `string message`               | Sends a message to the chat as the broadcaster.                               |
| `getuserid`          | `string username`              | Retrieves the `userid` for a given `username`.                                |
| `getusername`        | `string userid`                | Retrieves the `username` for a given `userid`.                                |
| `isbroadcaster`      | `string userid`                | Checks if the user is the Broadcaster.                                        |
| `ismoderator`        | `string userid`                | Checks if the user is a Moderator.                                            |
| `isvip`              | `string userid`                | Checks if the user is a VIP.                                                  |
| `isfollower`         | `string userid`                | Checks if the user is a Follower.                                             |
| `atleast`            | `string userid`, `string rank` | Checks if the user holds at least the specified `rank` (follower, vip, etc.). |
| `getfollowedate`     | `string userid`                | Returns the date the user followed the channel, or `-1` if not following.     |
| `getstreamtitle`     | -                              | Retrieves the current stream title.                                           |
| `getstreamgamename`  | -                              | Retrieves the name of the game currently being played.                        |
| `getstreamstartedat` | -                              | Retrieves the timestamp of when the stream started.                           |
| `getstreamviewers`   | -                              | Retrieves the current viewer count.                                           |

### Usage Example

Calculating stream uptime:

```lua
local start = twitchlib:getstreamstartedat()
local now = utilslib:getcurrenttimeutc()
local duration = utilslib:gettimedifference(start, now)
local formatted = utilslib:formatdatetimeutc(duration, 'HH h. mm min.')
```
