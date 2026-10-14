# Luma Player v1.1.0 — Windows Update Guide

## New Home page

The Home tab searches only your own Videos folder, top level of Downloads, and videos already in your player library. You can add other folders through **Add folder**. Video thumbnails use the Windows Explorer thumbnail providers, so some files may show a fallback ▶ icon instead of a scene preview. Playback remains offline through LibVLC.

## Updating inside the application

1. Install v1.1.0 once using the release Setup.exe (older versions cannot update themselves).
2. Open Luma Player and click **Check for updates** in the left sidebar when internet is available.
3. If a newer official release exists, click **Download & install**.
4. Approve the download. Luma Player verifies the downloaded installer against its SHA-256 digest published by GitHub.
5. Approve the final confirmation. The player closes and opens the standard Windows installer wizard.
6. Complete the wizard, then relaunch Luma Player.

The app does not auto-download or silently install updates. Updates without a verified SHA-256 digest are rejected. SmartScreen may warn about the unsigned preview installer; do not disable Windows Defender.

## Test checklist for Windows 11

- Launch Luma Player and verify Home appears with no startup errors.
- Verify local Videos files appear; wait a little for thumbnails (where supported).
- Click a video card and play/pause the video.
- Add a folder not in Videos/Downloads and confirm newly imported videos appear on Home.
- Run **Check for updates**. If v1.1.0 is the newest release, it should say up to date.
- Verify themes and the music library still work.
- Check for updates after a future release to test download and install. Back up any library metadata before upgrading.

This is a technical preview. Only startup has been exercised automatically on a hosted Windows machine, not full playback and upgrade installation on a physical end-user Windows 11 PC.
