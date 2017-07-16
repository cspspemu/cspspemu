#include "shared.h"

SceMpeg g_mpeg;
SceMpegRingbuffer2 g_ringbuffer;
void *g_mpegData = NULL;
void *g_ringbufferData = NULL;
void *g_atracData = NULL;
void *g_avc_buf = NULL;
SceMpegAu g_avc_au;
SceMpegAu g_atrac_au;
SceUID g_mpegFile;
SceInt32 g_streamOffset;
SceMpegStream *g_avc_stream;
SceMpegStream *g_atrac_stream;

int loadVideoModules() {
	int status = 0;
	status |= sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_ATRAC3PLUS);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_SASCORE);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_MP3);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_MPEGBASE);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_VAUDIO);
	status |= sceUtilityLoadModule(PSP_MODULE_AV_AAC);
	if (status != 0) {
		printf("ERROR: Could not load modules.\n");
		return -1;
	}
	return 0;
}

void unloadVideoModules() {
	sceMpegFinish();

	int status = 0;
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_AVCODEC);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_ATRAC3PLUS);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_SASCORE);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_MP3);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_MPEGBASE);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_VAUDIO);
	status |= sceUtilityUnloadModule(PSP_MODULE_AV_AAC);

	if (status != 0) {
		printf("ERROR: Could not unload modules.\n");
	}
}

SceInt32 testMpegCallback(void *data, SceInt32 numPackets, void *arg) {
	checkpoint("  - mpeg_callback called: %08x, %d, %08x", (char *) data - (char *) g_ringbufferData, numPackets, (unsigned int) arg);

	if (numPackets > 0)
	{
		int read = sceIoRead(g_mpegFile, data, 2048 * numPackets);
		checkpoint("    sceIoRead: %08x, returning %d", read, read / 2048);
		return read / 2048;
	}
	else
	{
		checkpoint("    asked for negative packets, ignoring");
		return 0;
	}
}

int createTestMpeg(int numPackets) {
	checkpointNext("createTestMpeg:");
	int ringbufSize = sceMpegRingbufferQueryMemSize(numPackets);
	checkpoint("  sceMpegRingbufferQueryMemSize: %08x", ringbufSize);
	int bufSize = sceMpegQueryMemSize(0);
	checkpoint("  sceMpegQueryMemSize: %08x", bufSize);
	g_ringbufferData = malloc(ringbufSize);
	g_mpegData = malloc(bufSize);

	if (g_mpegData == NULL || g_ringbufferData == NULL) {
		printf("TEST FAILURE: Could not allocate buffers?\n");
		free(g_ringbufferData);
		free(g_mpegData);
		return -1;
	}

	int result = sceMpegRingbufferConstruct((SceMpegRingbuffer *) &g_ringbuffer, 512, g_ringbufferData, ringbufSize, &testMpegCallback, (void *) 0x1234);
	checkpoint("  sceMpegRingbufferConstruct: %08x", result);
	if (result != 0) {
		printf("TEST FAILURE: Unable to create ringbuffer: %08x\n", result);
		free(g_ringbufferData);
		free(g_mpegData);
		return result;
	}
	result = sceMpegCreate(&g_mpeg, g_mpegData, bufSize, (SceMpegRingbuffer *) &g_ringbuffer, 512, 0, 0);
	checkpoint("  sceMpegCreate: %08x", result);
	if (result != 0) {
		printf("TEST FAILURE: Unable to create mpeg: %08x\n", result);
		sceMpegRingbufferDestruct((SceMpegRingbuffer *) &g_ringbuffer);
		free(g_ringbufferData);
		free(g_mpegData);
		return result;
	}

	SceMpegBuffer *mpeg = (SceMpegBuffer *) g_mpeg;
	checkpoint("  MPEG: %.*s", 8, mpeg->header.magic);

	return 0;
}

