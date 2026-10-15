# Nova Player Changelog

## v1.3.0 — Dark UI Redesign (Stability Candidate)

- Fully redesigned dark UI with compact sidebar, persistent top toolbar and playback dock.
- Functional Home, Music, Videos, Settings, Updates and About tabs.
- Full-width, virtualized Music/Videos lists and playable double-click items; player queue only when playing.
- Genuine video fullscreen hides the app toolbar and dock, returning via Esc.
- Improved navigation state and dark accent appearance.
- Safer playback-null guards and asynchronous media cleanup/removal.
- Transient LibVLC initialization failures can be retried.
- Windows automated smoke test exercises all six tabs, plus native audio playback tests.
- Preserves local music/video library, LibVLC codecs, video playback and manual SHA-256 verified updates.
- Preview limitations: installer is not signed; performance on every GPU and movie type not yet tested.

## v1.2.1 — Performance and Responsiveness

- Asynchronous VLC engine loading and media opening, capped video previews, background state saving.

## v1.2.0 — Nova Rebrand

- Nova Player branding, minimal player controls and in-app updates.
