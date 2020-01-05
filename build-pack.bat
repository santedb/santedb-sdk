@echo off

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
        echo will use VS 2017 Enterprise build tools
        set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
) else (
	if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" (
        	echo will use VS 2017 Professional build tools
	        set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
	) else (
		if exist "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
	        echo will use VS 2017 Community build tools
        	set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
		) else ( echo Unable to locate VS 2017 build tools, will use default build tools )
	)
)

set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%

%msbuild% santedb-sdk-ext.sln /t:clean /t:restore /t:build /p:configuration=debug /m
%msbuild% santedb-sdk-ext.sln /t:build /p:configuration=debug /m

FOR /R "%cwd%" %%G IN (*.nuspec) DO (
	echo Packing %%~pG
	pushd "%%~pG"
	%nuget% pack -OutputDirectory "%localappdata%\NugetStaging" -prop Configuration=Debug -symbols
	popd
)