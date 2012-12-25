TARGET = main
OBJS = main.o
PSP_EBOOT_TITLE = vfpu test 

BUILD_PRX = 1
PSP_FW_VERSION = 500

PSP_DRIVE = /cygdrive/j
USE_PSPSDK_LIBC = 1

CFLAGS = -g -G0 -Wall -O0 -fno-strict-aliasing
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti 
ASFLAGS = $(CFLAGS)

LDFLAGS = -G0
LIBS= -lGLUT -lGL -lpspvfpu -lpspgu -lpspgum -lpsprtc -lpspctrl -lpspmath -lc -lm

EXTRA_TARGETS = EBOOT.PBP
#PSP_EBOOT_ICON = icon.png

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
