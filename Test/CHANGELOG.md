# Changelog

## v1.1.0 — Glass Home and Secure In-App Updates

- Introduced an offline Home screen showing local video cards and previews obtained from Windows Explorer when supported.
- Automatically discovers the current user's Videos folder, top-level Downloads, and imported library videos. Bounded background scanning avoids full-disk indexing.
- Added manual update checks and a confirmation-based update installer workflow using GitHub Releases and SHA-256 digest verification.
- Added WPF user-interface startup testing to the Windows build workflow.
- Preserved the native LibVLC video playback engine, library data, custom themes, installer, and privacy-focused local operation.
- Limitations: Explorer may not have a thumbnail handler for every file; fallback play icon displayed. Lock-screen transport controls are not yet implemented.
- Unsigned test release; test on a physical Windows 11 PC before relying on it.


## 1.0.0 — initial technical preview

- Native Windows 11 WPF UI with glass-inspired blue dashboard and color themes.
- LibVLC engine, hardware decode request, file/folder import, basic HTTP/HTTPS streaming.
- Searchable library with Videos, Music, Favorites and Recently Played views.
- Queue navigation, playback speed, aspect ratio, subtitles, screenshots and keyboard shortcuts.
- Shuffle/three-state repeat, resume position, local settings/history, drag-and-drop and fullscreen.
- GitHub Actions Windows self-contained setup build, Inno Setup installer, app icon.

### Still to implement

- Windows system media transport/lock-screen integration (SMTC).
- Audio equalizer, subtitle/audio track dropdowns, advanced HDR metadata display and smart library covers.
- Gapless transitions, casting, DVD menus, media tags editing and a tested auto-update channel.
