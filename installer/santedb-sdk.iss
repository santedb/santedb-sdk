; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "SanteDB SDK"
#define MyAppPublisher "SanteDB Community"
#define MyAppURL "http://santedb.org"
#ifndef MyAppVersion
#define MyAppVersion "3.0"
#endif 

#ifndef SignKey
#define SignKey "8185304d2f840a371d72a21d8780541bf9f0b5d2"
#endif 

#ifndef SignOpts
#define SignOpts ""
#endif 

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
LicenseFile=..\License.rtf
OutputDir=.\dist                                                                                                   
OutputBaseFilename=santedb-sdk-{#MyAppVersion}
Compression=bzip
SolidCompression=yes
SignedUninstaller=yes
SignTool=default /sha1 {#SignKey} {#SignOpts} /d $qSanteDB iCDR Server$q $f
WizardStyle=modern


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\santedb-tools\bin\Release\sdb-vocab.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-vmu.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-vmu.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\Sample\*.*"; DestDir: "{app}\Sample"; Flags: ignoreversion recursesubdirs
Source: "..\santedb-tools\bin\Release\Schema\*.*"; DestDir: "{app}\Schema"; Flags: ignoreversion recursesubdirs
Source: "..\santedb-tools\bin\Release\sdb-ade.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-ade.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-bb.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-bb.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-bb.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-brd.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-brd.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\sdb-brd.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\pakman.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\lib\*.*"; DestDir: "{app}\lib"; Flags: ignoreversion recursesubdirs
Source: "..\santedb-tools\bin\Release\pakman.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\paksrv.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\paksrv.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\SanteDB.PakMan.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\pakman.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\solution items\cmdprompt.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\release\*.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\vc_redist.x64.exe"; DestDir: "{tmp}"; Flags: dontcopy;
Source: "..\santedb-tools\bin\Release\Antlr3.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\DynamicExpresso.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\Jint.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\solution items\spellfix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\runtimes\*"; DestDir: "{app}\runtimes"; Flags: recursesubdirs
Source: "..\santedb-tools\bin\Release\Microsoft.*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\System.*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\MohawkCollege.Util.Console.Parameters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\libgit2sharp.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "..\santedb-tools\bin\Release\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\Phonix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\RestSrvr.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\santedb-tools\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion

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
    ExtractTemporaryFile('vc_redist.x64.exe');
    Exec(ExpandConstant('{tmp}\vc_redist.x64.exe'), '/install /passive /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;
