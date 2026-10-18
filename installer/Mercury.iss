; Inno Setup script for the Mercury installer. Build it with installer\build.ps1, which
; publishes the app first. Compile by hand with: ISCC installer\Mercury.iss
#define AppName "Mercury"
#define AppVersion "0.2.0"
#define AppExe "Mercury.Desktop.exe"
#define Publish "..\publish"

[Setup]
AppId={{6F0B8A52-3B8D-4C1B-9E55-7D1C4A3E2F10}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Mercury
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=Mercury-Setup
SetupIconFile=..\Resources\App.ico
UninstallDisplayIcon={app}\{#AppExe}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Installs for the current user without asking for admin rights; the person can choose all users.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "{#Publish}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
; Makes mercury:// links open the app, so an invitation link joins straight away.
Root: HKA; Subkey: "Software\Classes\mercury"; ValueType: string; ValueData: "URL:Mercury"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\mercury"; ValueName: "URL Protocol"; ValueType: string; ValueData: ""
Root: HKA; Subkey: "Software\Classes\mercury\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExe}"" ""%1"""

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch Mercury"; Flags: nowait postinstall skipifsilent
