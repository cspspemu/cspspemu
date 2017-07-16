#include "shared.h"

extern "C" int main(int argc, char *argv[]) {
	AtracResetBufferInfo info;
	Atrac3File at3("sample.at3");
	at3.Require();
	LoadAtrac();

	int ignore;
	int atracID;
	u16 data[16384];
	
	atracID = sceAtracSetHalfwayBufferAndGetID(at3.Data(), at3.Size() / 2, at3.Size() / 2);
	memset(&info, 0xCC, sizeof(info));
	checkpointNext("IDs:");
	checkpoint("  Unallocated: %08x", sceAtracGetBufferInfoForResetting(4, 0, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Invalid: %08x", sceAtracGetBufferInfoForResetting(-1, 0, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Valid: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, &info));
	schedfResetBuffer(info, at3.Data());
	
	checkpointNext("Sample values:");
	const static int samples[] = {0, 0x1000, 0x10000, 0x100000, 0x1000000, 0x7FFFFFFF, -1};
	for (size_t i = 0; i < ARRAY_SIZE(samples); ++i) {
		checkpoint("  %08x: %08x", samples[i], sceAtracGetBufferInfoForResetting(atracID, samples[i], &info));
		schedfResetBuffer(info, at3.Data());
	}

	// Crashes.
	//checkpointNext("Buffer:");
	//checkpoint("  NULL: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, NULL));

	sceAtracReleaseAtracID(atracID);
	at3.Reload("sample.at3");
	atracID = sceAtracSetDataAndGetID(at3.Data(), at3.Size());
	checkpointNext("Entire buffer:");
	checkpoint("  At start -> 0: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Decode: %08x", sceAtracDecodeData(atracID, data, &ignore, &ignore, &ignore));
	checkpoint("  After decode -> 0: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, &info));
	schedfResetBuffer(info, at3.Data());

	checkpoint("  At start -> 2048: %08x", sceAtracGetBufferInfoForResetting(atracID, 65536, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Decode: %08x", sceAtracDecodeData(atracID, data, &ignore, &ignore, &ignore));
	checkpoint("  After decode -> 2048: %08x", sceAtracGetBufferInfoForResetting(atracID, 65536, &info));
	schedfResetBuffer(info, at3.Data());

	sceAtracReleaseAtracID(atracID);
	at3.Reload("sample.at3");
	atracID = sceAtracSetHalfwayBufferAndGetID(at3.Data(), at3.Size() / 2, at3.Size() / 2);
	checkpointNext("Half buffer:");
	checkpoint("  At start -> 0: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Decode: %08x", sceAtracDecodeData(atracID, data, &ignore, &ignore, &ignore));
	checkpoint("  After decode -> 0: %08x", sceAtracGetBufferInfoForResetting(atracID, 0, &info));
	schedfResetBuffer(info, at3.Data());

	checkpoint("  At start -> 2048: %08x", sceAtracGetBufferInfoForResetting(atracID, 65536, &info));
	schedfResetBuffer(info, at3.Data());
	checkpoint("  Decode: %08x", sceAtracDecodeData(atracID, data, &ignore, &ignore, &ignore));
	checkpoint("  After decode -> 2048: %08x", sceAtracGetBufferInfoForResetting(atracID, 65536, &info));
	schedfResetBuffer(info, at3.Data());

	sceAtracReleaseAtracID(atracID);

	return 0;
}
