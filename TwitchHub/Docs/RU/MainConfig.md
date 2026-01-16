# Установка и настройка

## Требования

- .NET 9.0 SDK
- Браузер (Chrome/Edge/Firefox)
- OBS Studio (для вывода медиа)

## 1. Конфигурация (appsettings.json)

В корне проекта найдите или создайте `appsettings.json`. Если вы разработчик или хотите собрать проект сами, то вам необходимо использовать свои clientId clientSecret, полученные с `dev.twitch.tv`. Их необходимо будет указать в соответствующих полях файла `appsettings.json`.

Стандартный файл конфигурации приведён ниже

```json
{
  "SeriLog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "TwitchLib.Api.Core.HttpCallHandlers.TwitchHttpClient": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Properties": {
      "Application": "TwitchHub"
    }
  },
  "AllowedHosts": "localhost",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:80"
      }
    }
  },
  "ConnectionStrings": {
    "LuaPointsDB": "Data Source=data/points.db",
    "TwitchClipsDB": "Data Source=data/clips.db"
  },
  "Twitch": {
    "RedirectUrl": "http://localhost:80/twitch-auth", // Зависит от конфигурации на dev.twitch.tv
    // "ClientId": "YOURS_CLIENT_ID", // Не требует конфигурации в релизах
    // "ClientSecret": "YOURS_CLIENT_SECRET", // Не требует конфигурации в релизах
    "Channel": "TARGET_CHANNEL",
    "ClipsPollingIntervalSeconds": 60
  },
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
}
```
