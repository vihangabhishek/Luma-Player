## 1.2.1 — Smoother playback and responsiveness

- Initialize native VLC asynchronously to keep the app window responsive.
- Open local media and network streams on background workers with serialized media requests.
- Offload playback pause/resume from the UI thread.
- Reduce playback progress polling and skip it during native media opening.
- Defer Home folder scanning, reduce thumbnail processing and cap visible library items.
- Queue and coalesce settings saves instead of waiting for disk I/O on the UI thread.
- Avoid blocking WPF close while native playback tears down.
- Preserve one complete self-contained app, offline codec support, and in-app updates.
# Nova Player — Changelog

## 1.2.0 preview
- Rebranded desktop application, icon, executable, Windows installer and UI as **Nova Player**.
- Simplified desktop UX with Home, Music, Videos, Settings, Updates and About views.
- Enabled immersive fullscreen when opening local videos by default; added switch under Settings.
- Deferred heavy VLC startup until after first window paint for faster perceived launch.
- Preserved old Luma Player library settings with nondestructive first-run migration.
- Kept legacy updater compatibility with a second installer filename containing identical Nova binary.

## Earlier versions
Luma Player 1.1 introduced offline Home video previews and a manual SHA-256 verified updater.
Luma Player 1.0.1 added native VLC startup diagnostics.
