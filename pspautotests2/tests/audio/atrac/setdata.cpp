#include "shared.h"

extern "C" int main(int argc, char *argv[]) {
	Atrac3File at3("sample.at3");
	at3.Require();
	LoadAtrac();
	int atracID = sceAtracGetAtracID(0x1001);
	int atracPlusID = sceAtracGetAtracID(0x1000);

	checkpointNext("IDs:");
	checkpoint("  ATRAC3: %08x", sceAtracSetData(atracID, at3.Data(), at3.Size()));
	checkpoint("  ATRAC3+: %08x", sceAtracSetData(atracPlusID, at3.Data(), at3.Size()));
	checkpoint("  Unallocated (1): %08x", sceAtracSetData(1, at3.Data(), at3.Size()));
	checkpoint("  Unallocated (4): %08x", sceAtracSetData(4, at3.Data(), at3.Size()));
	checkpoint("  -1: %08x", sceAtracSetData(-1, at3.Data(), at3.Size()));

	checkpointNext("Buffer:");
	// Crashes.
	//checkpoint("  NULL: %08x", sceAtracSetData(atracID, NULL, at3.Size()));
	checkpoint("  Zero length: %08x", sceAtracSetData(atracPlusID, at3.Data(), 0));

	memset(at3.Data(), 0, at3.Size());
	checkpoint("  Zeroed data: %08x", sceAtracSetData(atracPlusID, at3.Data(), at3.Size()));

	checkpointNext("sceAtracSetDataAndGetID:");
	int result = sceAtracSetDataAndGetID(at3.Data(), at3.Size());
	if (result >= 0) {
		sceAtracReleaseAtracID(result);
	}
	checkpoint("  Zeroed data: %08x", result);
	at3.Reload("sample.at3");
	checkpoint("  * Reloaded ATRAC3+ test file");
	result = sceAtracSetDataAndGetID(at3.Data(), at3.Size());
	if (result >= 0) {
		sceAtracReleaseAtracID(result);
	}
	checkpoint("  Normal: %08x", result);

	sceAtracReleaseAtracID(atracPlusID);
	sceAtracReleaseAtracID(atracID);
	UnloadAtrac();

	return 0;
}
