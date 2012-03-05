TARGET = prxdecrypter_01g
OBJS = main.o 

INCDIR = ../include
CFLAGS = -O2 -G0 -Wall -fno-pic
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS = $(CFLAGS)

PSP_FW_VERSION = 150

BUILD_PRX = 1
PRX_EXPORTS = exports.exp

LIBDIR =
LDFLAGS = -mno-crt0 -nostartfiles
LIBS =

#USE_KERNEL_LIBC = 1
#USE_KERNEL_LIBS = 1

#LIBDIR = ../lib
#LDFLAGS = -mno-crt0 -nostartfiles
#LIBS = -lpspsemaphore -lpspmesgd_driver -lpsputilsforkernel -lpspsysmem_kernel -lpspkdebug -lpspmodulemgr_kernel -lpspinit -lpsploadexec_kernel -lpspnwman_driver

PSPSDK=$(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
