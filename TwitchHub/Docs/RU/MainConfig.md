# Установка и настройка

## Требования

- .NET 9.0 SDK
- Браузер (Chrome/Edge/Firefox)

## 1. Конфигурация (appsettings.json)

В корне проекта найдите или создайте `appsettings.json`. Если вы разработчик или хотите собрать проект сами, то вам необходимо использовать свои clientId clientSecret, полученные с `dev.twitch.tv`. Их необходимо будет указать в соответствующих полях файла `appsettings.json`.

Стандартный файл конфигураций поставляется совместно с релизом.

Часть настроек можно изменять под себя. Разберем их ниже:

`ConnectionStrings` секция отвечает за расположение файлов базы данных на вашем компьютере. Можно изменить, где буду храниться данные о клипах и очках пользователей.

```json
"ConnectionStrings": {
  "LuaPointsDB": "Data Source=data/points.db",
  "TwitchClipsDB": "Data Source=data/clips.db"
}
```

`Twitch` секция отвечает за основную настройку подключения к твичу.
`Twitch.Channel` - канал, на который необходимо подключаться боту
`Twitch.ClipsPollingIntervalSeconds` - количество секунд, раз в которое будет проверяться наличие новых клипов

```json
"Twitch": {
  "RedirectUrl": "http://localhost:80/twitch-auth",
  "Channel": "targetchannel",
  "ClipsPollingIntervalSeconds": 60
}
```

`LuaStorage` секция отвечает за настрйоку места хранения информации из `storagelib` в `LUA` скриптах.

```json
"LuaStorage": {
  "StorageDirectory": "data/"
}
```

`MediaService` секция отвечает за настройку медиа сервиса для возспроизведения аудио/медиа контента
`MediaService.Channels` - коллекция с различными аудио/видео каналами
`Channels.Main` - стандартный канал, которые будет создаваться, даже если конфигурация сервиса пуста. (Транслирует только звук на всю систему)
`Channels.Stream` - дополнительный канал, которые транслирует аудио/видео на порт `6969` и секцию `stream` (`http://localhost:6969/stream`), но не транслирует на систему.

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
