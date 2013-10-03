@echo off
SET COMPLUS_AltJit=*
REM HKLM\SOFTWARE\Microsoft\.NETFramework\AltJit to the string "*".
cls && color && cd "%~dp0" && "%~dp0\CSPspEmu\bin\Debug\CSPspEmu.exe" %*
