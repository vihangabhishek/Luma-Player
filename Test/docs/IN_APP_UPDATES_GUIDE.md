# Nova Player — Update Guide

Nova Player v1.2.0 can be installed by running `Nova-Player-Setup-1.2.0.exe` from the official [release](https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.2.0-preview).

**In the app**: Open **Updates → Check for updates**. If GitHub provides a newer version and a valid SHA-256 digest, select **Download & install**, approve the download, then approve opening the standard installer. Nova closes before installation. Nothing downloads or installs silently.

**Upgrading from Luma Player**: The original repository URL stays `vihangabhishek/Luma-Player`. Its prior v1.1 update code recognizes the compatibility file `Luma-Player-Setup-1.2.0.exe`. Both setup files are identical. Existing Luma library metadata is read into Nova when Nova has no data yet, and the old state is retained.

**Movies:** Video playback opens fullscreen by default; turn this off in **Settings**. Press **Esc** to leave fullscreen.

**Privacy:** Local videos remain on your PC. Internet is used only when explicitly checking/downloading updates or opening a network media URL.

**Caution:** Preview builds are unsigned. Never disable Windows Defender to install software. Full playback and installer upgrades require user testing on actual Windows PCs.
