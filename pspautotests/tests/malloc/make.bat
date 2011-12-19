@ECHO OFF
CALL ..\prepare.bat
psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 malloc.c -lpspdebug -lpspge -lpspdisplay -lpspsdk -lc -lpspuser -lpspkernel -o malloc.elf
IF EXIST malloc.elf (
	psp-fixup-imports malloc.elf
)