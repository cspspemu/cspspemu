#include <common.h>
#include <pspge.h>
#include <pspthreadman.h>
#include <psputils.h>

#include "../commands/commands.h"

#include "sysmem-imports.h"

struct SceGeStack {
	int v[8];
};
extern "C" int sceGeGetStack(int level, SceGeStack *stack);

static u32 __attribute__((aligned(16))) list[262144];

void testGetStack(const char *title, int level, bool useStack = true) {
	SceGeStack stack;
	memset(&stack, 0xCC, sizeof(stack));
	checkpoint("  %s: %08x", title, sceGeGetStack(level, useStack ? &stack : NULL));
	// I'm not sure what the others are, or if they're important.
	// So far, no games have been seen accessing this, so let's just make sure the values we know are right.
	checkpoint("     * 0: %08x", 0, stack.v[0]);
	checkpoint("     * 1: %08x", 1, stack.v[1]);
	checkpoint("     * 2: %08x", 2, stack.v[2]);
	checkpoint("     * 7: %08x", 7, stack.v[7]);
}

extern "C" void signalFunc(int id, void *arg) {
	int listid = *(int *)arg;
	testGetStack("-1", -1);
	testGetStack("0", 0);
	testGetStack("255", 255);
	testGetStack("NULL stack", 0, false);
}

void runGetStackTests() {
	int listid;

	memset(list, 0, sizeof(list));
	//list[0] = (GE_CMD_BASE << 24) | 0x09FFFF;
	list[0] = (GE_CMD_BASE << 24) | ((((int)list) & 0xFF000000) >> 8);
	list[1] = (GE_CMD_VADDR << 24) | 0xAA1337;
	list[2] = (GE_CMD_IADDR << 24) | 0xBB1337;
	list[3] = (GE_CMD_OFFSETADDR << 24) | 0;
	list[3] = (GE_CMD_OFFSETADDR << 24) | 0xCC1337;
	//list[4] = (GE_CMD_CALL << 24) | ((u32)(&list[200]) & 0x00FFFFFF);
	list[10] = (GE_CMD_SIGNAL << 24) | (PSP_GE_SIGNAL_CALL << 16) | ((u32)(&list[1000]) >> 16);
	list[11] = (GE_CMD_END << 24) | ((u32)(&list[1000]) & 0x0000FFFF);
	list[100] = GE_CMD_FINISH << 24;
	list[101] = GE_CMD_END << 24;
	list[200] = (GE_CMD_BASE << 24) | 0x09FFFF;
	list[201] = (GE_CMD_OFFSETADDR << 24) | 0xCC1337;
	list[202] = GE_CMD_RET << 24;
	list[1000] = (GE_CMD_BASE << 24) | 0x0AEEEE;
	list[1010] = (GE_CMD_SIGNAL << 24) | (PSP_GE_SIGNAL_HANDLER_SUSPEND << 16);
	list[1011] = GE_CMD_END << 24;
	list[1020] = (GE_CMD_SIGNAL << 24) | (PSP_GE_SIGNAL_RET << 16);
	list[1021] = GE_CMD_END << 24;
	list[1100] = GE_CMD_FINISH << 24;
	list[1101] = GE_CMD_END << 24;
	sceKernelDcacheWritebackRange(list, sizeof(list));

	checkpointNext("sceGeGetStack - without list:");
	testGetStack("-1", -1);
	testGetStack("0", 0);
	testGetStack("255", 255);
	testGetStack("NULL stack", 0, false);

	PspGeCallbackData cb;
	cb.signal_func = signalFunc;
	cb.signal_arg = &listid;
	cb.finish_func = NULL;
	int cbID = sceGeSetCallback(&cb);

	checkpointNext("sceGeGetStack - within list:");
	listid = sceGeListEnQueue(list, list, cbID, NULL);
	sceGeListUpdateStallAddr(listid, &list[2000]);
	sceKernelDelayThread(10000);
	sceGeUnsetCallback(cbID);
	checkpoint("  Base address: %08x", sceGeGetCmd(GE_CMD_BASE));
}

struct CmdRange {
	u8 start;
	u8 end;
};

// These are state related cmds.
static const CmdRange contextCmdRanges[] = {
	{0x00, 0x02},
	// Skip: {0x03, 0x0F},
	{0x10, 0x10},
	// Skip: {0x11, 0x11},
	{0x12, 0x28},
	// Skip: {0x29, 0x2B},
	{0x2c, 0x33},
	// Skip: {0x34, 0x35},
	{0x36, 0x38},
	// Skip: {0x39, 0x41},
	{0x42, 0x4D},
	// Skip: {0x4E, 0x4F},
	{0x50, 0x51},
	// Skip: {0x52, 0x52},
	{0x53, 0x58},
	// Skip: {0x59, 0x5A},
	{0x5B, 0xB5},
	// Skip: {0xB6, 0xB7},
	{0xB8, 0xC3},
	// Skip: {0xC4, 0xC4},
	{0xC5, 0xD0},
	// Skip: {0xD1, 0xD1}
	{0xD2, 0xE9},
	// Skip: {0xEA, 0xEA},
	{0xEB, 0xEC},
	// Skip: {0xED, 0xED},
	{0xEE, 0xEE},
	// Skip: {0xEF, 0xEF},
	{0xF0, 0xF6},
	// Skip: {0xF7, 0xF7},
	{0xF8, 0xF9},
	// Skip: {0xFA, 0xFF},
};

