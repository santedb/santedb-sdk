@echo off
set path=%path%;%cd%
cd "%1"

echo SanteDB Software Development Kit
echo =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
echo.
echo There are several tools which are useful for debugging SanteDB:
echo.
echo	sdb-bb		-	A tool for extracting files from connected Android devices (requires ADB on path)
echo	adb-ade		-	A tool which allows you to debug your applets in real time in an edit/save/refresh cycle
echo	sdb-dbg		-	Debugging tool for business rules and clinical protocols
echo	pakman		-	A tool for packaging your applet files for distribution
echo	logviewer	-	A tool which opens a graphical tool for viewing/search log files
echo Successfully added to path..