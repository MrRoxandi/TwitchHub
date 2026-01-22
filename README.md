# TwitchHub

> This project consolidates essential Twitch streaming tools into one platform, including music playback, keyboard and mouse emulation, and data storage. All features are activated via various events system.

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/MrRoxandi/TwitchHub)

## Features

- **TwitchService:**
  - Fully customizable own events though Twitch Events (Chat messages, Chat comamnds, Subs and [more](TwitchHub/Docs/EN/KIndPrototypes.md))
- **MediaService:**
  - Play any audio and video files with this service on separated channels
  - Suports output on local port and streamnames like: `http://127.0.0.1:port/streamname`
  - Accepts internet links like: `https://example.com/audiofile.mp3`
- **DataContainerService:**
  - Stores almost any data under specified name
- **HardwareService:**
  - Allows to emulate keyboard actions
  - Allows to emulate mouse actions
- **PointsService**
  - Integrated points system

## Highlights

- **[LUA Scripting](#lua-scripting)**: Since the project focuses on complete freedom, we provide possibilities and tools, and the user determines what and how it should work. That's why the most user-friendly interface was chosen for this approach.

## Installation (Windows)

1. Ensure you have Windows with .NET 9 installed. [You can download .NET 9 here if needed](https://dotnet.microsoft.com/en-us/download).
2. Download the latest release from the [Releases](https://github.com/MrRoxandi/TwitchHub/releases) page.
3. Extract the archive to a desired location.

## Running (Windows)

1. Navigate to the extracted directory.
2. Find the appsettings file.json and configure it by following the [documentation](TwitchHub/Docs/EN/MainConfig.md).
3. Run the `TwitchHub.exe`.

## LUA Scripting

This whole project is based on a system of events that trigger the corresponding actions (LuaReactions), which are located in configs/reactions. For a detailed understanding of how to create and write LuaReactions data, you should read the documentation: [GUIDE RU](TwitchHub/Docs/RU/MainDoc.md) | [GUIDE ENG](TwitchHub/Docs/EN/MainDoc.md) .
