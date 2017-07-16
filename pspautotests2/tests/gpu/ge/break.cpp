#include <common.h>
#include <pspge.h>
#include <pspthreadman.h>
#include <psputils.h>

#include "../commands/commands.h"

// No idea what the second param could be?  Was getting errors until passing NULL.
extern "C" int sceGeBreak(int mode, void *unk1);
extern "C" int sceGeContinue();

static u32 __attribute__((aligned(16))) list[262144];

void logAddresses() {
	PspGeContext ctx;
	memset(&ctx, 0xCC, sizeof(ctx));
	sceGeSaveContext(&ctx);
	checkpoint("  * Addresses: %08x, %08x, %08x", ctx.context[5], ctx.context[6], ctx.context[7]);
}

extern "C" int main(int argc, char *argv[]) {
	int listid;
	int result;

	memset(list, 0, sizeof(list));
	list[0] = GE_CMD_NOP << 24;
	list[1] = (GE_CMD_SIGNAL << 24) | (PSP_GE_SIGNAL_HANDLER_PAUSE << 16);
	list[2] = GE_CMD_END << 24;
	list[3] = GE_CMD_NOP << 24;
	list[4] = GE_CMD_FINISH << 24;
	list[5] = GE_CMD_END << 24;
	list[6] = GE_CMD_NOP << 24;
	list[7] = GE_CMD_FINISH << 24;
	list[8] = GE_CMD_END << 24;

	list[100] = (GE_CMD_BASE << 24) | 0;
	list[101] = (GE_CMD_OFFSETADDR << 24) | 0x011111;
	list[102] = (GE_CMD_BASE << 24) | 0x0A1337;
	list[103] = (GE_CMD_VADDR << 24) | 0xBB1337;
	list[104] = (GE_CMD_IADDR << 24) | 0xCC1337;
	list[105] = (GE_CMD_OFFSETADDR << 24) | 0x222222;
	list[120] = GE_CMD_NOP << 24;
	list[121] = GE_CMD_FINISH << 24;
	list[122] = GE_CMD_END << 24;

	list[200] = GE_CMD_NOP << 24;
	list[201] = GE_CMD_FINISH << 24;
	list[202] = GE_CMD_END << 24;
	sceKernelDcacheWritebackRange(list, sizeof(list));

	checkpointNext("sceGeBreak - empty queue:");
	checkpoint("  Mode 0: %08x", sceGeBreak(0, NULL));
	checkpoint("  Mode 1: %08x", sceGeBreak(1, NULL));
	checkpoint("  Mode -1: %08x", sceGeBreak(-1, NULL));
	checkpoint("  Mode 2: %08x", sceGeBreak(2, NULL));
	checkpoint("  Valid ptr: %08x", sceGeBreak(0, list));
	checkpoint("  Invalid ptr 1: %08x", sceGeBreak(0, (void *)0xDEADBEEF));
	checkpoint("  Invalid ptr 2: %08x", sceGeBreak(0, (void *)-1));
	checkpoint("  Invalid ptr 3: %08x", sceGeBreak(0, (void *)0x7FFFFFF0));
	checkpoint("  Invalid ptr 4: %08x", sceGeBreak(0, (void *)0x7FFFFFEF));

	checkpointNext("sceGeContinue - empty queue:");
	checkpoint("  Normal: %08x", sceGeContinue());

	listid = sceGeListEnQueue(list, list, -1, NULL);
	sceGeListSync(listid, 1);
	checkpointNext("Stalled list:");
	result = sceGeBreak(0, NULL);
	checkpoint("  Break 0: %08x", result == listid ? 0x1337 : result);
	checkpoint("  Continue: %08x", sceGeContinue());
	checkpoint("  Break 1: %08x", sceGeBreak(1, NULL));

	listid = sceGeListEnQueue(list, list + 100, -1, NULL);
	sceGeListSync(listid, 1);
	checkpointNext("Pause signal:");
	result = sceGeBreak(0, NULL);
	checkpoint("  Break 0: %08x, status = %08x", result == listid ? 0x1337 : result, sceGeListSync(listid, 1));
	result = sceGeContinue();
	checkpoint("  Continue: %08x, status = %08x", result, sceGeListSync(listid, 1));
	result = sceGeBreak(1, NULL);
	checkpoint("  Break 1: %08x, status = %08x", result, sceGeListSync(listid, 1));

	listid = sceGeListEnQueue(list + 200, list + 300, -1, NULL);
	sceGeListSync(listid, 1);
	checkpointNext("Completed list:");
	result = sceGeBreak(0, NULL);
	checkpoint("  Break 0: %08x", result == listid ? 0x1337 : result);
	checkpoint("  Continue: %08x", sceGeContinue());
	checkpoint("  Break 1: %08x", sceGeBreak(1, NULL));

	// TODO: There's some odd case where things get reset to zero but it seems like a bug.
	// Not sure how to reproduce it properly.
	listid = sceGeListEnQueue(list + 100, list + 200, -1, NULL);
	checkpointNext("Address handling:");
	logAddresses();
	// And another so that something is pending to break.
	listid = sceGeListEnQueue(list, list, -1, NULL);
	result = sceGeBreak(0, NULL);
	checkpoint("  Break 0: %08x", result == listid ? 0x1337 : result);
	logAddresses();
	checkpoint("  Break 1: %08x", sceGeBreak(1, NULL));
	logAddresses();

	listid = sceGeListEnQueue(list + 100, list + 120, -1, NULL);
	sceKernelDelayThread(10000);
	sceGeListSync(listid, 1);
	sceGeListUpdateStallAddr(listid, list + 121);
	checkpointNext("Address handling:");
	logAddresses();
	result = sceGeBreak(0, NULL);
	checkpoint("  Break 0: %08x", result == listid ? 0x1337 : result);
	logAddresses();
	checkpoint("  Break 1: %08x", sceGeBreak(1, NULL));
	logAddresses();

	return 0;
}