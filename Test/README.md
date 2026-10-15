# Nova Player v1.4.0

**Download:** https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.4.0-preview

The app now renders video through a native WinForms video view, hosted by WPF. The video HWND is connected after the control is shown and the layout is ready, reducing black-screen video failures.

**New interface:** classic, uncluttered Media / Playback / Help menu, Open File/Add Folder buttons, centered black video stage and controls at the bottom. Home, Music, Videos, Settings, Updates and About tabs remain.

**Update:** Updates > Check for updates > Download & install. GitHub release assets are SHA-256 verified before standard interactive setup.

**Windows CI:** .NET compilation, engine loading, PCM audio playback, real generated AVI video playback (HWND attached) and all six WPF tabs checked before installer upload.

**Limitation:** An automated 64x48 AVI test does not ensure every MKV/MP4 or high-resolution GPU playback is correct. Please test on your own PC. The installer is unsigned; never disable security tools to install it.

**Troubleshooting:** %LOCALAPPDATA%/NovaPlayer/logs/startup.log. Remove private paths before sending any log.
