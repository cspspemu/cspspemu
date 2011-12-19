@ECHO OFF
CALL ..\prepare.bat
SET BASE_FILE=atractest
SET C_FILES=%BASE_FILE%.c
SET ELF_FILE=%BASE_FILE%.elf
psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 %C_FILES% -lpspdebug -lpspge -lpspdisplay -lpspsdk -lc -lpspuser -lpspkernel -lpspatrac3 -lpspaudio -o %ELF_FILE%
IF EXIST %ELF_FILE% (
	psp-fixup-imports %ELF_FILE%
	mksfo "%BASE_FILE%" PARAM.SFO
	pack-pbp EBOOT.PBP PARAM.SFO NUL NUL NUL NUL NUL %ELF_FILE% NUL
)