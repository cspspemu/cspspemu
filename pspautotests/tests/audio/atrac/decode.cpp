#include "shared.h"

void testDecodeData(const char *title, int atracID, u16 *samples, bool useCount = true, bool useEnded = true, bool useRemaining = true) {
	int count = 0x1337;
	int ended = 0x1337;
	int remaining = 0x1337;
	int result = sceAtracDecodeData(atracID, samples, useCount ? &count : NULL, useEnded ? &ended : NULL, useRemaining ? &remaining : NULL);
	checkpoint("%s: %08x (count=%x, ended=%x, remaining=%x)", title, result, count, ended, remaining);
}

extern "C" int main(int argc, char *argv[]) {
	Atrac3File at3("sample.at3");
	at3.Require();
	LoadAtrac();
	int atracID = sceAtracSetDataAndGetID(at3.Data(), at3.Size());
	
	u16 *samples = new u16[65536];

	checkpointNext("IDs:");
	testDecodeData("  Normal", atracID, samples);
	testDecodeData("  Unallocated (1)", 1, samples);
	testDecodeData("  Unallocated (4)", 4, samples);
	testDecodeData("  -1", -1, samples);

	checkpointNext("Missing parameters:");
	testDecodeData("  NULL output", atracID, NULL);
	// Crashes.
	//testDecodeData("  No count", atracID, samples, false);
	//testDecodeData("  No count", atracID, samples, true, false);
	//testDecodeData("  No count", atracID, samples, true, true, false);

	
	sceAtracResetPlayPosition(atracID, 0, 0, 0);
	testDecodeData("  Normal", atracID, samples);

	checkpointNext("Drain:");
	int result;
	int i;
	for (i = 0; i < 1000; ++i) {
		int ended;
		int remaining;
		int count;
		result = sceAtracDecodeData(atracID, samples, &count, &ended, &remaining);
		if (result != 0) {
			break;
		}
	}
	checkpoint("  Drained in %d calls: %08x", i, result);

	delete [] samples;

	sceAtracReleaseAtracID(atracID);
	UnloadAtrac();

	return 0;
}
