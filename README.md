# Nova Player

**Your media. Beautifully simple.** A clean video and music player for Windows, built with native WPF and the LibVLC engine.

**[Download Nova Player v1.3.0 — Windows Setup.exe](https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.3.0-preview)**

> Stability candidate / unsigned preview. Windows CI checks compilation, native LibVLC loading, actual WAV playback, WPF window startup and navigation. Physical Windows GPU/driver/movie/subtitle/installer testing is still needed. Never disable Windows Defender.

## What's new in v1.3.0

A redesigned dark interface, minimal lightweight surfaces, and six working navigation tabs:

- **Home** — video preview cards from your own Videos folder, top-level Downloads and imported library, with Explorer thumbnails when available.
- **Music** — a full-width scrollable music library with double-click to play.
- **Videos** — a full-width dedicated video browser, no empty placeholder.
- **Settings** — fullscreen-on-open preference, dark accent preset, and video refresh.
- **Updates** — manual verified GitHub release checks, downloads, and installer launch.
- **About** — app information and privacy.
- **Player view** — large black video surface, right-hand playlist, streamlined play/pause/seek/volume/shuffle/repeat, subtitles, snapshots, aspect/speed controls and fullscreen.

Background media loading prevents many window hangs; the browser lists virtualize media items and thumbnails are capped. Movies go fullscreen by default; turn it off in Settings or press Esc to leave. GPU playback varies by hardware; zero-lag 4K is not guaranteed.

## Install and update

Download **Nova-Player-Setup-1.3.0.exe** in the Releases page. It contains both the .NET desktop runtime and the complete LibVLC codec engine, so the installer is around 125 MB.

From Nova v1.2.0/v1.2.1, open **Updates → Check for updates → Download & install**. The app verifies the official GitHub SHA-256 checksum and, after your confirmation, opens the normal Windows installer. If your old version is frozen, close it and use the direct link above instead. It preserves user state in %APPDATA%/NovaPlayer/state.json and imports an older LumaPlayer state if needed.

## Keyboard shortcuts

| Keys | Action |
| --- | --- |
| Space | Play / pause |
| Left / Right | Skip 5 seconds |
| Up / Down | Volume |
| F11 | Video fullscreen |
| Esc | Exit fullscreen |
| Ctrl + O | Open media file |
| Ctrl + N | Next track |

Local playback works offline with MP4, MKV, MP3, FLAC and other LibVLC-supported formats. Network streams only contact the URL you explicitly open. Some movies will display a play icon instead of a thumbnail if Windows Explorer has no preview handler for that format.

## Build from source

Requirements: Windows 10/11, .NET 8 SDK, Inno Setup 6 for packaging.

1. Run: dotnet restore Test/src/NovaPlayer/NovaPlayer.csproj -r win-x64
2. Run: dotnet publish Test/src/NovaPlayer/NovaPlayer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o Test/publish
3. The GitHub Actions workflow at .github/workflows/windows.yml tests and packages the Windows installer.

## Privacy, licensing and troubleshooting

The player does not upload local media, metadata or thumbnails, has no user account or telemetry, and contacts GitHub only when you ask it to check for an update. Diagnostic logs stay in %LOCALAPPDATA%/NovaPlayer/logs/startup.log. Redact private paths before sharing.

Source: MIT License (Test/LICENSE). Third-party: LibVLCSharp and VideoLAN LibVLC (Test/THIRD_PARTY.md).

The GitHub repo address remains **vihangabhishek/Luma-Player** for compatibility with older updates, while the app installed on your PC is **Nova Player**.
