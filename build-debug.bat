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
set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will build version %version%
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%


if exist "%nuget%" (
	%msbuild%\msbuild santedb-sdk-ext.sln /t:clean
	%msbuild%\msbuild santedb-sdk-ext.sln /t:restore
	%msbuild%\msbuild santedb-sdk-ext.sln /t:rebuild /p:configuration=Debug /m:1 /p:VersionNumber=%version%

	rem "C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign ".\bin\dist\santedb-sdk-%version%.exe"
	del ".\bin\Debug\*.pak" /s /q
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\config.init" --output=".\bin\Debug\org.santedb.config.init.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\config" --output=".\bin\Debug\org.santedb.config.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\core" --output=".\bin\Debug\org.santedb.core.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\uicore" --output=".\bin\Debug\org.santedb.uicore.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\admin" --output=".\bin\Debug\org.santedb.admin.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\bicore" --output=".\bin\Debug\org.santedb.bicore.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\locales\en" --output=".\bin\Debug\org.santedb.i18n.en.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\locales\fr" --output=".\bin\Debug\org.santedb.i18n.fr.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\locales\es" --output=".\bin\Debug\org.santedb.i18n.es.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --version=%version%  --compile --source="..\applets\locales\sw" --output=".\bin\Debug\org.santedb.i18n.sw.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert --install
	"bin\Debug\pakman.exe" --compose --version=%version%  --source="..\applets\santedb.core.sln.xml" --output=".\bin\Debug\santedb.core.sln.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert
	"bin\Debug\pakman.exe" --compose --version=%version%  --source="..\applets\santedb.admin.sln.xml" --output=".\bin\Debug\santedb.admin.sln.pak" --sign --certHash=f3bea1ee156254656669f00c03eeafe8befc4441 --embedcert
	

) else (	
	echo Cannot find NUGET 
)

:eof