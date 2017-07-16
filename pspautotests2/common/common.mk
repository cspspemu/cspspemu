# Include this to simplify your test include file.
#
# Usage:
#
# TARGETS=mytest
# include ../../path/to/common/common.mk

BUILD_PRX = 1
USE_PSPSDK_LIBC = 1
PSP_FW_VERSION = 500

INCDIR := $(INCDIR) . $(COMMON_DIR)
LIBDIR := $(LIBDIR) . $(COMMON_DIR)

ifndef CFLAGS
CFLAGS = -g -G0 -Wall -O0 -fno-strict-aliasing
endif
ifndef CXXFLAGS
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti 
endif
ifndef ASFLAGS
ASFLAGS = $(CFLAGS)
endif
ifndef LDFLAGS
LDFLAGS = -G0
endif

ifndef LIBS
LIBS = -lpspgu -lpsprtc -lpspctrl -lpspmath -lcommon -lc -lm
endif
ifdef EXTRA_LIBS
LIBS := $(LIBS) $(EXTRA_LIBS)
endif

TARGET = $(firstword $(TARGETS))
OBJS = $(firstword $(TARGETS)).o $(EXTRA_OBJS)

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak

%.elf: %.o $(EXTRA_OBJS) $(EXPORT_OBJ)
	$(LINK.c) $^ $(LIBS) -o $@
	$(FIXUP) $@

%.prx: %.elf
	psp-prxgen $< $@

%.o: %.S
	$(AS) $(ASFLAGS) -c -o $@ $<

all: $(TARGETS:=.prx)
clean: EXTRA_TARGETS:=$(EXTRA_TARGETS) $(TARGETS:=.prx)
