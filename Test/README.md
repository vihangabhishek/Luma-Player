# Nova Player — Windows build guide

**Nova Player v1.3.0** is a single offline-capable WPF + LibVLC music/movie player.

Download the unsigned preview from https://github.com/vihangabhishek/Luma-Player/releases/tag/v1.3.0-preview.

## UI
Home / Music / Videos / Settings / Updates / About. Music and Videos now have full-width scrollable lists. Home shows local preview cards, and the video player has its own playlist. Movies open in fullscreen by default; Esc exits and Settings can disable auto-fullscreen.

## Update from inside the app
Open Updates, click Check for updates, approve Download & install, then finish the normal Windows Setup wizard. Checksums are SHA-256 verified from official GitHub release metadata. Updates are never silently installed.

## Development
Restore the project with "dotnet restore Test/src/NovaPlayer/NovaPlayer.csproj -r win-x64".
Publish with "dotnet publish Test/src/NovaPlayer/NovaPlayer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o Test/publish".

Windows CI checks compilation, real LibVLC WAV playback, WPF startup and navigation through six tabs. It does not guarantee flawless 4K playback on every GPU. The installer remains unsigned and includes the desktop runtime and LibVLC files.

The repository name stays Luma-Player for older updater compatibility.
