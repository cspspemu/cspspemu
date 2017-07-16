#include <common.h>

#include <stdlib.h>
#include <stdio.h>
#include <pspgu.h>
#include <pspgum.h>

#include "sysmem-imports.h"

extern int sceGeContinue();
extern int sceGeBreak(int breakType);

static unsigned int __attribute__((aligned(16))) list[262144];

char buffer[65535];
char *bpos = &buffer[0];

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4) /* change this if you change to another screenmode */
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH * SCR_HEIGHT * 2) /* zbuffer seems to be 16-bit? */

unsigned int __attribute__((aligned(16))) dlist1[] = {
	0x00000000, // 0x00 NOP
	0x0E010000, // 0x01 SIGNAL + WAIT
	0x0C000000, // 0x02 END
	0x00000000, // 0x03 NOP
	0x00000000, // 0x04 NOP
	0x00000000, // 0x05 NOP
	0x00000000, // 0x06 NOP
	0x00000000, // 0x07 NOP
	0x00000000, // 0x08 NOP
	0x00000000, // 0x09 NOP
	0x0F000000, // 0x0A FINISH
	0x0C000000, // 0x0B END
	0x00000000, // 0x0C NOP
	0x00000000, // 0x0D NOP
	0x0F000000, // 0x0E FINISH
	0x0C000000, // 0x0F END
};

int dlist1id, dlist2id;
int cbid1, cbid2;

char* status_str(int status) {
	if(status < 0 || status > 4)
		return "INVALID";

	char* str[] = {
		"DONE",
		"QUEUED",
		"DRAWING",
		"STALL",
		"PAUSED",
	};
	return str[status];
}

inline void breakInfo(const char *format, ...) {
	bpos += sprintf(bpos, "  BREAK \t");

	int result = sceGeBreak(1);
	bpos += sprintf(bpos, "  %-8s\t%08x", "", result);

	va_list args;
	va_start(args, format);
	bpos += vsprintf(bpos, format, args);
	va_end(args);
}

inline void syncInfo(const char *format, ...) {
	bpos += sprintf(bpos, "  WAIT  \t");

	int drawsync = sceGeDrawSync(0);
	bpos += sprintf(bpos, "%x %-8s\t", drawsync, status_str(drawsync));

	va_list args;
	va_start(args, format);
	bpos += vsprintf(bpos, format, args);
	va_end(args);
}

inline void listInfo(int n, const char *format, ...) {
	if (n != 0) {
		bpos += sprintf(bpos, "  List %d\t", n);
	} else {
		bpos += sprintf(bpos, "        \t");
	}

	int drawsync = sceGeDrawSync(1);
	bpos += sprintf(bpos, "%x %-8s\t", drawsync, status_str(drawsync));

	va_list args;
	va_start(args, format);
	bpos += vsprintf(bpos, format, args);
	va_end(args);
}

inline void listInfoNosync(int n, const char *format, ...) {
	if (n != 0) {
		bpos += sprintf(bpos, "  List %d\t", n);
	} else {
		bpos += sprintf(bpos, "        \t");
	}

	bpos += sprintf(bpos, "  %-8s\t", "");

	va_list args;
	va_start(args, format);
	bpos += vsprintf(bpos, format, args);
	va_end(args);
}

int ge_signal(int value, void* arg) {
	unsigned int *addr;
    asm("sw $a2, %0" : "=m"(addr));

	sceGeListUpdateStallAddr(dlist2id, 0);
	unsigned int pos = addr == NULL ? 0xFF : (unsigned int) (addr - dlist1);
	listInfo((int) arg + 1, "Signal(%x, list+%02X)\n", (unsigned int) value, pos);
	return 0;
}

int ge_finish(int value, void* arg) {
	unsigned int *addr;
    asm("sw $a2, %0" : "=m"(addr));

	unsigned int pos = addr == NULL ? 0xFF : (unsigned int) (addr - dlist1);
	listInfo((int) arg + 1, "Finish(%x, list+%02X)\n", (unsigned int) value, pos);
	return 0;
}

