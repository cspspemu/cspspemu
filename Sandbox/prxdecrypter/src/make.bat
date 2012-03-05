@ECHO OFF

set PSPDEV=C:/pspsdk
set PATH=%PATH%;%PSPDEV%/bin

make.exe clean
make.exe

DEL *.o
DEL *.SFO
del *.elf

md PRXdecrypter
copy prxdecrypter_01g\prxdecrypter_01g.prx PRXdecrypter\prxdecrypter_01g.prx
copy prxdecrypter_02g\prxdecrypter_02g.prx PRXdecrypter\prxdecrypter_02g.prx
move eboot.pbp PRXdecrypter\EBOOT.PBP

pause