void deleteTestMpeg() {
	if (g_mpegData != NULL) {
		sceMpegDelete(&g_mpeg);
		free(g_mpegData);
	}
	if (g_ringbufferData != NULL) {
		sceMpegRingbufferDestruct((SceMpegRingbuffer *) &g_ringbuffer);
		free(g_ringbufferData);
	}
	free(g_atracData);
	sceIoClose(g_mpegFile);
}

void registMpegStreams(int vid, int aud) {
	checkpointNext("registMpegStreams:");
	g_avc_stream = sceMpegRegistStream(&g_mpeg, 0, vid);
	checkpoint("  sceMpegRegistStream: %08x", (char *) g_avc_stream - (char *) g_mpeg);
	g_atrac_stream = sceMpegRegistStream(&g_mpeg, 1, aud);
	checkpoint("  sceMpegRegistStream: %08x", (char *) g_atrac_stream - (char *) g_mpeg);

	g_avc_buf = sceMpegMallocAvcEsBuf(&g_mpeg);
	checkpoint("  sceMpegMallocAvcEsBuf: %08x", (unsigned int) g_avc_buf);

	int result = sceMpegInitAu(&g_mpeg, g_avc_buf, &g_avc_au);
	checkpoint("  sceMpegInitAu AVC: %08x", result);

	SceInt32 atrac_esSize, atrac_outSize;
	result = sceMpegQueryAtracEsSize(&g_mpeg, &atrac_esSize, &atrac_outSize);
	checkpoint("  sceMpegQueryAtracEsSize: %08x (%08x, %08x)", result, (unsigned int) atrac_esSize, (unsigned int) atrac_outSize);

	// Not sure?
	g_atracData = memalign(64, atrac_outSize);
	result = sceMpegInitAu(&g_mpeg, g_atracData, &g_atrac_au);
	checkpoint("  sceMpegInitAu ATRAC: %08x", result);
}

void loadMpegFile(const char *filename) {
	checkpointNext("loadMpegFile:");
	g_mpegFile = sceIoOpen(filename, PSP_O_RDONLY, 0777);
	checkpoint("  sceIoOpen: %s", g_mpegFile > 0 ? "OK" : "Failed");

	char header[2048];
	checkpoint("  sceIoRead: %08x", sceIoRead(g_mpegFile, header, 2048));

	int result = sceMpegQueryStreamOffset(&g_mpeg, header, &g_streamOffset);
	checkpoint("  sceMpegQueryStreamOffset: %08x (%08x)", result, (unsigned int) g_streamOffset);
	SceInt32 streamSize;
	result = sceMpegQueryStreamSize(header, &streamSize);
	checkpoint("  sceMpegQueryStreamSize: %08x (%08x)", result, (unsigned int) streamSize);

	sceIoLseek(g_mpegFile, g_streamOffset, SEEK_SET);
}

void schedfAu(SceMpegAu *au) {
	SceMpegAu2 *simpler = (SceMpegAu2 *) au;

	const char *esName = "??";
	if (simpler->esBuffer == (u32) g_atracData)
		esName = "ATRAC";
	else if (simpler->esBuffer == (u32) g_avc_buf)
		esName = "AVC";

	if (simpler->presentedTimeHigh == -1 && simpler->presentedTimeLow == -1 && simpler->decodeTimeHigh == -1 && simpler->decodeTimeLow == -1) {
		checkpoint("    Au: presented=%d/%d, decoded=%d/%d, es=%s, size=%d", simpler->presentedTimeHigh, simpler->presentedTimeLow, simpler->decodeTimeHigh, simpler->decodeTimeLow, esName, simpler->auSize);
	} else {
		checkpoint("    Au (interesting): presented=%d/%d, decoded=%d/%d, es=%s, size=%d", simpler->presentedTimeHigh, simpler->presentedTimeLow, simpler->decodeTimeHigh, simpler->decodeTimeLow, esName, simpler->auSize);
	}
}