void init() {
	sceKernelDcacheWritebackAll();

	sceGuInit();
	sceGuStart(GU_DIRECT, list);

	sceGuDrawBuffer (GU_PSM_8888, (void*)FRAME_SIZE, BUF_WIDTH);
	sceGuDepthBuffer((void *)(FRAME_SIZE * 2), BUF_WIDTH);
	sceGuOffset     (2048 - (SCR_WIDTH / 2),2048 - (SCR_HEIGHT / 2));
	sceGuViewport   (2048, 2048, SCR_WIDTH, SCR_HEIGHT);
	sceGuDepthRange (0xc350, 0x2710);
	sceGuScissor    (0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuFinish     ();
	sceGuSync       (GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

	PspGeCallbackData cbdata;
	cbdata.signal_func = (PspGeCallback) ge_signal;
	cbdata.signal_arg  = NULL;
	cbdata.finish_func = (PspGeCallback) ge_finish;
	cbdata.finish_arg  = NULL;
	cbid1 = sceGeSetCallback(&cbdata);

	PspGeCallbackData cbdata2;
	cbdata2.signal_func = (PspGeCallback) ge_signal;
	cbdata2.signal_arg  = (void*) 1;
	cbdata2.finish_func = (PspGeCallback) ge_finish;
	cbdata2.finish_arg  = (void*) 1;
	cbid2 = sceGeSetCallback(&cbdata2);
}

enum {
	TEST_USE_DRAWSYNC = 0x00,
	TEST_USE_BREAK = 0x01,
	TEST_STALL_LATE = 0x02,
};

void testGeCallbacks(int method) {
	int listsync, result;
	sceKernelDcacheWritebackAll();

	bpos += sprintf(bpos, "  LIST #\tDRAWSTATE\tINFO\n");

	dlist1id = sceGeListEnQueue(dlist1, 0, cbid1, 0);
	listInfoNosync(1, "Enqueued without stall...\n");
	listsync = sceGeListSync(dlist1id, 1);
	listInfo(1, "Sync %x %s\n", listsync, status_str(listsync));

	result = sceGeContinue();
	listsync = sceGeListSync(dlist1id, 1);
	listInfo(1, "Sync %x %s after continue (%08x)\n", listsync, status_str(listsync), result);

	dlist2id = sceGeListEnQueue(dlist1, method & TEST_STALL_LATE ? dlist1 + 0x0D : dlist1 + 0x05, cbid2, 0);
	listInfoNosync(2, "Enqueued with %s...\n", method & TEST_STALL_LATE ? "late stall" : "stall");
	listsync = sceGeListSync(dlist2id, 1);
	listInfo(2, "Sync %x %s\n", listsync, status_str(listsync));

	result = sceGeContinue();
	listsync = sceGeListSync(dlist2id, 1);
	listInfo(2, "Sync %x %s after continue (%08x)\n", listsync, status_str(listsync), result);

	result = sceGeListUpdateStallAddr(dlist2id, 0);
	listsync = sceGeListSync(dlist2id, 1);
	listInfo(2, "Sync %x %s after unstall (%08x)\n", listsync, status_str(listsync), result);

	printf("%s", buffer);
	bpos = buffer;
	bpos[0] = '\0';

	if (method & TEST_USE_BREAK) {
		breakInfo("\n");
	} else {
		syncInfo("\n");
	}

	printf("%s", buffer);
	bpos = buffer;
	bpos[0] = '\0';
}

enum
{
	GE_SIG_SUSPEND = 0x01,
	GE_SIG_CONTINUE = 0x02,
	GE_SIG_PAUSE = 0x03,
	GE_SIG_SYNC = 0x08,

	GE_SIG_BP1 = 0xF0,
	GE_SIG_BP2 = 0xFF,
};

#define MAKE_GE_END(type, value) (0x0C000000 | ((type & 0xFF) << 16) | (value & 0xFFFF))
#define MAKE_GE_SIGNAL(type, value) (0x0E000000 | ((type & 0xFF) << 16) | (value & 0xFFFF))
#define MAKE_GE_FINISH(type, value) (0x0F000000 | ((type & 0xFF) << 16) | (value & 0xFFFF))

void testSignalTypes() {
	// All invalid callbacks should work this way? (0x04, 0xFE, etc.)
	printf("\nUnknown (0x%02x):\n", 0);
	dlist1[1] = MAKE_GE_SIGNAL(0, 0x1234);
	dlist1[2] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_BREAK | TEST_STALL_LATE);

	// SUSPEND list while in handler
	printf("\nSignal handler + wait (0x%02x):\n", GE_SIG_SUSPEND);
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_SUSPEND, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	// CONTINUE list keeps getting processed
	printf("\nSignal handler only (0x%02x):\n", GE_SIG_CONTINUE);
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_CONTINUE, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	// PAUSE list and call handler at FINISH CMD, but can resume
	printf("\nSignal handler + pause (0x03):\n");
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_PAUSE, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC | TEST_STALL_LATE);

	// SYNC doesn't call handler but syncs and skips
	printf("\nSync + continue (0x%02x):\n", GE_SIG_SYNC);
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_SYNC, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC | TEST_STALL_LATE);

	// Not included in this test - 0x10 - 0x16 (call/jump/ret/etc.)
	// Not included in this test - 0x20 - 0x38 (tex/clut/etc.)

	// BREAKPOINT may not do anything?
	printf("\nBreakpoint #1 (0x%02x):\n", GE_SIG_BP1);
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_BP1, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	printf("\nUnknown (0x%02x):\n", 0xEE);
	dlist1[1] = MAKE_GE_SIGNAL(0xEE, 0x1234);
	dlist1[2] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_BREAK | TEST_STALL_LATE);

	printf("\nBreakpoint #2 (0x%02x):\n", GE_SIG_BP2);
	dlist1[0x01] = MAKE_GE_SIGNAL(GE_SIG_BP2, 0x1234);
	dlist1[0x02] = MAKE_GE_END(0, 0);
	testGeCallbacks(TEST_USE_DRAWSYNC);
}

int main(int argc, char *argv[]) {
	printf("Using default SDK version:\n");
	init();
	testSignalTypes();

	printf("\n\nUsing 6.60 SDK version:\n");
	sceKernelSetCompiledSdkVersion(0x6060010);
	init();
	testSignalTypes();

	return 0;
}
