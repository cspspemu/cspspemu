@ECHO OFF
CALL %~dp0\make_eboot.bat %*

IF EXIST EBOOT.PBP (
	copy EBOOT.PBP K:\PSP\GAME\test\EBOOT.PBP /Y
	EBOOT.PBP
)
