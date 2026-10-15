# Nova Player v1.4.0 — Classic Windows Media Player

A clean, simple media player for Windows inspired by the usability of classic desktop players. The app remains an independent Nova product using VideoLAN's LibVLC engine.

**Download:** https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.4.0-preview

## Why v1.4 was needed

The previous WPF video view could create a video window that was not ready when playback started, potentially resulting in a black screen or audio-only playback. Nova Player v1.4 uses a **LibVLCSharp.WinForms native video view hosted in WindowsFormsHost** and only attaches the player after the video host becomes visible. This makes playback less dependent on WPF overlay behavior.

## Features

- Classic menu bar with Media, Playback and Help; easy Open File, Open Stream and Add Folder actions
- Large video stage and bottom play/pause, seek, volume, fullscreen and speed controls
- Home with video preview cards; Music and Videos with browse-and-double-click lists
- Separate Settings, Updates, About and playlist queue
- Subtitles, hardware decoding where supported, resume, favorites and shuffle/repeat
- Fullscreen movie playback default; turn it off in Settings and use Esc to exit
- Offline media library, no telemetry, no account

## In-app updates

From Nova Player v1.2 or newer open **Updates > Check for updates > Download & install**. Nova downloads the official GitHub release setup and verifies its published SHA-256 hash before opening the normal setup wizard. If the old app cannot launch, use the release installer directly.

The installer includes .NET Desktop and the native LibVLC engine. Windows 10/11 x64; package size is larger than a standalone DLL because full offline dependencies are bundled.

## Build and testing

Restore: dotnet restore Test/src/NovaPlayer/NovaPlayer.csproj -r win-x64

Publish: dotnet publish Test/src/NovaPlayer/NovaPlayer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o Test/publish

Windows GitHub Actions tests native engine startup, actual WAV audio playback, WPF navigation and **a locally generated AVI clip played through the real native Windows video HWND**. Those tests are a stronger baseline than merely compiling, but do not guarantee all user videos, subtitles, GPUs or 4K playback. This remains an unsigned technical preview, not an audited stable release.

Existing library and settings are preserved under %APPDATA%/NovaPlayer/state.json and older LumaPlayer state can be imported.

Diagnostics: %LOCALAPPDATA%/NovaPlayer/logs/startup.log (remove private paths before sharing).

License: Test/LICENSE (MIT). Native media credits: Test/THIRD_PARTY.md.

GitHub repository retains the Luma-Player URL for legacy updater compatibility. Installed product branding is Nova Player.
