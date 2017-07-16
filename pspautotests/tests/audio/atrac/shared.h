#include <common.h>
#include "atrac.h"

inline void schedfSingleResetBuffer(AtracSingleResetBufferInfo &info, void *basePtr) {
	int diff = info.writePos - (u8 *)basePtr;
	if (diff < 0x10000 && diff >= 0) {
		schedf("write=p+0x%x, writable=%08x, min=%08x, file=%08x", diff, info.writableBytes, info.minWriteBytes, info.filePos);
	} else {
		schedf("write=%p, writable=%08x, min=%08x, file=%08x", info.writePos, info.writableBytes, info.minWriteBytes, info.filePos);
	}
}

inline void schedfResetBuffer(AtracResetBufferInfo &info, void *basePtr) {
	schedf("   #1: ");
	schedfSingleResetBuffer(info.first, basePtr);
	schedf("\n   #2: ");
	schedfSingleResetBuffer(info.second, basePtr);
	schedf("\n");
}

void LoadAtrac();
void UnloadAtrac();

struct Atrac3File {
	Atrac3File(const char *filename);
	~Atrac3File();

	void Reload(const char *filename);
	void Require();

	bool IsValid() {
		return data_ != NULL;
	}
	void *Data() {
		return data_;
	}
	size_t Size() {
		return size_;
	}

private:
	size_t size_;
	void *data_;
};
