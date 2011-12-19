@ECHO OFF
del "%1\..\EBOOT.PBP" "%1.elf" 2> NUL
CALL %~dp0\make.bat %*
IF NOT EXIST "%1.elf" GOTO end
mksfo TESTMODULE "%1\..\param.sfo"
pack-pbp "%1\..\EBOOT.PBP" "%1\..\param.sfo" NUL NUL NUL NUL NUL "%1.elf" NUL > NUL
REM del %1.elf param.sfo
del "%1\..\param.sfo"
:end