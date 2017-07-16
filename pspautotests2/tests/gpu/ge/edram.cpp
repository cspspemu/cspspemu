#include <common.h>
#include <pspge.h>

extern "C" int sceGeEdramSetAddrTranslation(int value);

extern "C" int main(int argc, char *argv[]) {
	checkpoint("sceGeEdramGetAddr: %08x", sceGeEdramGetAddr());
	checkpoint("sceGeEdramGetSize: %08x", sceGeEdramGetSize());

	checkpointNext("sceGeEdramSetAddrTranslation:");
	checkpoint("  Zero: %08x", sceGeEdramSetAddrTranslation(0));
	checkpoint("  Zero again: %08x", sceGeEdramSetAddrTranslation(0));

	int values[] = {-1, 1, 0x10, 0x20, 0x40, 0x80, 0xC0, 0x1FF, 0x100, 0x200, 0x400, 0x800, 0xC00, 0x1000, 0x1800, 0x1fff, 0x2000};
	for (size_t i = 0; i < ARRAY_SIZE(values); ++i) {
		checkpoint("  %x: %08x", values[i], sceGeEdramSetAddrTranslation(values[i]));
	}

	return 0;
}