#include <common.h>
#include <pspaudio.h>
#include <psputility.h>

int channel;
int doAudioOutputBlocking(int vol, void *buf) {
	return sceAudioOutputBlocking(channel, vol, buf);
}

int doAudioOutputPannedBlocking(int vol, void *buf) {
	return sceAudioOutputPannedBlocking(channel, vol, vol, buf);
}

int doAudioRest() {
	return sceAudioGetChannelRestLength(channel);
}

const char *roughDurationDesc(u64 t) {
	if (t < 1000)
		return "very little time";
	if (t < 85000)
		return "less than 85ms";
	if (t < 86000)
		return "about the right time for 48kHz";
	if (t < 90000)
		return "less than 90ms";
	if (t < 92000)
		return "less than 92ms";
	if (t < 93000)
		return "about the right time for 44.1kHz";
	if (t < 98000)
		return "less than 98ms";
	return "a long time";
}

void testBlockingRun(const char *title, int outputFunc(int, void*), int restFunc(), int vol, u32 *data) {
	u64 startTicks, totalTicks;
	int result;

	startTicks = sceKernelGetSystemTimeWide();
	result = outputFunc(vol, data);
	totalTicks = sceKernelGetSystemTimeWide() - startTicks;
	checkpoint("  %s: 0x%x (took %s, remaining: 0x%x samples)", title, result, roughDurationDesc(totalTicks), restFunc());
}

void testBlockingType(int outputFunc(int, void*), int restFunc(), int vol, u32 *data) {
	u64 startTicks, totalTicks;
	int result;

	testBlockingRun("first", outputFunc, restFunc, vol, data);
	testBlockingRun("second", outputFunc, restFunc, vol, data);

	startTicks = sceKernelGetSystemTimeWide();
	while (sceKernelGetSystemTimeWide() - startTicks < 94000) {
		sceKernelDelayThread(2000);
	}
	checkpoint("    after delay, before output - remaining: 0x%x samples", restFunc());
	
	testBlockingRun("after delay", outputFunc, restFunc, vol, data);
	testBlockingRun("NULL", outputFunc, restFunc, vol, NULL);
	testBlockingRun("NULL", outputFunc, restFunc, vol, NULL);
	testBlockingRun("NULL", outputFunc, restFunc, vol, NULL);
}

void testBlocking() {
	u32 data[4096 + 64];
	memset(data, 0, sizeof(data));

	channel = sceAudioChReserve(PSP_AUDIO_NEXT_CHANNEL, 4096 + 64, PSP_AUDIO_FORMAT_STEREO);
	sceAudioSetChannelDataLen(channel, 4096 + 64);
	sceAudioChangeChannelVolume(channel, 0x8000, 0x8000);

	checkpointNext("sceAudioOutputBlocking:");
	testBlockingType(&doAudioOutputBlocking, &doAudioRest, -1, data);

	checkpointNext("sceAudioOutputPannedBlocking:");
	testBlockingType(&doAudioOutputPannedBlocking, &doAudioRest, 0x8000, data);
	sceAudioChRelease(channel);

	channel = sceAudioChReserve(0, 4096 + 64, PSP_AUDIO_FORMAT_STEREO);
	sceAudioSetChannelDataLen(channel, 4096 + 64);
	sceAudioChangeChannelVolume(channel, 0x8000, 0x8000);

	checkpointNext("sceAudioOutputBlocking channel 0:");
	testBlockingType(&doAudioOutputBlocking, &doAudioRest, -1, data);

	checkpointNext("sceAudioOutputPannedBlocking channel 0:");
	testBlockingType(&doAudioOutputPannedBlocking, &doAudioRest, 0x8000, data);
	sceAudioChRelease(channel);

	checkpointNext("sceAudioOutput2OutputBlocking:");
	sceAudioOutput2Reserve(4096);
	testBlockingType(&sceAudioOutput2OutputBlocking, &sceAudioOutput2GetRestSample, 0x8000, data);
	sceAudioOutput2Release();

	checkpointNext("sceAudioSRCOutputBlocking:");
	sceAudioSRCChReserve(4096, 44100, 2);
	testBlockingType(&sceAudioSRCOutputBlocking, &sceAudioOutput2GetRestSample, 0x8000, data);
	sceAudioSRCChRelease();
}

