#define AppName "Nova Player"
#define AppVersion "1.2.1"
#define AppExeName "NovaPlayer.exe"

[Setup]
; Preserve original AppId so upgrading Luma Player replaces it instead of creating duplicate installs.
AppId={{4AA28F60-3729-4AD0-9B12-5E3D480A3319}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Nova Player Contributors
AppPublisherURL=https://github.com/vihangabhishek/Luma-Player
DefaultDirName={localappdata}\Programs\Nova Player
DefaultGroupName=Nova Player
OutputDir=..\dist
OutputBaseFilename=Nova-Player-Setup-1.2.1
SetupIconFile=..\assets\NovaPlayer.ico
UninstallDisplayIcon={app}\{#AppExeName}
PrivilegesRequired=lowest
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableWelcomePage=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[InstallDelete]
; Remove the obsolete executable after the old player has been closed.
Type: files; Name: "{app}\LumaPlayer.exe"
[Icons]
Name: "{autoprograms}\Nova Player"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"
Name: "{autodesktop}\Nova Player"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch Nova Player"; Flags: nowait postinstall skipifsilent
