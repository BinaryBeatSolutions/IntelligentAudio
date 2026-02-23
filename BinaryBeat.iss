[Setup]
AppName=BinaryBeat IntelligentAudio
AppVersion=1.0
AppMutex=BinaryBeat_Mutex_Unique_ID
DefaultDirName={autopf}\BinaryBeat
DefaultGroupName=BinaryBeat
OutputDir=.\InstallerOutput
OutputBaseFilename=BinaryBeatSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin


[Files]
; Skicka bara med själva programmet och dess drivrutiner/DLL:er
; Denna rad tar med BinaryBeat.exe OCH hela runtimes-trädet automatiskt
Source: "D:\Projects\BinaryBeat.Audio\Package\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Ableton-amxd
Source: "D:\Projects\BinaryBeat.Audio\Ableton\BinaryBeat.amxd"; DestDir: "{userdocs}\Ableton\User Library\Presets\Audio Effects\Max Audio Effect"; Flags: ignoreversion


; Whisper-model, not downloaded yet
//Source: "D:\Projects\BinaryBeat.Audio\BinaryBeat\bin\Release\net8.0\Models\ggml-tiny.en.bin"; DestDir: "{app}\Models"; Flags: ignoreversion

; Max for Live-enheten (Ableton User Library)
; Vi placerar den där Ableton letar efter egna enheter
Source: "D:\Projects\BinaryBeat.Audio\Ableton\BinaryBeat.amxd"; DestDir: "{userdocs}\Ableton\User Library\Presets\Audio Effects\Max Audio Effect"; Flags: ignoreversion

[Icons]
Name: "{group}\BinaryBeat"; Filename: "{app}\BinaryBeat.exe"
Name: "{userstartup}\BinaryBeat"; Filename: "{app}\BinaryBeat.exe"; Comment: "Start BinaryBeat with Windows"

[Run]
; Starta appen direkt efter installation
Filename: "{app}\BinaryBeat.exe"; Description: "Launch BinaryBeat Engine"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\BinaryBeat"