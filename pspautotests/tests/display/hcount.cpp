#include <common.h>
#include <pspdisplay.h>

extern "C" {
	int sceDisplayIsVblank();
	int sceDisplayGetCurrentHcount();
	int sceDisplayGetAccumulatedHcount();
	int sceDisplayAdjustAccumulatedHcount(int value);
}

void testAdjustHcount(const char *title, int value) {
	int result = sceDisplayAdjustAccumulatedHcount(value);
	int newValue = sceDisplayGetAccumulatedHcount();

	if (result < 0) {
		checkpoint("%s: %08x", title, result);
	} else {
		checkpoint("%s: %08x, hcount -> %d", title, result, newValue);
	}
}

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("sceDisplayAdjustAccumulatedHcount:");
	testAdjustHcount("  -1", -1);
	testAdjustHcount("  INT_MIN", 0x80000000);
	testAdjustHcount("  INT_MAX", 0x7FFFFFFF);
	testAdjustHcount("  0", 0);

	checkpointNext("sceDisplayGetAccumulatedHcount:");
	testAdjustHcount("  Adjust to INT_MAX", 0x7FFFFFFF);
	int wrappedH = sceDisplayGetAccumulatedHcount();
	if (wrappedH < 5) {
		checkpoint("  Wrapped around to 0: OK");
	} else {
		checkpoint("  Wrapped around to 0: Failed %d", wrappedH);
	}

	// Okay, let's gather some statistics.
	int minH = 1000;
	int maxH = 0;
	int maxStrideH = 0;
	int lastH = 0;
	
	sceDisplayWaitVblankStart();
	sceDisplayAdjustAccumulatedHcount(0x7FFFFFFF);
	for (int i = 0; i < 60; ++i) {
		sceDisplayWaitVblankStart();
		int h = sceDisplayGetCurrentHcount();
		int totalH = sceDisplayGetAccumulatedHcount();

		if (h < minH) {
			minH = h;
		}
		if (totalH - lastH > maxStrideH) {
			maxStrideH = totalH - lastH;
		}
		lastH = totalH;

		while (sceDisplayIsVblank()) {
			h = sceDisplayGetCurrentHcount();
			if (h > maxH) {
				maxH = h;
			}
		}
	}
	int avgH = sceDisplayGetAccumulatedHcount() / 60;

	checkpointNext("sceDisplayGetCurrentHcount:");

	// Generally, 1, 14, 286, 286.
	checkpoint("  (NOTE: not always consistent, use a few runs to get common numbers.)");
	checkpoint("  Lowest: %d", minH);
	checkpoint("  Highest (in vblank): %d", maxH);
	checkpoint("  Highest stride: %d", maxStrideH);
	checkpoint("  Average: %d", avgH);

	return 0;
}