int main(int argc, char *argv[]) {
	int result;
	int i;
	u32 data[256];
	memset(data, 0, sizeof(data));

	int channel = sceAudioChReserve(PSP_AUDIO_NEXT_CHANNEL, ARRAY_SIZE(data), PSP_AUDIO_FORMAT_STEREO);
	sceAudioSetChannelDataLen(channel, ARRAY_SIZE(data));
	sceAudioChangeChannelVolume(channel, 0x8000, 0x8000);

	checkpointNext("sceAudioOutput:");
	checkpoint("Normal: %08x", sceAudioOutput(channel, 0x8000, data));
	checkpoint("Twice: %08x", sceAudioOutput(channel, 0x8000, data));
	checkpoint("Blocking: %08x", sceAudioOutputBlocking(channel, 0x8000, data));
	checkpoint("Bad channel (blocking): %08x", sceAudioOutputBlocking(-1, 0x8000, data));
	checkpoint("Unreserved channel (blocking): %08x", sceAudioOutputBlocking(5, 0x8000, data));
	checkpoint("NULL data (blocking): %08x", sceAudioOutputBlocking(channel, 0x8000, NULL));
	checkpoint("NULL data (non blocking): %08x", sceAudioOutput(channel, 0x8000, NULL));

	// NOTE: It appears (when using noise) that any negative volume means "use default" or something.
	checkpointNext("Volumes:");
	const static int volumes[] = {-0x100, -2, -1, 0, 1, 2, 0x100, 0xFFFE, 0xFFFF, 0x10000, 0x10001, 0xFFFFE, 0xFFFFF, 0x100000, 0x100001, 0x80000000, 0x7FFFFFFF};
	for (i = 0; i < ARRAY_SIZE(volumes); ++i) {
		result = sceAudioOutput(channel, volumes[i], NULL);
		checkpoint("  Non-blocking %d: %08x", volumes[i], result);
		result = sceAudioOutputBlocking(channel, volumes[i], NULL);
		checkpoint("  Blocking %d: %08x", volumes[i], result);
	}

	checkpointNext("Panned volumes:");
	for (i = 0; i < ARRAY_SIZE(volumes); ++i) {
		result = sceAudioOutputPanned(channel, volumes[i], volumes[i], NULL);
		checkpoint("  Non-blocking %d: %08x", volumes[i], result);
		result = sceAudioOutputPannedBlocking(channel, volumes[i], volumes[i], NULL);
		checkpoint("  Blocking %d: %08x", volumes[i], result);
	}

	sceAudioChRelease(channel);

	sceAudioOutput2Reserve(ARRAY_SIZE(data));

	checkpointNext("sceAudioOutput2OutputBlocking:");
	checkpoint("Normal: %08x", sceAudioOutput2OutputBlocking(0x8000, data));
	checkpoint("NULL data: %08x", sceAudioOutput2OutputBlocking(0x8000, NULL));

	checkpointNext("Volumes:");
	for (i = 0; i < ARRAY_SIZE(volumes); ++i) {
		result = sceAudioOutput2OutputBlocking(volumes[i], data);
		checkpoint("  %d: %08x", volumes[i], result);
	}

	sceAudioSRCOutputBlocking(0, NULL);
	sceAudioOutput2Release();

	sceAudioSRCChReserve(ARRAY_SIZE(data), 48000, 2);

	checkpointNext("sceAudioSRCOutputBlocking:");
	checkpoint("Normal: %08x", sceAudioSRCOutputBlocking(0x8000, data));
	checkpoint("NULL data: %08x", sceAudioSRCOutputBlocking(0x8000, NULL));

	checkpointNext("Volumes:");
	for (i = 0; i < ARRAY_SIZE(volumes); ++i) {
		result = sceAudioSRCOutputBlocking(volumes[i], data);
		checkpoint("  %d: %08x", volumes[i], result);
	}

	sceAudioSRCOutputBlocking(0, NULL);
	sceAudioSRCChRelease();

	testBlocking();

	return 0;
}
