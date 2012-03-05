@ECHO OFF

set PSPDEV=C:/pspsdk
set PATH=%PATH%;%PSPDEV%/bin

make.exe

DEL *.o

SET DRIVE=.\

pause