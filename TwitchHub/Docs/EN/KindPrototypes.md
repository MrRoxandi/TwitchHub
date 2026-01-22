# Kind Prototypes

Each `LuaReaction` type (`kind`) corresponds to a specific function prototype. The table below details the required signature for each kind:

| Kind               | Prototype                                                |
| ------------------ | -------------------------------------------------------- |
| `Command`          | `function(username, userid, args)`                       |
| `Reward`           | `function(username, userid, title, id, userinput, cost)` |
| `Message`          | `function(username, userid, message)`                    |
| `Follow`           | `function(username, userid)`                             |
| `Subscribe`        | `function(username, userid, tier, isgift)`               |
| `GiftSubscribe`    | `function(username, userid, total, tier, sumtotal)`      |
| `Cheer`            | `function(username, userid, bits, message)`              |
| `StreamOn`         | `function(startedat, type)`                              |
| `StreamOff`        | `function()`                                             |
| `Clip`             | `function(id, url, title, userid, username, duration)`   |
| `KeyDown`          | `function(keycode)`                                      |
| `KeyUp`            | `function(keycode)`                                      |
| `KeyType`          | `function(keycode)`                                      |
| `MouseDown`        | `function(keycode)`                                      |
| `MouseUp`          | `function(keycode)`                                      |
| `MouseClick`       | `function(keycode)`                                      |
| `MouseMove`        | `function(x, y)`                                         |
| `MouseWheel`       | `function(delta, direction)`                             |
| `MediaAdd`         | `function(channelname, source, queuepos)`                |
| `MediaStart`       | `function(channelname, source, startedat)`               |
| `MediaSkip`        | `function(channelname, source, skippedat)`               |
| `MediaPause`       | `function(channelname, source, pausedat)`                |
| `MediaStop`        | `function(channelname, source, stoppedat)`               |
| `MediaEnd`         | `function(channelname, source, endedat)`                 |
| `MediaQueueFinish` | `function(channelname)`                                  |
| `MediaError`       | `function(channelname, source, errortime)`               |
