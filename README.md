<p align="center"><img src="Test/assets/NovaPlayer.png" alt="Nova Player" width="100"></p>

# Nova Player
**Your media. Beautifully simple.**

A minimal, offline-first **Windows 10/11 music and video player**. Native Windows app, LibVLC engine, soft-blue design, fast loading and a movie-first fullscreen experience.

[**Download Nova Player v1.2.0 — Windows Setup.exe**](https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.2.0-preview)

> **Preview:** Unsigned Windows installer. Automated Windows compilation, LibVLC startup and WPF home-screen smoke tests are required before each published build. Media-playback performance, thumbnails and the upgrade flow should be checked on a real Windows PC before production use. Never disable Windows Defender.

## Highlights

| Feature | Details |
| --- | --- |
| Minimal navigation | Home, Music, Videos, Settings, Updates, About |
| Video playback | LibVLC, MP4/MKV/AVI/WebM and other supported formats |
| Audio | MP3/FLAC/WAV and more (codec dependent) |
| Fullscreen movies | **On by default** for local videos; configurable in **Settings**; Esc to exit |
| Fast window startup | Deferred initialization of LibVLC until after first UI paint |
| Home gallery | Local video cards from Videos, top-level Downloads and imported library |
| Local thumbnails | Windows Explorer thumbnail system; graceful fallback for unavailable codecs |
| Library | Import files and folders, local search, favorites, shuffle/repeat, playback resume |
| Player | Subtitles, snapshots, seeking, 10-second skips, playback speed and aspect ratio |
| Themes | Blue, Violet, Ocean, Rose and Midnight |
| Updates | Manual in-app check, SHA-256 verified download and confirmation-based Windows setup |
| Privacy | No account needed, no media uploaded |

## Requirements

- Windows 10/11, x64
- Compatible GPU and drivers for hardware decoding (performance varies by hardware and codec)
- Storage for an approximately 125 MB Setup.exe and an unpacked self-contained application
- Internet only for optional network streams and updates

## Installation / Updating from Luma Player

1. Download **Nova-Player-Setup-1.2.0.exe** from the [v1.2.0-preview release](https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.2.0-preview).
2. Close the previous Luma Player before installing Nova Player.
3. Run the normal Windows setup wizard.
4. Your previous player library/settings are read from the old **LumaPlayer** settings path on first launch when there isn't a Nova settings file; the original stays untouched.
5. Open **Settings** to enable/disable default video fullscreen. Use **Updates** to check for new versions.

The original **Luma-Player** GitHub repository name remains unchanged during the app rebrand, preserving compatibility with the previous Luma in-app updater. Previous v1.1 updater users can upgrade via an identical compatibility installer named **Luma-Player-Setup-1.2.0.exe**.

## Build from source

```powershell
dotnet restore Test/src/NovaPlayer/NovaPlayer.csproj --runtime win-x64
dotnet publish Test/src/NovaPlayer/NovaPlayer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o Test/publish
```

This builds **NovaPlayer.exe**. The Windows GitHub Actions workflow packages it using Inno Setup 6; see `.github/workflows/windows.yml` and `Test/installer/NovaPlayer.iss`.

## Support and limitations

- Not all files have Windows Explorer thumbnails; cards use a fallback play icon.
- Fullscreen controls use F11 and Esc; some GPU/driver combinations may require testing.
- Windows lock-screen media transport controls are **not** yet implemented.
- No silent or background updater: updates require user action, valid published SHA-256 digest and confirmation.
- Installer is not digitally signed. Microsoft SmartScreen may warn. Never bypass security software.
- To report startup crashes, check `%LOCALAPPDATA%\NovaPlayer\logs\startup.log` (remove private paths before sharing).

## License and third-party notices

See [MIT license](Test/LICENSE) and [third-party licenses](Test/THIRD_PARTY.md). LibVLC and LibVLCSharp have their own applicable notices.

---
Nova Player is the rebranded continuation of Luma Player.