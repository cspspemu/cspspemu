#include "shared.h"

void testDecodeVideo() {
	checkpointNext("testDecodeVideo:");

	const int PACKETS_PER_RUN = 24;
	const int MAX_FRAMES = 180;

	void *vbuffer = memalign(64, 512 * 272 * 4);
	void *abuffer;

	SceInt32 atracParamNotTestedYet = 1;
	SceInt32 avcParamNotSureYet = 6;
	// TODO: Possible a struct?  Or wrong?
	SceInt32 decodeParamNotSureYet = 512;

	int j;
	for (j = 0; j < MAX_FRAMES; ++j) {
		checkpointNext("Next frame");

		int freePackets = sceMpegRingbufferAvailableSize((SceMpegRingbuffer *) &g_ringbuffer);
		checkpoint("  sceMpegRingbufferAvailableSize: %d", freePackets);
		int result = sceMpegRingbufferPut((SceMpegRingbuffer *) &g_ringbuffer, PACKETS_PER_RUN, freePackets);
		checkpoint("  sceMpegRingbufferPut: %08x", result);
		freePackets = sceMpegRingbufferAvailableSize((SceMpegRingbuffer *) &g_ringbuffer);
		checkpoint("  sceMpegRingbufferAvailableSize: %d", freePackets);

		result = sceMpegGetAtracAu(&g_mpeg, g_atrac_stream, &g_atrac_au, &abuffer);
		checkpoint("  sceMpegGetAtracAu: %08x (at %08x from %08x)", result, abuffer, g_atracData);
		schedfAu(&g_atrac_au);

		result = sceMpegAtracDecode(&g_mpeg, &g_atrac_au, abuffer, atracParamNotTestedYet);
		checkpoint("  sceMpegAtracDecode: %08x", result);
		schedfAu(&g_atrac_au);

		result = sceMpegGetAvcAu(&g_mpeg, g_avc_stream, &g_avc_au, &avcParamNotSureYet);
		checkpoint("  sceMpegGetAvcAu: %08x (%08x)", result, avcParamNotSureYet);
		schedfAu(&g_avc_au);

		SceInt32 *decodeParamNotSureYetp = &decodeParamNotSureYet;
		result = sceMpegAvcDecode(&g_mpeg, &g_avc_au, 512, &vbuffer, (SceInt32 *) &decodeParamNotSureYetp);
		checkpoint("  sceMpegAvcDecode: %08x (%08x, %08x)", result, decodeParamNotSureYet, decodeParamNotSureYetp);
		schedfAu(&g_avc_au);

		checkpoint(" *** Frame: %d ***", j);
	}

	checkpoint("  sceMpegAvcDecodeFlush: %08x", sceMpegAvcDecodeFlush(&g_mpeg));

	free(vbuffer);
}

int main(int argc, char *argv[]) {
	if (loadVideoModules() < 0) {
		return 1;
	}

	sceMpegInit();
	if (createTestMpeg(512) >= 0) {
		registMpegStreams(0, 0);
		loadMpegFile("test.pmf");
		testDecodeVideo();
		deleteTestMpeg();
	}

	unloadVideoModules();
	return 0;
}