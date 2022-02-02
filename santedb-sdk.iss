; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "SanteDB SDK"
#define MyAppPublisher "SanteDB Community"
#define MyAppURL "http://santedb.org"
[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8DE5D303-441E-4742-970A-C2C240C89BD2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf64}\SanteSuite\SanteDB\SDK
DisableProgramGroupPage=yes
LicenseFile=.\License.rtf
OutputDir=.\dist                                                                                                   
OutputBaseFilename=santedb-sdk-{#MyAppVersion}
Compression=bzip
SolidCompression=yes
SignedUninstaller=yes
SignTool=default sign /a /n $qFyfe Software$q /d $q{#MyAppName}$q $f
WizardStyle=modern


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: ".\bin\Release\sdb-vocab.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-vmu.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-vmu.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Sample\*.*"; DestDir: "{app}\Sample"; Flags: ignoreversion recursesubdirs
Source: ".\bin\Release\Schema\*.*"; DestDir: "{app}\Schema"; Flags: ignoreversion recursesubdirs
Source: ".\bin\Release\importer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-fake.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-ade.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-ade.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-backup.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\lib\win32\x86\git2-106a5f2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\paksrv.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\paksrv.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\tools\cmdprompt.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\release\*.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\tools\vcredist_x86.exe"; DestDir: "{tmp}"; Flags: dontcopy;
Source: ".\bin\Release\Antlr3.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\DynamicExpresso.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Jint.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\libcrypto-1_1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\spellfix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Microsoft.Win32.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\MohawkCollege.Util.Console.Parameters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Mono.Data.Sqlite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\libgit2sharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Phonix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\RestSrvr.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\fr\SanteDB.DisconnectedClient.i18n.resources.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{commonprograms}\SanteDB\SanteDB SDK Command Prompt"; Filename: cmd; Parameters: "/k ""{app}\cmdprompt.cmd"""; WorkingDir: "{app}"

[Code]
function PrepareToInstall(var needsRestart:Boolean): String;
var
  hWnd: Integer;
  ResultCode : integer;
  uninstallString : string;
begin
    EnableFsRedirection(true);
    ExtractTemporaryFile('vcredist_x86.exe');
    Exec(ExpandConstant('{tmp}\vcredist_x86.exe'), '/install /passive /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;
