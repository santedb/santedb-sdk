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
;SignedUninstaller=yes
;SignTool=default

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: ".\bin\Release\AjaxMin.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Antlr3.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\BouncyCastle.Crypto.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\ExpressionEvaluator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-vmu.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\jint.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\libeay32md.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\MohawkCollege.Util.Console.Parameters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Mono.Data.Sqlite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Sample\*.*"; DestDir: "{app}\Sample"; Flags: ignoreversion recursesubdirs
Source: ".\bin\Release\Schema\*.*"; DestDir: "{app}\Schema"; Flags: ignoreversion recursesubdirs
Source: ".\bin\Release\SanteDB.BusinessRules.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Cdss.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Api.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Applets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Model.AMI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Model.RISI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Core.Model.ViewModelSerializers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.DisconnectedClient.Ags.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.DisconnectedClient.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.DisconnectedClient.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.DisconnectedClient.Xamarin.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.DisconnectedClient.i18n.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Messaging.AMI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Messaging.HDSI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Messaging.RISI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.ReportR.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Rest.AMI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\RestSrvr.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Rest.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Rest.HDSI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-ade.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-ade.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-ade.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-bb.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sdb-brd.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\pakman.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SharpCompress.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SqlCipher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SQLite.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SQLite.Net.Platform.SqlCipher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\sqlite3.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\System.Data.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\System.Transactions.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\zlib.net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\Phonix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.BI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Matcher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SanteDB.Rest.BIS.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SQLite.Net.Platform.Generic.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\SQLite.Net.Platform.Win32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\Release\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\tools\cmdprompt.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\installsupp\*.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\tools\vcredist_x86.exe"; DestDir: "{tmp}"; Flags: dontcopy;

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
    Exec(ExpandConstant('{tmp}\vcredist_x86.exe'), '/install /passive', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;
