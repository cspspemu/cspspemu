@ECHO OFF
CALL ..\prepare.bat
psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 prxloader.c MyLib.S -lpspdebug -lpspge -lpspdisplay -lpspsdk -lc -lpspuser -lpspkernel -o prxloader.elf
IF EXIST prxloader.elf psp-fixup-imports prxloader.elf

psp-gcc -I%PSPSDK%/psp/sdk/include/libc -I. -I%PSPSDK%/psp/sdk/include -O2 -G0 -Wall -D_PSP_FW_VERSION=150   -c -o mymodule.o mymodule.c
psp-build-exports -b mymodule_exports.exp > mymodule_exports.c
psp-gcc -I%PSPSDK%/psp/sdk/include/libc -I. -I%PSPSDK%/psp/sdk/include -O2 -G0 -Wall -D_PSP_FW_VERSION=150   -c -o mymodule_exports.o mymodule_exports.c
psp-gcc -I%PSPSDK%/psp/sdk/include/libc -I. -I%PSPSDK%/psp/sdk/include -O2 -G0 -Wall -D_PSP_FW_VERSION=150  -L. -L%PSPSDK%/psp/sdk/lib -specs=%PSPSDK%/psp/sdk/lib/prxspecs -Wl,-q,-T%PSPSDK%/psp/sdk/lib/linkfile.prx   mymodule.o mymodule_exports.o  -lpspdebug -lpspdisplay -lpspge -lpspctrl -lpspsdk -lpsplibc -lpspnet -lpspnet_inet -lpspnet_apctl -lpspnet_resolver -lpsputility -lpspuser -lpspkernel -o mymodule.elf
psp-fixup-imports mymodule.elf
psp-prxgen mymodule.elf mymodule.prx
REM rm mymodule_exports.c