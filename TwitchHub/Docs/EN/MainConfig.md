# Installation and Setup

## Prerequisites

- .NET 9.0 SDK
- Modern Web Browser (Chrome/Edge/Firefox)

## 1. Configuration (appsettings.json)

Locate or create the `appsettings.json` file in the project root.

**Note for Developers:** If you are building the project from source, you must obtain your own `clientId` and `clientSecret` from `dev.twitch.tv` and configure them in the `appsettings.json` file. A default configuration file is provided with release builds.

Below is a breakdown of the customizable settings:

### ConnectionStrings

Defines the file paths for the local databases. You can change where user points and clip data are stored.

```json
"ConnectionStrings": {
  "LuaPointsDB": "Data Source=data/points.db",
  "TwitchClipsDB": "Data Source=data/clips.db"
}
```

### Twitch

Configures the main Twitch connection.

- `Twitch.Channel`: The target channel the bot should connect to.
- `Twitch.ClipsPollingIntervalSeconds`: How often (in seconds) the bot checks for new clips.

```json
"Twitch": {
  "RedirectUrl": "http://localhost:80/twitch-auth",
  "Channel": "targetchannel",
  "ClipsPollingIntervalSeconds": 60
}
```

### LuaStorage

Configures the directory where the `storagelib` persists data accessible by LUA scripts.

```json
"LuaStorage": {
  "StorageDirectory": "data/"
}
```

### MediaService

Configures the service responsible for audio/media playback.

- `MediaService.Channels`: A collection of distinct audio/video channels.
- `Channels.Main`: The default channel. It is created even if the configuration is empty. It outputs audio to the system's default device.
- `Channels.Stream`: An example of an additional channel that streams audio/video to a specific port (e.g., `http://localhost:6969/stream`) but does **not** output to the system audio device.

```json
"MediaService": {
  "Channels": {
    "Main": {
      "PortEnabled": false
    },
    "Stream": {
      "PortEnabled": true,
      "Port": 6969,
      "Stream": "stream",
      "KeepOnSystem": false
    }
  }
}
```
