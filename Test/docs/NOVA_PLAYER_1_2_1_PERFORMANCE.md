# Nova Player 1.2.1 — Performance, Stability & Updates

One Windows video + music app. One full self-contained installer. This update focuses on reducing Windows **Not Responding** events without removing codecs or requiring separate runtime downloads.

## Changes
- Native VLC initialization now runs on a background worker, instead of blocking the WPF message loop.
- Opening local videos, audio files and supported network streams runs on a worker and is serialized so rapid file selection does not overlap native open operations.
- Pause/resume is moved off the window thread.
- Playback timeline refreshes at 750 ms instead of 300 ms and skips native polling while a media file is opening.
- The Home page does not start disk scans before its first render, and limits Windows-generated thumbnails.
- The on-screen library list is capped at 650 items to avoid creating an unbounded number of WPF visual items (the saved library is not deleted).
- Player-state snapshots are written to disk in a coalescing background queue and flushed on normal shutdown.
- Native player stop/disposal no longer blocks the closing window.

## Upgrade inside Nova Player
From Nova Player 1.2.0, open **Updates**, select **Check for updates**, then **Download & install**. The downloaded Nova setup is checked against the official GitHub SHA-256 digest before prompting you to launch the ordinary Windows installer. The app closes to let the installer proceed. These are unsigned preview installers: respect Windows protection prompts and do not disable Microsoft Defender.

## Known limitations
- Windows GPU driver, media codec, resolution and file location affect playback speed. The app cannot guarantee perfectly smooth 4K playback on every PC.
- The app still bundles the complete LibVLC engine plus .NET runtime to support one straightforward offline installer; its file size cannot safely shrink dramatically without changing these requirements.
- The Windows CI validates compilation, native LibVLC initialization, offline PCM WAV playback and WPF window startup. Physical-Windows video playback, subtitle combinations, GPU drivers and upgrades still need end-user testing.

## Troubleshooting
If the app still freezes, note whether the freeze occurs during initial launch, folder scan, opening a video, fullscreen transition, or closing. Find the local log at `%LOCALAPPDATA%\NovaPlayer\logs\startup.log`. Redact personal file paths before sharing.
