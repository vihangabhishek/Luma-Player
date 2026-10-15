# Nova Player Change Log

## v1.4.0 — Real Video Playback Fix and Classic UI
- Replaced WPF LibVLC video overlay view with LibVLCSharp.WinForms VideoView in a WindowsFormsHost (real HWND).
- Waits for the video host to become visible before connecting the MediaPlayer and opening videos.
- Better error diagnostics if the native video HWND cannot initialize.
- Added Windows CI smoke test with an auto-generated AVI video (checks the native HWND and actual video playing state).
- Replaced the previous dark dashboard-style UI with a familiar, compact Windows media-player layout.
- Maintained Home, Music, Videos, Settings, Updates, About, playlist and full codec engine.
- Kept SHA-256 verified manual in-app updates and original local library/settings migration.
- Fixed fullscreen toolbar height and pre-playback Ctrl+O.
- Still a preview: testing on user GPUs/4K/H.265/subtitles required.

## v1.3.0 — Redesigned library
- Six tabs, video queue, WPF player stage.

## v1.2.1 — Responsiveness
- Background VLC initialization and media opening; more efficient previews and settings saves.
