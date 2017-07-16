TARGET = lesson3
OBJS = main.o

CFLAGS = -O2 -G0 -Wall
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)

PSPBIN = $(PSPSDK)/../bin
CFLAGS += -I$(PSPSDK)/../include  -fsingle-precision-constant -g -Wall -O2 
LIBS += -lglut -lGLU -lGL -lm -lc -lpsputility -lpspdebug -lpspge -lpspdisplay -lpspctrl -lpspsdk -lpspvfpu -lpsplibc -lpspuser -lpspkernel -lpsprtc -lpsppower -lstdc++
LDFLAGS += -DMODULE_NAME="lesson3" psp-setup.c


EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = LESSON3
# PSP_EBOOT_ICON = hero.png
# PSP_EBOOT_PIC1 = bg.png


PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak