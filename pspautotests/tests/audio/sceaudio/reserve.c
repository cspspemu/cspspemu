#include "shared.h"

int main(int argc, char *argv[]) {
	int i;

	sceUtilityLoadModule(PSP_MODULE_AV_VAUDIO);

	checkpointNext("sceAudioChReserve:");
	checkpoint("Normal: %08x", sceAudioChReserve(7, 1024, 0));
	checkpoint("Same channel twice: %08x", sceAudioChReserve(7, 1024, 0));
	sceAudioChRelease(7);

	for (i = 0; i < 8; ++i)
		sceAudioChReserve(i, 1024, 0);
	checkpoint("No channels left: %08x", sceAudioChReserve(-1, 1024, 0));
	for (i = 0; i < 8; ++i)
		sceAudioChRelease(i);

	checkpointNext("Channel values:");
	const static int channels[] = {-9, -7, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 256, 0x10001, 0x80000000, 0x7FFFFFFF};
	for (i = 0; i < ARRAY_SIZE(channels); ++i) {
		int ch = sceAudioChReserve(channels[i], 1024, 0);
		checkpoint("  %d: %08x", channels[i], ch);
		sceAudioChRelease(ch);
	}

	checkpointNext("Sample counts:");
	const static int sampleCounts[] = {-64, -2, -1, 0, 1, 2, 3, 16, 17, 18, 32, 64, 96, 128, 256, 1024, 2048, 2049, 4096, 4110, 4111, 4112, 65472, 65504, 65536};
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		int ch = sceAudioChReserve(0, sampleCounts[i], 0);
		int ch2 = sceAudioChReserve(1, sampleCounts[i], 0x10);
		checkpoint("  %d: %08x", sampleCounts[i], ch);
		if (ch2 != 1 && ch2 != ch) {
			checkpoint("  %d (format 0x10): %08x", sampleCounts[i], ch2);
		}
		sceAudioChRelease(0);
		sceAudioChRelease(1);
	}

	checkpointNext("Formats:");
	const static int formats[] = {-2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0x10, 0x11, 0x12};
	for (i = 0; i < ARRAY_SIZE(formats); ++i) {
		int ch = sceAudioChReserve(0, 1024, formats[i]);
		checkpoint("  %d: %08x", formats[i], ch);
		sceAudioChRelease(0);
	}

	checkpointNext("sceAudioOutput2Reserve:");

	checkpoint("Normal: %08x", sceAudioOutput2Reserve(1024));
	checkpoint("Twice: %08x", sceAudioOutput2Reserve(1024));
	sceAudioOutput2Release();

	for (i = 0; i < 8; ++i)
		sceAudioChReserve(i, 1024, 0);
	checkpoint("No channels left: %08x", sceAudioOutput2Reserve(1024));
	sceAudioOutput2Release();
	for (i = 0; i < 8; ++i)
		sceAudioChRelease(i);

	sceAudioSRCChReserve(1024, 48000, 2);
	checkpoint("SRC channel taken: %08x", sceAudioOutput2Reserve(1024));
	sceAudioSRCChRelease();

	checkpointNext("Sample counts:");
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		checkpoint("  %d: %08x", sampleCounts[i], sceAudioOutput2Reserve(sampleCounts[i]));
		sceAudioOutput2Release();
	}

	checkpointNext("sceAudioSRCChReserve:");

	checkpoint("Normal: %08x", sceAudioSRCChReserve(1024, 48000, 2));
	checkpoint("Twice: %08x", sceAudioSRCChReserve(1024, 48000, 2));
	sceAudioSRCChRelease();

	for (i = 0; i < 8; ++i)
		sceAudioChReserve(i, 1024, 0);
	checkpoint("No channels left: %08x", sceAudioSRCChReserve(1024, 48000, 2));
	sceAudioSRCChRelease();
	for (i = 0; i < 8; ++i)
		sceAudioChRelease(i);

	sceAudioOutput2Reserve(1024);
	checkpoint("Output2 channel taken: %08x", sceAudioSRCChReserve(1024, 48000, 2));
	sceAudioOutput2Release();

	sceAudioOutput2Reserve(1024);
	sceAudioSRCChRelease();
	checkpoint("Output2 channel taken (wrong release): %08x", sceAudioSRCChReserve(1024, 48000, 2));
	sceAudioOutput2Release();

	checkpointNext("Sample counts:");
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		checkpoint("  %d: %08x", sampleCounts[i], sceAudioSRCChReserve(sampleCounts[i], 48000, 2));
		sceAudioSRCChRelease();
	}
	
	checkpointNext("Frequencies:");
	const static int frequencies[] = {-1, -48000, 0, 8000, 11025, 12000, 16000, 22050, 24000, 32000, 48000, 64000};
	for (i = 0; i < ARRAY_SIZE(frequencies); ++i) {
		checkpoint("  %d: %08x", frequencies[i], sceAudioSRCChReserve(1024, frequencies[i], 2));
		sceAudioSRCChRelease();
	}
	
	checkpointNext("Channel counts:");
	const static int channelCounts[] = {-2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8};
	for (i = 0; i < ARRAY_SIZE(channelCounts); ++i) {
		checkpoint("  %d: %08x", channelCounts[i], sceAudioSRCChReserve(1024, 48000, channelCounts[i]));
		sceAudioSRCChRelease();
	}

	checkpointNext("sceVaudioChReserve:");

	checkpoint("Normal: %08x", sceVaudioChReserve(256, 48000, 2));
	checkpoint("Twice: %08x", sceVaudioChReserve(256, 48000, 2));
	sceVaudioChRelease();

	for (i = 0; i < 8; ++i)
		sceAudioChReserve(i, 1024, 0);
	checkpoint("No channels left: %08x", sceVaudioChReserve(256, 48000, 2));
	sceVaudioChRelease();
	for (i = 0; i < 8; ++i)
		sceAudioChRelease(i);

	sceAudioOutput2Reserve(1024);
	checkpoint("Output2 channel taken: %08x", sceVaudioChReserve(256, 48000, 2));
	sceAudioOutput2Release();

	sceAudioOutput2Reserve(1024);
	sceVaudioChRelease();
	checkpoint("Output2 channel taken (wrong release): %08x", sceVaudioChReserve(256, 48000, 2));
	sceVaudioChRelease();

	// 257, 384, 2047, etc. make the thread never wake.
	checkpointNext("Sample counts:");
	for (i = 0; i < ARRAY_SIZE(sampleCounts); ++i) {
		checkpoint("  %d: %08x", sampleCounts[i], sceVaudioChReserve(sampleCounts[i], 48000, 2));
		sceVaudioChRelease();
	}

	// 4 makes the thread never wake.
	checkpointNext("Channel counts:");
	for (i = 0; i < ARRAY_SIZE(channelCounts); ++i) {
		if (channelCounts[i] != 4) {
			checkpoint("  %d: %08x", channelCounts[i], sceVaudioChReserve(256, 48000, channelCounts[i]));
			sceVaudioChRelease();
		}
	}

	// Wrong frequencies ruin it for life (well, until unload at least.)
	// It will only return busy after that.
	checkpointNext("Frequencies:");
	const static int vfrequencies[] = {0, 8000, 11025, 12000, 16000, 22050, 24000, 32000, 48000, 64000};
	for (i = 0; i < ARRAY_SIZE(vfrequencies); ++i) {
		checkpoint("  %d: %08x", vfrequencies[i], sceVaudioChReserve(256, vfrequencies[i], 2));
		sceVaudioChRelease();
	}

	return 0;
}