void runGetCmdTests() {
	u32 *listPos = list;
	int listid;

	checkpointNext("sceGeGetCmd:");

	for (size_t i = 0; i < ARRAY_SIZE(contextCmdRanges); ++i) {
		for (int n = contextCmdRanges[i].start; n <= contextCmdRanges[i].end; ++n) {
			*listPos++ = (n << 24) | 0x133700 | n;
		}
	}
	*listPos++ = GE_CMD_FINISH << 24;
	*listPos++ = GE_CMD_END << 24;
	*listPos++ = GE_CMD_NOP << 24;

	sceKernelDcacheWritebackRange(list, sizeof(u32) * (listPos - list));
	listid = sceGeListEnQueue(list, listPos, -1, NULL);
	checkpoint("Set a bunch of values: %08x", sceGeListSync(listid, 0));

	bool mismatch = false;
	for (size_t i = 0; i < ARRAY_SIZE(contextCmdRanges); ++i) {
		for (int n = contextCmdRanges[i].start; n <= contextCmdRanges[i].end; ++n) {
			u32 expected = (n << 24) | 0x133700 | n;
			u32 actual = sceGeGetCmd(n);
			if (actual != expected) {
				checkpoint("  Mistmatch: %d %08x != %08x", n, actual, expected);
				mismatch = true;
			}
		}
	}
	if (!mismatch) {
		checkpoint("  All match");
	}

	// Let's reset them...
	listPos = list;
	for (size_t i = 0; i < ARRAY_SIZE(contextCmdRanges); ++i) {
		for (int n = contextCmdRanges[i].start; n <= contextCmdRanges[i].end; ++n) {
			*listPos++ = (n << 24) | 0;
		}
	}
	*listPos++ = GE_CMD_FINISH << 24;
	*listPos++ = GE_CMD_END << 24;
	*listPos++ = GE_CMD_NOP << 24;

	sceKernelDcacheWritebackRange(list, sizeof(u32) * (listPos - list));
	listid = sceGeListEnQueue(list, listPos, -1, NULL);
	checkpoint("Reset back: %08x", sceGeListSync(listid, 0));

	checkpoint("Too low: %08x", sceGeGetCmd(-1));
	checkpoint("Too high: %08x", sceGeGetCmd(256));
}

void testGetMtx(const char *title, int n, int size, bool useDataPtr = true) {
	u32 data[100];
	memset(data, 0xCC, sizeof(data));

	int result = sceGeGetMtx(n, useDataPtr ? data : NULL);
	checkpoint("  %s: %08x", title, result);
	checkpoint(NULL);
	schedf("  * ");
	for (int i = 0; i < size + 1; ++i) {
		schedf(" %08x", data[i]);
	}
	schedf("\n");
}

void runGetMtxTests() {
	u32 *listPos = list;
	int listid;

	checkpointNext("sceGeGetMtx:");

	*listPos++ = (GE_CMD_BONEMATRIXNUMBER << 24) | 0;
	// Intentionally going over a bit, let's see if the data wraps.
	for (int i = 0; i < 100; ++i) {
		*listPos++ = (GE_CMD_BONEMATRIXDATA << 24) | i;
	}
	
	*listPos++ = (GE_CMD_WORLDMATRIXNUMBER << 24) | 0;
	for (int i = 0; i < 16; ++i) {
		*listPos++ = (GE_CMD_WORLDMATRIXDATA << 24) | i;
	}
	
	*listPos++ = (GE_CMD_VIEWMATRIXNUMBER << 24) | 0;
	for (int i = 0; i < 16; ++i) {
		*listPos++ = (GE_CMD_VIEWMATRIXDATA << 24) | i;
	}
	
	*listPos++ = (GE_CMD_PROJMATRIXNUMBER << 24) | 0;
	for (int i = 0; i < 20; ++i) {
		*listPos++ = (GE_CMD_PROJMATRIXDATA << 24) | i;
	}
	
	*listPos++ = (GE_CMD_TGENMATRIXNUMBER << 24) | 0;
	for (int i = 0; i < 16; ++i) {
		*listPos++ = (GE_CMD_TGENMATRIXDATA << 24) | i;
	}

	*listPos++ = GE_CMD_FINISH << 24;
	*listPos++ = GE_CMD_END << 24;
	*listPos++ = GE_CMD_NOP << 24;

	sceKernelDcacheWritebackRange(list, sizeof(u32) * (listPos - list));
	listid = sceGeListEnQueue(list, listPos, -1, NULL);
	checkpoint("Set a bunch of values: %08x", sceGeListSync(listid, 0));

	testGetMtx("BONE0", PSP_GE_MATRIX_BONE0, 12);
	testGetMtx("BONE1", PSP_GE_MATRIX_BONE1, 12);
	testGetMtx("BONE2", PSP_GE_MATRIX_BONE2, 12);
	testGetMtx("BONE3", PSP_GE_MATRIX_BONE3, 12);
	testGetMtx("BONE4", PSP_GE_MATRIX_BONE4, 12);
	testGetMtx("BONE5", PSP_GE_MATRIX_BONE5, 12);
	testGetMtx("BONE6", PSP_GE_MATRIX_BONE6, 12);
	testGetMtx("BONE7", PSP_GE_MATRIX_BONE7, 12);
	testGetMtx("WORLD", PSP_GE_MATRIX_WORLD, 12);
	testGetMtx("VIEW", PSP_GE_MATRIX_VIEW, 12);
	testGetMtx("PROJ", PSP_GE_MATRIX_PROJECTION, 16);
	testGetMtx("TGEN", PSP_GE_MATRIX_TEXGEN, 12);

	testGetMtx("Too low", -1, 12);
	testGetMtx("Too high", 12, 12);
	// Crashes.
	//testGetMtx("Bad ptr", PSP_GE_MATRIX_BONE0, 12, false);
}

extern "C" int main(int argc, char *argv[]) {
	runGetStackTests();
	runGetCmdTests();
	runGetMtxTests();
	return 0;
}