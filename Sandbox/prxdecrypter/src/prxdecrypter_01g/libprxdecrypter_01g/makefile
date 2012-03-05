#
# PSP kernel modules 
#
TARGET_LIB = libprxdecrypter_01g.a

OBJS = \
 API_0000.o\

USE_KERNEL_LIBC = 1

INCDIR = 
CFLAGS = -O2 -G0 -Wall
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak

$(OBJS): pspAPI.S
	$(CC) -DF_$* $< -c -o $@

