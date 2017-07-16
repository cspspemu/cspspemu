#include <common.h>
#include <pspge.h>
#include <pspthreadman.h>
#include <psputils.h>

#include "../commands/commands.h"

extern "C" {
#include "sysmem-imports.h"
}

// No idea what the second param could be?  Was getting errors until passing NULL.
extern "C" int sceGeBreak(int mode, void *unk1);
extern "C" int sceGeContinue();

static u32 __attribute__((aligned(16))) list[262144];

struct SimpleIDMap {
	static const int SIZE = 128;
	u32 keys_[SIZE + 1];
	u32 values_[SIZE + 1];
	int next_;

	SimpleIDMap() : next_(0) {
	}

	int GetIndex(u32 key) {
		for (int i = 0; i < next_; ++i) {
			if (key == keys_[i]) {
				return i;
			}
		}
		return -1;
	}

	int AddKey(u32 key) {
		if (next_ >= SIZE) {
			return -1;
		}
		keys_[next_] = key;
		values_[next_] = 0;
		return next_++;
	}

	u32 GetKey(int i) const {
		return keys_[i];
	}

	u32 GetValue(int i) const {
		return values_[i];
	}

	int Size() const {
		return next_;
	}

	bool Empty() const {
		return Size() > 0;
	}

	void Clear() {
		next_ = 0;
	}

	u32 &operator [] (u32 key) {
		int i = GetIndex(key);
		if (i == -1) {
			i = AddKey(key);
			if (i == -1) {
				// Prevent crashing the ugly way.
				i = SIZE;
			}
		}

		return values_[i];
	}
};

void testListIDs() {
	SimpleIDMap reuse;
	for (int i = 0; i < 1000; ++i) {
		int id = sceGeListEnQueue(list, list, -1, NULL);
		if (id >= 0) {
			++reuse[id];
		}
	}

	checkpoint("  Out of IDs / dup: %08x", sceGeListEnQueue(list, list, -1, NULL));

	checkpoint(NULL);
	schedf("  Reuse when full:");
	for (int i = 0; i < reuse.Size(); ++i) {
		schedf(" %d", reuse.GetValue(i));
		sceGeListDeQueue(reuse.GetKey(i));
	}
	schedf("\n");

	sceGeBreak(1, NULL);
	reuse.Clear();

	for (int i = 0; i < 1000; ++i) {
		int id = sceGeListEnQueue(list + i, list + i, -1, NULL);

		if (id >= 0) {
			sceGeListDeQueue(id);
			++reuse[id];
		}
	}

	checkpoint(NULL);
	schedf("  Reuse when dequeuing:");
	for (int i = 0; i < reuse.Size(); ++i) {
		schedf(" %d", reuse.GetValue(i));
	}
	schedf("\n");

	sceGeBreak(1, NULL);
}

int makeCompletedList() {
	list[0] = GE_CMD_NOP << 24;
	list[1] = GE_CMD_FINISH << 24;
	list[2] = GE_CMD_END << 24;
	sceKernelDcacheWritebackRange(list, sizeof(list));

	sceGeBreak(1, NULL);
	int listID = sceGeListEnQueueHead(list, list + 10, -1, NULL);
	sceGeContinue();
	return listID;
}

extern "C" int main(int argc, char *argv[]) {
	memset(list, 0, sizeof(list));
	sceKernelDcacheWritebackRange(list, sizeof(list));

	checkpointNext("List IDs usage:");
	sceKernelSetCompiledSdkVersion(0);
	testListIDs();
	sceKernelSetCompiledSdkVersion(0x02000000);
	testListIDs();
	sceKernelSetCompiledSdkVersion(0);

	checkpointNext("Head with a stalled list:");
	int listID1 = sceGeListEnQueue(list, list, -1, NULL);
	checkpoint("  Enqueue 1: %08x", listID1 >= 0 ? 0x1337 : listID1);
	int listID2 = sceGeListEnQueueHead(list + 1, list + 1, -1, NULL);
	checkpoint("  Enqueue 2: %08x", listID2 >= 0 ? 0x1337 : listID2);

	checkpointNext("sceGeListDeQueue:");
	checkpoint("  Not enqueued: %08x", sceGeListDeQueue(listID1));
	checkpoint("  Bad ID: %08x", sceGeListDeQueue(0));
	listID1 = sceGeListEnQueue(list, list, -1, NULL);
	checkpoint("  Enqueued: %08x", sceGeListDeQueue(listID1));
	checkpoint("  Completed: %08x", sceGeListDeQueue(makeCompletedList()));
	sceGeBreak(1, NULL);

	PspGeListArgs args;
	PspGeContext ctx;
	args.size = sizeof(args);
	args.context = &ctx;

	listID1 = sceGeListEnQueue(list, list + 1, -1, &args);
	sceGeBreak(1, NULL);
	sceGeContinue();
	checkpoint("  With context: %08x", sceGeListDeQueue(listID1));
	sceGeBreak(1, NULL);

	checkpointNext("sceGeListUpdateStallAddr:");
	listID1 = sceGeListEnQueue(list, list + 1, -1, NULL);
	checkpoint("  Not Enqueued: %08x", sceGeListUpdateStallAddr(listID2, list + 1));
	checkpoint("  Bad ID: %08x", sceGeListUpdateStallAddr(0, list + 1));
	checkpoint("  Update: %08x", sceGeListUpdateStallAddr(listID1, list + 1));
	checkpoint("  Completed: %08x", sceGeListUpdateStallAddr(makeCompletedList(), list + 10));
	sceGeBreak(1, NULL);

	return 0;
}