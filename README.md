[Luma-Player-README.md](https://github.com/user-attachments/files/33452180/Luma-Player-README.md)
<div align="center">
  <img src="assets/LumaPlayer.png" alt="Luma Player logo" width="116" />

  # Luma Player
  **Your media. Beautifully played.**

  A blue-glass-inspired, offline-first Windows 11 **music & video player**, powered by the VideoLAN LibVLC playback engine.

  ![Windows](https://img.shields.io/badge/Windows-11%20%7C%2010-blue) ![.NET](https://img.shields.io/badge/.NET-8-512BD4) ![Build](https://img.shields.io/badge/status-technical%20preview-orange) ![License](https://img.shields.io/badge/code-MIT-green)
</div>

> ⚠️ **Technical preview:** Source is prepared, but the actual Windows installer and end-to-end Windows performance have not been tested yet. Do not call it stable until the release checklist passes. Installer is not code-signed.

## ✨ Highlights

| Feature | Status |
| --- | --- |
| Modern glass-inspired interface, five presets, blue by default | Included |
| Native Windows WPF app (no browser window, no PowerShell launcher) | Included |
| Audio & video powered by LibVLC (MP4, MKV, MP3, FLAC, etc.) | Included, codec dependent |
| GPU hardware decoding request via LibVLC | Included, hardware/driver dependent |
| Open files, scan folders, drag and drop | Included |
| HTTP/HTTPS streams (not an internet discovery service) | Included |
| Play/pause, seek, next/previous, 10-second skips | Included |
| Volume/mute, playback speed 0.5×–2×, aspect ratios | Included |
| External SRT/ASS/VTT subtitle loading | Included |
| Screenshots of a video as PNG | Included |
| Filterable library, search, Favorites, recently played | Included |
| Shuffle and Repeat Off / All / One | Included |
| Remember playback positions and reopen local library | Included |
| Fullscreen and keyboard shortcuts | Included |
| Windows Lock Screen / System Media Transport Controls (SMTC) | **Not yet implemented** |
| In-app audio equalizer, HDR tone mapping controls, album-art fetching, casting, gapless playback | Planned, not promised |

**Quality:** LibVLC supports hardware decoding where possible; smooth 1080p, 4K or HDR playback depends on your CPU/GPU, file codec, GPU drivers and storage throughput. No player can guarantee lag-free 4K on every PC. Glass effects are used outside the video render surface to help avoid impairing decoding/rendering. Native codec support comes from LibVLC; all VideoLAN and codec/patent licenses remain applicable.

## 🖥️ Requirements

- Windows 11 x64 (Windows 10 x64 may work, not yet certified).
- Recommended: modern GPU/updated graphics drivers for high-resolution playback.
- Internet only for downloading/installing the application and when opening your own streaming URL. Local playback works offline.
- Installer bundles a self-contained .NET runtime and native LibVLC libraries: its file size will be larger than a simple note app.

## 📦 Install from GitHub Actions

1. Create a new **public** GitHub repository named `Luma-Player` (separate from `Luma-Diary`).
2. Upload the **contents** of this source folder to the repository root, including hidden `.github/workflows/windows.yml`.
3. Go to **Actions → Build Windows Installer → Run workflow → main**.
4. After a successful build, open the workflow run and download the `Luma-Player-Windows-Setup` artifact.
5. Extract the ZIP and run `Luma-Player-Setup-1.0.0.exe`.

The installer uses **Inno Setup** (ordinary wizard, Start Menu/Desktop shortcuts and Windows uninstall), not a PowerShell-based launcher. If CI fails, review the build logs. **An unsigned EXE can trigger SmartScreen**; do not disable Defender to bypass it.

## 🛠️ Build locally on Windows

Install Visual Studio 2022 with the .NET desktop workload or .NET 8 SDK, then run:

```powershell
dotnet restore src/LumaPlayer/LumaPlayer.csproj --runtime win-x64
dotnet publish src/LumaPlayer/LumaPlayer.csproj -c Release -r win-x64 --self-contained true -o publish
```

For an installer, install [Inno Setup 6](https://jrsoftware.org/isinfo.php) and compile `installer/LumaPlayer.iss`. The app icon and source assets are located under `assets/`.

## ⌨️ Shortcuts

| Key | Action |
| --- | --- |
| Space | Play / Pause |
| Left / Right | Seek back / forward 5 seconds |
| Up / Down | Volume ±5 |
| F11 | Fullscreen |
| Esc | Exit fullscreen |
| Ctrl+O | Open media files |
| Ctrl+N | Next item |

Use double-click to play a library item. In the right library panel, single-click an item to favorite or remove it.

## 🛡️ Privacy

No mandatory account, telemetry or cloud sync. Local library history (paths and playback positions) lives at `%APPDATA%\LumaPlayer\state.json` and is **not encrypted**. Opening a streaming URL sends requests to that server. The app doesn't upload your local files. See [SECURITY.md](SECURITY.md).

## 🎵 Windows lock-screen controls

The Windows lock-screen/Quick Settings media player uses **System Media Transport Controls (SMTC)**. Microsoft's `Windows.Media.Playback.MediaPlayer` gets automatic integration, but this app uses **LibVLC** for broad codec support. Manual SMTC integration must be implemented and tested separately; **v1.0 does not yet display lock-screen controls**. See [Microsoft's SMTC guide](https://learn.microsoft.com/en-us/windows/apps/develop/media-playback/integrate-with-systemmediatransportcontrols).

## 🚀 Roadmap

1. Confirm Windows 11 install, full-screen behavior, subtitle loading, real 1080p/4K playback and GPU usage.
2. Implement SMTC/lock-screen playback controls and media metadata, with Windows integration tests.
3. Add audio equalizer, thumbnail art, track menus and additional advanced controls.
4. Code signing, official releases and secure update notifications.

See [testing checklist](docs/TESTING.md), [Sinhala GitHub guide](docs/GITHUB_GUIDE_SI.md) and [changelog](CHANGELOG.md).

## 🧾 License & Credits

Luma Player application source is MIT licensed (see LICENSE). Playback uses [LibVLCSharp](https://github.com/videolan/libvlcsharp) and [VideoLAN LibVLC](https://www.videolan.org/vlc/libvlc.html), with separate copyleft license obligations. Read the notices in [THIRD_PARTY.md](THIRD_PARTY.md) before distributing binaries. Not affiliated with VideoLAN/VLC.
