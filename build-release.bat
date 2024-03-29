@echo off

set version=%1

		if exist "c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\15.0\Bin\MSBuild.exe" (
	        	echo will use VS 2019 Community build tools
        		set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\15.0\Bin"
		) else ( 
			if exist "c:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
        			set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin"
	        		echo will use VS 2019 Pro build tools
			) else (
				echo Unable to locate VS 2019 build tools, will use default build tools on path
			)
		)

if exist "c:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
	set inno="c:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else (
	if exist "c:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
		set inno="c:\Program Files (x86)\Inno Setup 5\ISCC.exe"
	) else (
		echo Can't Find INNO Setup Tools
		goto :eof
	)
)

set signtool="C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64\signtool.exe"
set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will build version %version%
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%


if exist "%nuget%" (
	%msbuild%\msbuild santedb-sdk-ext.sln /t:clean
	%msbuild%\msbuild santedb-sdk-ext.sln /t:restore
	%msbuild%\msbuild santedb-sdk-ext.sln /t:rebuild /p:configuration=Release /m:1 /p:VersionNumber=%version%

	FOR /R "%cwd%\bin\Release" %%G IN (*.exe) DO (
		echo Signing %%G
		%signtool% sign /sha1 a11164321e30c84bd825ab20225421434622c52a /d "SanteDB SDK" "%%G"
	)

	FOR /R "%cwd%\bin\Release" %%G IN (SanteDB*.dll) DO (
		echo Signing %%G
		%signtool% sign /sha1 a11164321e30c84bd825ab20225421434622c52a /d "SanteDB SDK" "%%G"
	)
	
	FOR /R "%cwd%" %%G IN (*.nuspec) DO (
		echo Packing %%~pG
		pushd "%%~pG"
		if exist "packages.config" (
			%nuget% restore -SolutionDirectory ..\
		)
		if [%2] == [] (
			%nuget% pack -OutputDirectory "%localappdata%\NugetStaging" -prop Configuration=Release 
		) else (
			echo Publishing NUPKG
			%nuget% pack -prop Configuration=Release 
			FOR /R %%F IN (*.nupkg) do (
				%nuget% push "%%F" -Source https://api.nuget.org/v3/index.json -ApiKey %2
			)
		) 
		popd
	)

	
	rem "C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign ".\bin\dist\santedb-sdk-%version%.exe"
	del ".\bin\Release\*.pak" /s /q
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\config.init" --output=".\bin\release\org.santedb.config.init.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\config" --output=".\bin\release\org.santedb.config.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\core" --output=".\bin\release\org.santedb.core.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\uicore" --output=".\bin\release\org.santedb.uicore.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\admin" --output=".\bin\release\org.santedb.admin.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\bicore" --output=".\bin\release\org.santedb.bicore.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\locales\en" --output=".\bin\release\org.santedb.i18n.en.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\locales\fr" --output=".\bin\release\org.santedb.i18n.fr.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\locales\es" --output=".\bin\release\org.santedb.i18n.es.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --version=%version% --optimize --compile --source="..\applets\locales\sw" --output=".\bin\release\org.santedb.i18n.sw.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\release\pakman.exe" --compose --version=%version% --optimize --source="..\applets\santedb.core.sln.xml" --output=".\bin\release\santedb.core.sln.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert
	"bin\release\pakman.exe" --compose --version=%version% --optimize --source="..\applets\santedb.admin.sln.xml" --output=".\bin\release\santedb.admin.sln.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert
	%inno% "/o.\bin\dist" ".\santedb-sdk.iss" /d"MyAppVersion=%version%"
	
	rem ################# TARBALLS 
	echo Building Linux Tarball

	mkdir santedb-sdk-%version%
	cd santedb-sdk-%version%
	copy "..\bin\Release\*.dll"
	copy "..\bin\Release\*.exe"
	copy "..\bin\Release\*.pak"
	copy "..\bin\Release\lib\win32\x86\git2-106a5f2.dll"
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
	rmdir .\santedb-sdk-%version%\distribution
	rmdir .\santedb-sdk-%version%
	rmdir .\installsupp

) else (	
	echo Cannot find NUGET 
)

:eof