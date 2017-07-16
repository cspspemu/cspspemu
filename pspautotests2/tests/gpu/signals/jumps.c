#include <common.h>

#include <stdlib.h>
#include <stdio.h>
#include <pspgu.h>
#include <pspgum.h>

#include "sysmem-imports.h"

extern int sceGeContinue();
extern int sceGeBreak(int breakType);

typedef struct
{
    unsigned int stack[8];
} PspGeStack;

typedef struct
{
    u32 size;
    PspGeContext *ctx;
    u32 numStacks;
    PspGeStack *stacks;
} PspGeListArgs2;

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
	0x0E017777, // 0x08 SIGNAL + WAIT
	0x0C000000, // 0x09 END
	0x0F000000, // 0x0A FINISH
	0x0C000000, // 0x0B END
	0x0E018888, // 0x0C SIGNAL + WAIT
	0x0C000000, // 0x0D END
	0x0F000000, // 0x0E FINISH
	0x0C000000, // 0x0F END

	0x00000000, // 0x10 NOP
	0x0E019999, // 0x11 SIGNAL + WAIT
	0x0C000000, // 0x12 END
	0x00000000, // 0x13 NOP
	0x00000000, // 0x14 NOP
	0x00000000, // 0x15 NOP
	0x00000000, // 0x16 NOP
	0x00000000, // 0x17 NOP
	0x00000000, // 0x18 NOP
	0x00000000, // 0x19 NOP
	0x00000000, // 0x1A NOP
	0x00000000, // 0x1B NOP
	0x00000000, // 0x1C NOP
	0x00000000, // 0x1D NOP
	0x0F000000, // 0x1E FINISH
	0x0C000000, // 0x1F END

	0x00000000, // 0x20 NOP
	0x0E01AAAA, // 0x21 SIGNAL + WAIT
	0x0C000000, // 0x22 END
	0x00000000, // 0x23 NOP
	0x0E120000, // 0x24 SIGNAL RET
	0x0C000000, // 0x25 END
	0x00000000, // 0x26 NOP
	0x00000000, // 0x27 NOP
	0x00000000, // 0x28 NOP
	0x00000000, // 0x29 NOP
	0x00000000, // 0x2A NOP
	0x00000000, // 0x2B NOP
	0x00000000, // 0x2C NOP
	0x00000000, // 0x2D NOP
	0x0F000000, // 0x2E FINISH
	0x0C000000, // 0x2F END
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
	PspGeListArgs2 args;
	PspGeContext saved;
	PspGeStack stacks[32];

	sceGeSaveContext(&saved);

	args.size = sizeof(args);
	args.ctx = &saved;
	args.numStacks = 1;
	args.stacks = &stacks[0];

	sceKernelDcacheWritebackAll();

	bpos += sprintf(bpos, "  LIST #\tDRAWSTATE\tINFO\n");

	dlist1id = sceGeListEnQueue(dlist1, 0, cbid1, (PspGeListArgs *) &args);
	listInfoNosync(1, "Enqueued without stall...\n");
	listsync = sceGeListSync(dlist1id, 1);
	listInfo(1, "Sync %x %s\n", listsync, status_str(listsync));

	result = sceGeContinue();
	listsync = sceGeListSync(dlist1id, 1);
	listInfo(1, "Sync %x %s after continue (%08x)\n", listsync, status_str(listsync), result);

	dlist2id = sceGeListEnQueue(dlist1, method & TEST_STALL_LATE ? dlist1 + 0x0D : dlist1 + 0x05, cbid2, (PspGeListArgs *) &args);
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

	GE_SIG_JUMP = 0x10,
	GE_SIG_CALL = 0x11,
	GE_SIG_JUMP_REL = 0x13,
	GE_SIG_CALL_REL = 0x14,
	GE_SIG_JUMP_ORIGIN = 0x15,
	GE_SIG_CALL_ORIGIN = 0x16,

	GE_SIG_BP1 = 0xF0,
	GE_SIG_BP2 = 0xFF,
};

#define MAKE_GE_END(type, value) (0x0C000000 | ((type & 0xFF) << 16) | ((value) & 0xFFFF))
#define MAKE_GE_SIGNAL(type, value) (0x0E000000 | ((type & 0xFF) << 16) | ((value) & 0xFFFF))
#define MAKE_GE_FINISH(type, value) (0x0F000000 | ((type & 0xFF) << 16) | ((value) & 0xFFFF))
#define MAKE_GE_BASE(address) (0x10000000 | (((address) & 0xFF000000) >> 8))
#define MAKE_GE_ORIGIN(value) (0x14000000 | ((value) & 0xFFFFFF))

inline void dlist1SignalOffset(int pos, int type, int endtype, unsigned int address) {
	dlist1[pos + 0x00] = MAKE_GE_SIGNAL(type, address >> 16);
	dlist1[pos + 0x01] = MAKE_GE_END(endtype, address);
}

inline void dlist1SignalRelative(int pos, int type, int endtype, unsigned int address) {
	dlist1SignalOffset(pos, type, endtype, address * sizeof(unsigned int));
}

inline void dlist1SignalAddress(int pos, int type, int endtype, unsigned int *address) {
	dlist1SignalOffset(pos, type, endtype, (unsigned int) address);
}

void testSignalCallsAndJumps() {
	printf("\nJump absolute (0x%02x):\n", GE_SIG_JUMP);
	dlist1SignalAddress(0x01, GE_SIG_JUMP, 0, dlist1 + 0x10);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	printf("\nJump relative (0x%02x):\n", GE_SIG_JUMP_REL);
	dlist1SignalRelative(0x01, GE_SIG_JUMP_REL, 0, 0x10 - 0x01);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	printf("\nJump relative to origin (0x%02x):\n", GE_SIG_JUMP_ORIGIN);
	// Base seems not to matter, just proving that here.
	dlist1[0x01] = MAKE_GE_BASE(0);
	dlist1[0x02] = MAKE_GE_ORIGIN(0);
	dlist1SignalRelative(0x03, GE_SIG_JUMP_ORIGIN, 0, 0x10 - 0x02);
	testGeCallbacks(TEST_USE_DRAWSYNC);
	dlist1[0x03] = 0;
	dlist1[0x04] = 0;

	printf("\nCall absolute (0x%02x):\n", GE_SIG_CALL);
	dlist1SignalAddress(0x01, GE_SIG_CALL, 0, dlist1 + 0x20);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	printf("\nCall relaive (0x%02x):\n", GE_SIG_CALL_REL);
	dlist1SignalRelative(0x01, GE_SIG_CALL_REL, 0, 0x20 - 0x01);
	testGeCallbacks(TEST_USE_DRAWSYNC);

	printf("\nCall relative to origin (0x%02x):\n", GE_SIG_CALL_ORIGIN);
	// Base seems not to matter, just proving that here.
	dlist1[0x01] = MAKE_GE_BASE(0);
	dlist1[0x02] = MAKE_GE_ORIGIN(0);
	dlist1SignalRelative(0x03, GE_SIG_CALL_ORIGIN, 0, 0x20 - 0x02);
	testGeCallbacks(TEST_USE_DRAWSYNC);
	dlist1[0x03] = 0;
	dlist1[0x04] = 0;
}

int main(int argc, char *argv[]) {
	printf("Using default SDK version:\n");
	init();
	testSignalCallsAndJumps();

	printf("\n\nUsing 6.60 SDK version:\n");
	sceKernelSetCompiledSdkVersion(0x6060010);
	init();
	testSignalCallsAndJumps();

	return 0;
}
