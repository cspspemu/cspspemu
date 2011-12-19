@ECHO OFF
CALL ..\prepare.bat
psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 string.c ../emits.c -lpspdebug -lpspge -lpspdisplay -lpspsdk -lc -lpspuser -lpspkernel -o string.elf
IF EXIST string.elf (
	psp-fixup-imports string.elf
)