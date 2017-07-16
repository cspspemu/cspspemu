#include <common.h>
#include <pspgu.h>
#include <psprtc.h>
#include <pspmpegbase.h>
#include <pspmpeg.h>
#include <psputils.h>
#include <psputility.h>
#include <stdarg.h>
#include <pspdisplay.h>
#include <malloc.h>

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4) /* change this if you change to another screenmode */
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH * SCR_HEIGHT * 2) /* zbuffer seems to be 16-bit? */

static unsigned int __attribute__((aligned(16))) list[262144];
unsigned int __attribute__((aligned(16))) dlist1[] = {0};

void init() {
	sceKernelDcacheWritebackAll();

	sceGuInit();
	sceGuStart(GU_DIRECT, list);

	sceGuDrawBuffer (GU_PSM_8888, (void*)FRAME_SIZE, BUF_WIDTH);
	sceGuDepthBuffer((void *)(FRAME_SIZE * 2), BUF_WIDTH);
	sceGuOffset     (2048 - (SCR_WIDTH / 2),2048 - (SCR_HEIGHT / 2));
	sceGuViewport   (2048, 2048, SCR_WIDTH, SCR_HEIGHT);
	sceGuDepthRange (0xc350, 0x2710);
	sceGuScissor    (0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuFinish     ();
	sceGuSync       (GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);
}

typedef struct PsmfPlayerData {
	u32 videoCodec;
	u32 videoStreamNum;
	u32 audioCodec;
	u32 audioStreamNum;
	u32 playMode;
	u32 playSpeed;
} PsmfPlayerData;

typedef struct PsmfPlayerCreateData {
	void *buffer;
	u32 bufferSize;
	int threadPriority;
} PsmfPlayerCreateData;

typedef struct PsmfVideoData {
	int frameWidth;
	void *displaybuf;
	u64 displaypts;
	// TODO: Probably don't exist.
	int unk1;
	int unk2;
	int unk3;
	int unk4;
} PsmfVideoData;

int scePsmfPlayerCreate(SceUID *psmf, PsmfPlayerCreateData *data);
int scePsmfPlayerGetAudioOutSize(SceUID *psmf);
int scePsmfPlayerSetPsmfCB(SceUID *psmf, const char *filename);
int scePsmfPlayerGetPsmfInfo(SceUID *psmf, void *info);
int scePsmfPlayerStart(SceUID *psmf, PsmfPlayerData *data, int initPts);
int scePsmfPlayerGetVideoData(SceUID *psmf, PsmfVideoData *videoData);
int scePsmfPlayerGetCurrentStatus(SceUID *psmf);
int scePsmfPlayerGetCurrentPts(SceUID *psmf, u64 *pts);
int scePsmfPlayerGetAudioData(SceUID *psmf, void *audioData);
int scePsmfPlayerUpdate(SceUID *psmf);
int scePsmfPlayerSetTempBuf(SceUID *psmf, void *buffer, size_t size);
int scePsmfPlayerStop(SceUID *psmf);
int scePsmfPlayerReleasePsmf(SceUID *psmf);
int scePsmfPlayerDelete(SceUID *psmf);
int scePsmfPlayerBreak(SceUID *psmf);

int scePsmfQueryStreamOffset(void *buffer, u32 *offset);
int scePsmfQueryStreamSize(void *buffer, u32 *size);

int main(int argc, char *argv[]) {
	init();

	sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	sceUtilityLoadModule(PSP_MODULE_AV_MPEGBASE);

	if (!RUNNING_ON_EMULATOR) {
		SceUID mod1 = sceKernelLoadModule("psmf.prx", 0, NULL);
		if (mod1 < 0) {
			printf("TEST FAILURE: Please place a psmf.prx in this directory.");
			return 1;
		}
		sceKernelStartModule(mod1, 0, 0, NULL, NULL);

		SceUID mod2 = sceKernelLoadModule("libpsmfplayer.prx", 0, NULL);
		sceKernelStartModule(mod2, 0, 0, NULL, NULL);
		if (mod2 < 0) {
			printf("TEST FAILURE: Please place a libpsmfplayer.prx in this directory.");
			return 1;
		}
	}

	sceMpegInit();
	checkpointNext("Init");

	const int MAIN_BUF_SIZE = 0x00300000;
	const int TEMP_BUF_SIZE = 0x00010000;
	const int MAX_FRAMES = 180;
	// Not sure if these need to be aligned.
	char *buf1 = (char *)memalign(MAIN_BUF_SIZE, 64);
	char *buf2 = TEMP_BUF_SIZE > 0 ? (char *)memalign(TEMP_BUF_SIZE, 64) : NULL;
	// Needs a full path for some reason?
	const char *filename = RUNNING_ON_EMULATOR ? "test.pmf" : "host0:/tests/video/psmfplayer/test.pmf";

	SceUID psmf = -1;
	// Crash if any are 0.
	PsmfPlayerCreateData createData = {
		buf1, MAIN_BUF_SIZE, 0x00000017,
	};

	u32 info[0x100];
	memset(info, 0xFF, 0x100);

	PsmfPlayerData data = {
		0x0000000e, 0x00000000, 0x0000000f, 0x00000000, 0x00000000, 0x00000001,
	};

	checkpointNext("Setup psmfplayer:");
	int result = scePsmfPlayerCreate(&psmf, &createData);
	checkpoint("  scePsmfPlayerCreate: %08x (psmf=%08x *psmf=%08x, **psmf=%08x)", result, psmf, *(u32 *) psmf, **(u32 **) psmf);

	if (TEMP_BUF_SIZE > 0) {
		result = scePsmfPlayerSetTempBuf(&psmf, buf2, 0x00010000);
		checkpoint("  scePsmfPlayerSetTempBuf: %08x", result);
	}

	result = scePsmfPlayerGetAudioOutSize(&psmf);
	checkpoint("  scePsmfPlayerGetAudioOutSize: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerSetPsmfCB(&psmf, filename);
	checkpoint("  scePsmfPlayerSetPsmfCB: %08x", result);
	result = scePsmfPlayerGetPsmfInfo(&psmf, &info);
	checkpoint("  scePsmfPlayerGetPsmfInfo: %08x (info: %08x %08x %08x %08x %08x %08x %08x)", result, info[0], info[1], info[2], info[3], info[4], info[5], info[6]);
	result = scePsmfPlayerStart(&psmf, &data, 0);
	checkpoint("  scePsmfPlayerStart: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);

	u8 audioData[0x2000] = {0};
	result = scePsmfPlayerGetAudioData(&psmf, audioData);
	checkpoint("  scePsmfPlayerGetAudioData: %08x", result);

	u64 pts = -1;
	result = scePsmfPlayerGetCurrentPts(&psmf, &pts);
	checkpoint("  scePsmfPlayerGetCurrentPts: %08x, %lld", result, pts);

	// Change to sceGeEdramGetAddr() to see it.
	char *dbuf = (char *)malloc(512 *  272 * 4);

	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerGetAudioData(&psmf, audioData);
	checkpoint("  scePsmfPlayerGetAudioData: %08x", result);

	int i;
	for (i = 0; i < MAX_FRAMES; i++) {
		checkpointNext("Next frame");
		result = scePsmfPlayerGetCurrentStatus(&psmf);
		checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
		result = scePsmfPlayerUpdate(&psmf);
		checkpoint("  scePsmfPlayerUpdate: %08x", result);
		result = scePsmfPlayerUpdate(&psmf);
		checkpoint("  scePsmfPlayerUpdate: %08x", result);
		result = scePsmfPlayerGetCurrentStatus(&psmf);
		checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
		if (result == 512)
			break;
		PsmfVideoData videoData = {512};
		videoData.displaybuf = dbuf;
		result = scePsmfPlayerGetVideoData(&psmf, &videoData);
		checkpoint("  scePsmfPlayerGetVideoData: %08x (%08x, %08x/%08x, %lld, %08x, %08x, %08x, %08x)", result, videoData.frameWidth, videoData.displaybuf, dbuf, videoData.displaypts, videoData.unk1, videoData.unk2, videoData.unk3, videoData.unk4);
		result = scePsmfPlayerGetCurrentStatus(&psmf);
		checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);

		result = scePsmfPlayerGetAudioData(&psmf, audioData);
		checkpoint("  scePsmfPlayerGetAudioData: %08x", result);

		// Add a wait here like sceDisplayWaitVblankStart() if you want to see smooth playback.
	}

	checkpointNext("End state checks:");
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerStop(&psmf);
	checkpoint("  scePsmfPlayerStop: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerBreak(&psmf);
	checkpoint("  scePsmfPlayerBreak: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerReleasePsmf(&psmf);
	checkpoint("  scePsmfPlayerReleasePsmf: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);
	result = scePsmfPlayerDelete(&psmf);
	checkpoint("  scePsmfPlayerDelete: %08x", result);
	result = scePsmfPlayerGetCurrentStatus(&psmf);
	checkpoint("  scePsmfPlayerGetCurrentStatus: %08x", result);

	return 0;
}