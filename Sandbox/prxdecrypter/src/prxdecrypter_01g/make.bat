@ECHO OFF

set PSPDEV=C:/pspsdk
set PATH=%PATH%;%PSPDEV%/bin

make.exe

DEL *.o
DEL *.SFO
DEL *.ELF

echo.
echo ** GENERATE EXPORTS
echo.

psp-build-exports -s exports.exp

pause