@ECHO OFF
CALL ..\prepare.bat
REM LIBS = -lstdc++ -lSDL_ttf -lfreetype -lSDL_image -ljpeg -lpng -lz -lm -lSDL_mixer -lvorbisidec -lmikmod -lmad $(shell $(SDL_CONFIG) --libs)
REM LIBS += -lpspaudiolib -lpspaudio -lpsppower
SET PSPSDK=E:\projects\pspemu\dev\pspsdk
SET LIBS=
REM SET LIBS=%LIBS% -lSDL_mixer -lsmpeg -lmikmod -lmad -lstdc++ -lpspaudiolib -lpspaudio -lpsppower 
REM SET LIBS=%LIBS% -lSDL_mixer -lsmpeg -lstdc++ -lvorbisidec -logg -lmikmod
SET LIBS=%LIBS% -lSDL_mixer -lsmpeg -lstdc++ -lvorbisidec -logg
SET LIBS=%LIBS% -lSDLmain -lSDL -lm -lGL -lpspvfpu -lpspirkeyb -lpsppower -lpspdebug -lpspgu -lpspctrl -lpspge -lpspdisplay -lpsphprm -lpspsdk -lpsprtc -lpspaudio -lc -lpspuser -lpsputility -lpspkernel -lpspnet_inet
"%PSPSDK%\bin\psp-gcc" -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" -D_PSP_FW_VERSION=150 -Wall -g -O0 main.c %LIBS% -o main.elf
IF EXIST main.elf (
	"%PSPSDK%\bin\psp-fixup-imports" main.elf
)