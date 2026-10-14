# Nova Player Windows Test Checklist

- GitHub Actions: build `NovaPlayer.exe` with .NET 8 for win-x64.
- GitHub Actions: native LibVLC self-test `NovaPlayer.exe --diagnose`.
- GitHub Actions: WPF window startup `NovaPlayer.exe --ui-smoke-test`.
- GitHub Actions: Inno Setup compile `Nova-Player-Setup-1.2.0.exe`.
- Physical PC: install/upgrade from Luma 1.1.0 without losing playlist settings.
- Physical PC: Home thumbnails, navigation, Music, Videos tabs.
- Physical PC: videos open fullscreen by default and exit via Esc; toggle Settings to off.
- Physical PC: music does not auto fullscreen.
- Physical PC: subtitles, playlists, volume, snapshots and update detection.
- Physical PC: verify old app update recognises compatibility asset after release.

Unsigned technical preview: full interactive validation on real Windows PCs is still needed.
