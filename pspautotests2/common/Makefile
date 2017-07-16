TARGET_LIB = libcommon.a
OBJS = common.o vram.o sysmem-imports.o

INCDIR = 
CFLAGS = -O0 -G 0 -g -Wall
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)

LIBDIR =
LDFLAGS = -G0

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
