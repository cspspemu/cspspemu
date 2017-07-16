#include <common.h>
#include <stdlib.h>
#include <malloc.h>
#include <pspatrac3.h>
#include <psputility.h>

extern int sceAtracGetAtracID(int codecType);
extern int sceAtracReinit(int at3origCount, int at3plusCount);

static int ids[64];

void resetIds() {
	size_t i;
	for (i = 0; i < ARRAY_SIZE(ids); ++i) {
		sceAtracReleaseAtracID(ids[i]);
	}
	memset(ids, 0xFF, sizeof(ids));
}

void checkpointIds() {
	int orig[16];
	int plus[16];
	size_t i;
	size_t c;

	memset(orig, -1, sizeof(orig));
	memset(plus, -1, sizeof(plus));

	checkpoint(NULL);
	schedf("    ATRAC3+:");
	for (i = 0, c = 0; i < ARRAY_SIZE(plus); ++i) {
		plus[i] = sceAtracGetAtracID(0x1000);
		if (plus[i] >= 0) {
			schedf(" %d", plus[i]);
		}
		++c;
	}
	if (c == 0) {
		schedf(" (none)");
	}

	schedf(", ATRAC3:");
	for (i = 0, c = 0; i < ARRAY_SIZE(orig); ++i) {
		orig[i] = sceAtracGetAtracID(0x1001);
		if (orig[i] >= 0) {
			schedf(" %d", orig[i]);
		}
		++c;
	}
	if (c == 0) {
		schedf(" (none)");
	}

	for (i = 0; i < ARRAY_SIZE(plus); ++i) {
		sceAtracReleaseAtracID(plus[i]);
	}

	for (i = 0; i < ARRAY_SIZE(orig); ++i) {
		sceAtracReleaseAtracID(orig[i]);
	}

	schedf("\n");
}

int main(int argc, char *argv[]) {
	sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	sceUtilityLoadModule(PSP_MODULE_AV_ATRAC3PLUS);

	checkpointNext("sceAtracReinit ID usage:");

	checkpoint("  Initial ids:");
	checkpointIds();

	checkpoint("  sceAtracReinit(2, 2): %08x", sceAtracReinit(2, 2));
	checkpointIds();

	checkpoint("  sceAtracReinit(3, 1): %08x", sceAtracReinit(3, 1));
	checkpointIds();

	checkpoint("  sceAtracReinit(999, 0): %08x", sceAtracReinit(999, 0));
	checkpointIds();

	checkpoint("  sceAtracReinit(0, 999): %08x", sceAtracReinit(0, 999));
	checkpointIds();

	checkpoint("  sceAtracReinit(999, 999): %08x", sceAtracReinit(0, 999));
	checkpointIds();

	checkpoint("  sceAtracReinit(0, 0): %08x", sceAtracReinit(0, 0));
	checkpointIds();

	checkpoint("  sceAtracReinit(0, 0): %08x", sceAtracReinit(0, 0));
	checkpointIds();

	checkpoint("  sceAtracReinit(0, -1): %08x", sceAtracReinit(0, -1));
	checkpointIds();

	checkpoint("  sceAtracReinit(0, -1): %08x", sceAtracReinit(0, -1));
	checkpointIds();

	checkpointNext("sceAtracReinit while open:");
	checkpoint("  Reset: %08x", sceAtracReinit(2, 2));
	int orig = sceAtracGetAtracID(0x1001);
	checkpoint("  With allocated, just one: %08x", sceAtracReinit(1, 0));
	checkpoint("  With allocated, free: %08x", sceAtracReinit(0, 0));
	sceAtracReleaseAtracID(orig);

	checkpointNext("ID reuse:");
	checkpoint("  Reset: %08x", sceAtracReinit(6, 0));
	checkpointIds();

	int reuse[6];
	size_t i;
	for (i = 0; i < ARRAY_SIZE(reuse) - 1; ++i) {
		reuse[i] = sceAtracGetAtracID(0x1001);
	}
	checkpoint("  Allocated #0-#4 (#5 still free)");
	checkpoint("  Release #3: %08x", sceAtracReleaseAtracID(reuse[3]));
	checkpoint("  Allocate new: %s", sceAtracGetAtracID(0x1001) == reuse[3] ? "#3" : "different");
	for (i = 0; i < ARRAY_SIZE(reuse) - 1; ++i) {
		sceAtracReleaseAtracID(reuse[i]);
	}

	return 0;
}