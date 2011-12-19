@ECHO OFF
CALL %~dp0\prepare.bat
SET BASE_FILE=%1
IF EXIST "%~dp0\..\..\pspsdk\bin\psp-gcc.exe" (
	SET PSPSDK=%~dp0\..\..\pspsdk
) ELSE (
	SET PSPSDK=C:\pspsdk
)
SET C_FILES=
SET C_FILES=%C_FILES% "%~dp0/../common/common.c" 
SET C_FILES=%C_FILES% %BASE_FILE%.c
SET ELF_FILE=%BASE_FILE%.elf
SET PRX_INFO=
REM SET PRX_INFO=%PRX_INFO% -specs=%PSPSDK%/psp/sdk/lib/prxspecs -Wl,-q,-T%PSPSDK%/psp/sdk/lib/linkfile.prx
SET PSP_LIBS=
SET PSP_LIBS=%PSP_LIBS% -lpspumd
SET PSP_LIBS=%PSP_LIBS% -lpsppower
SET PSP_LIBS=%PSP_LIBS% -lpspdebug
REM SET PSP_LIBS=%PSP_LIBS% -lpspgum
SET PSP_LIBS=%PSP_LIBS% -lpspgum_vfpu
SET PSP_LIBS=%PSP_LIBS% -lpspgu
SET PSP_LIBS=%PSP_LIBS% -lpspge
SET PSP_LIBS=%PSP_LIBS% -lpspdisplay
SET PSP_LIBS=%PSP_LIBS% -lpspsdk
SET PSP_LIBS=%PSP_LIBS% -lm
SET PSP_LIBS=%PSP_LIBS% -lc
SET PSP_LIBS=%PSP_LIBS% -lpspnet
SET PSP_LIBS=%PSP_LIBS% -lpspnet_inet
SET PSP_LIBS=%PSP_LIBS% -lpspuser
REM SET PSP_LIBS=%PSP_LIBS% -lpspkernel
SET PSP_LIBS=%PSP_LIBS% -lpsprtc
SET PSP_LIBS=%PSP_LIBS% -lpspctrl

SET PATH=%PSPSDK%\bin;%PATH%

PUSHD %~dp1

	ECHO %BASE_FILE%

	IF EXIST make_prepare.bat (
		CALL make_prepare.bat
	)

	"%PSPSDK%\bin\psp-gcc" -I. -I"%PSPSDK%/psp/sdk/include" -I"%~dp0/../common" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 %C_FILES% %PRX_INFO% %PSP_LIBS% -o %ELF_FILE%

	IF EXIST %ELF_FILE% (
		"%PSPSDK%\bin\psp-fixup-imports" %ELF_FILE%
	)

POPD