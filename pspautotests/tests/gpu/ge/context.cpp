#include <common.h>
#include <pspge.h>
#include <pspthreadman.h>

#include "../commands/commands.h"

static u32 __attribute__((aligned(16))) list[262144];

extern "C" int main(int argc, char *argv[]) {
	int listid;
	PspGeContext context;
	memset(&context, 0xCC, sizeof(context));

	memset(&list, 0, sizeof(list));
	list[0] = GE_CMD_NOP << 24;
	list[1] = GE_CMD_FINISH << 24;
	list[2] = GE_CMD_END << 24;

	checkpointNext("sceGeSaveContext:");
	checkpoint("  Normal: %08x", sceGeSaveContext(&context));
	// Crash.
	//checkpoint("  Normal: NULL", sceGeSaveContext(NULL));

	checkpoint("  Size check: %s", context.context[386] == 0xCCCCCCCC ? "OK" : "Too big");

	checkpointNext("With enqueued list");
	listid = sceGeListEnQueue(list, list, -1, NULL);
	checkpoint("  Enqueued: %08x", listid >= 0 ? 0x1337 : listid);
	checkpoint("  Status: list=%08x, draw=%08x", sceGeListSync(listid, 1), sceGeDrawSync(1));
	checkpoint("  Save context: %08x", sceGeSaveContext(&context));
	checkpoint("  Restore context: %08x", sceGeRestoreContext(&context));
	checkpoint("  Status: list=%08x, draw=%08x", sceGeListSync(listid, 1), sceGeDrawSync(1));
	checkpoint("  Unstall: %08x", sceGeListUpdateStallAddr(listid, NULL));
	checkpoint("  Save context: %08x", sceGeSaveContext(&context));
	checkpoint("  Restore context: %08x", sceGeRestoreContext(&context));

	PspGeListArgs listargs;
	listargs.size = sizeof(listargs);
	listargs.context = &context;
	memset(&context, 0xCC, sizeof(context));

	const int GE_CTX_LIGHTINGENABLE_OFFSET = 26;
	list[0] = (GE_CMD_LIGHTINGENABLE << 24) | 1;

	checkpointNext("From sceGeListEnQueue:");
	listid = sceGeListEnQueue(list, list + 1, -1, &listargs);
	checkpoint("  Enqueued: %08x", listid >= 0 ? 0x1337 : listid);
	checkpoint("  Delayed for catch up: %08x", sceKernelDelayThread(20000));
	checkpoint("  Current value: %08x", sceGeGetCmd(GE_CMD_LIGHTINGENABLE));
	checkpoint("  Context value: %08x", context.context[GE_CTX_LIGHTINGENABLE_OFFSET]);
	checkpoint("  Unstall: %08x", sceGeListUpdateStallAddr(listid, NULL));
	checkpoint("  Delayed for catch up: %08x", sceKernelDelayThread(20000));
	checkpoint("  Current value: %08x", sceGeGetCmd(GE_CMD_LIGHTINGENABLE));
	checkpoint("  Context value: %08x", context.context[GE_CTX_LIGHTINGENABLE_OFFSET]);

	return 0;
}