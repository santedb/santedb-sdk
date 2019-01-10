@echo off

set version=%1


if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
        echo will use VS 2017 build tools
        set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
) else (
	if exist "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
        echo will use VS 2017 build tools
        set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
	) else ( 
		echo Unable to locate VS 2017 build tools, will use default build tools 
		goto :eof
	)
)

if exist "c:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
	set inno="c:\Program Files (x86)\Inno Setup 5\ISCC.exe"
) else (
	echo Can't Find INNO Setup Tools
	goto :eof
)

set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will build version %version%
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%


if exist %nuget% (
	%msbuild% santedb-sdk-ext.sln /t:restore
	%msbuild% santedb-sdk-ext.sln /t:clean /t:rebuild /p:configuration=Release /m

	FOR /R %cwd% %%G IN (*.nuspec) DO (
		echo Packing %%~pG
		pushd %%~pG
		rem %nuget% pack -OutputDirectory %localappdata%\NugetStaging -prop Configuration=Release
		popd
	)

	FOR /R %cwd%\bin\Release %%G IN (*.exe) DO (
		echo Signing %%G
		"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign "%%G"
	)

	FOR /R %cwd%\bin\Release %%G IN (*.dll) DO (
		echo Signing %%G
		"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign "%%G"
	)
	
	rem "C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign ".\bin\dist\santedb-sdk-%version%.exe"
	
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\config.init" --output=".\installsupp\org.santedb.config.init.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\config" --output=".\installsupp\org.santedb.config.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\core" --output=".\installsupp\org.santedb.core.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\uicore" --output=".\installsupp\org.santedb.uicore.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\i18n\en" --output=".\installsupp\org.santedb.i18n.en.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\i18n\fr" --output=".\installsupp\org.santedb.i18n.fr.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\i18n\es" --output=".\installsupp\org.santedb.i18n.es.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert
	"c:\Program Files\SanteSuite\SanteDB\SDK\sdb-pakr.exe" --optimize --compile --source="..\applets\i18n\sw" --output=".\installsupp\org.santedb.i18n.sw.pak" --keyFile="..\..\keys\org.openiz.core.pfx" --keyPassword="..\..\keys\org.openiz.core.pass" --embedcert

	%inno% "/o.\bin\dist" ".\install.iss" /d"MyAppVersion=%version%"

	"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign ".\bin\dist\santedb-sdk-%version%.exe"
	
	rem ################# TARBALLS 
	echo Building Linux Tarball

	mkdir santedb-sdk-%version%
	cd santedb-sdk-%version%
	copy "..\bin\Release\*.dll"
	copy "..\bin\Release\*.exe"
	copy "..\bin\Release\jint.dll"
	copy "..\installsupp"
	xcopy /I "..\bin\Release\Schema\*.*" ".\Schema"
	xcopy /I "..\bin\Release\Sample\*.*" ".\Sample"
	cd ..
	"C:\program files\7-zip\7z" a -r -ttar .\bin\dist\santedb-sdk-%version%.tar .\santedb-sdk-%version%
	"C:\program files\7-zip\7z" a -r -tzip .\bin\dist\santedb-sdk-%version%.zip .\santedb-sdk-%version%
	"C:\program files\7-zip\7z" a -tbzip2 .\bin\dist\santedb-sdk-%version%.tar.bz2 .\bin\dist\santedb-sdk-%version%.tar
	"C:\program files\7-zip\7z" a -tgzip .\bin\dist\santedb-sdk-%version%.tar.gz .\bin\dist\santedb-sdk-%version%.tar
	del /q /s .\installsupp\*.* 
	del /q /s .\santedb-sdk-%version%\*.*
	rmdir .\santedb-sdk-%version%\schema
	rmdir .\santedb-sdk-%version%\sample
	rmdir .\santedb-sdk-%version%
	rmdir .\installsupp

) else (	
	echo Cannot find NUGET 
)

:eof