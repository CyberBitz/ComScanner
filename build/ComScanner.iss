#ifndef AppVersion
  #error AppVersion must be supplied to ISCC.
#endif
#ifndef FileVersion
  #error FileVersion must be supplied to ISCC.
#endif
#ifndef PublishDir
  #error PublishDir must be supplied to ISCC.
#endif
#ifndef OutputDir
  #error OutputDir must be supplied to ISCC.
#endif

[Setup]
AppId={{A88A04EA-8B48-4B0B-BD84-C2545303A73A}
AppName=ComScanner
AppVersion={#AppVersion}
AppVerName=ComScanner {#AppVersion}
AppPublisher=CyberBitz
AppPublisherURL=https://github.com/CyberBitz/ComScanner
AppSupportURL=https://github.com/CyberBitz/ComScanner/issues
AppUpdatesURL=https://github.com/CyberBitz/ComScanner/releases
DefaultDirName={localappdata}\Programs\ComScanner
DefaultGroupName=ComScanner
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile={#PublishDir}\LICENSE.txt
OutputDir={#OutputDir}
OutputBaseFilename=ComScanner-{#AppVersion}-win-x64-Setup
VersionInfoVersion={#FileVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\ComScanner.exe
CloseApplications=yes
RestartApplications=no

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\ComScanner"; Filename: "{app}\ComScanner.exe"
Name: "{autodesktop}\ComScanner"; Filename: "{app}\ComScanner.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\ComScanner.exe"; Description: "Launch ComScanner"; Flags: nowait postinstall skipifsilent
