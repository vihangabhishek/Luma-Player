# Windows 11 quality assurance checklist

- [ ] Build from GitHub Actions, installer completes without PowerShell launching.
- [ ] App starts offline on a clean Win11 x64 user account.
- [ ] MP4 H.264 and MKV H.265, including 1080p and 4K test files where supported by hardware.
- [ ] MP3, FLAC, WAV and AAC playback with no audio/video track crosstalk.
- [ ] Seek forward/back, pause, resume after restart, wrong/missing file detection.
- [ ] Open files, folder import, drag/drop, favorites, library search and filtering.
- [ ] Shuffle, repeat off/all/one, speed, aspect ratio, PNG screenshot, external .srt subtitles.
- [ ] Test fullscreen/windowed, secondary monitor, 100/125/150% DPI.
- [ ] Change themes, restart, confirm state persists.
- [ ] Uninstall/reinstall, confirm user library state survives (and can be removed manually).
- [ ] Measure CPU, GPU, RAM and dropped frames using Windows Task Manager and representative test media.
- [ ] Test low-spec 4 GB RAM PC separately. Do not imply 4K video can play on every machine.
- [ ] Verify native LibVLC binaries and licenses are packaged with the setup.
- [ ] Confirm Windows lock-screen/SMTC is **not yet implemented**, and don't advertise it as active.
