@echo off
REM http://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/for.mspx?mfr=true

FOR /F "usebackq delims==" %%i IN (`dir *.expected /S /B`) DO (
	CALL %~dp0\make.bat %%~dpi%%~ni
)
