#define AppName "Luma Player"
#define AppVersion "1.0.1"
[Setup]
AppId={{4AA28F60-3729-4AD0-9B12-5E3D480A3319}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Luma Player Contributors
AppPublisherURL=https://github.com/
DefaultDirName={localappdata}\Programs\Luma Player
DefaultGroupName=Luma Player
OutputDir=..\dist
OutputBaseFilename=Luma-Player-Setup-1.0.1
SetupIconFile=..\assets\LumaPlayer.ico
UninstallDisplayIcon={app}\LumaPlayer.exe
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
[Icons]
Name: "{autoprograms}\Luma Player"; Filename: "{app}\LumaPlayer.exe"; IconFilename: "{app}\LumaPlayer.exe"
Name: "{autodesktop}\Luma Player"; Filename: "{app}\LumaPlayer.exe"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
[Run]
Filename: "{app}\LumaPlayer.exe"; Description: "Launch Luma Player"; Flags: nowait postinstall skipifsilent
