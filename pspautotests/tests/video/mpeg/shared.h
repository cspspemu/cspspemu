#define sceMpegFinish sceMpegFinish_IGNORE
#define sceMpegRingbufferDestruct sceMpegRingbufferDestruct_IGNORE
#define sceMpegDelete sceMpegDelete_IGNORE

#include <common.h>

#include <pspkernel.h>
#include <pspsdk.h>
#include <psptypes.h>
#include <psppower.h>
#include <psputilsforkernel.h>
#include <pspdisplay.h>
#include <pspge.h>
#include <pspgu.h>
#include <pspctrl.h>
#include <pspaudio.h>

#include <stdio.h>
#include <malloc.h>
#include <string.h>

#include <pspmpeg.h>
#include <psputility.h>

#undef sceMpegFinish
#undef sceMpegRingbufferDestruct
#undef sceMpegDelete
extern int sceMpegFinish();
extern int sceMpegRingbufferDestruct(SceMpegRingbuffer* Ringbuffer);
extern int sceMpegDelete(SceMpeg *mpeg);
extern int sceMpegAvcDecodeDetail(SceMpeg *mpeg, void *detail);
extern int sceMpegAvcDecodeFlush(SceMpeg *mpeg);

#pragma pack(1)

typedef struct SceMpegRingbuffer2 {
	s32 packetsTotal;
	s32 packetsRead;
	s32 packetsWritten;
	s32 packetsAvail;
	s32 packetSize;
	void *data;
	sceMpegRingbufferCB callback;
	void *callbackArg;
	void *dataEnd;
	int unknownValue;
	SceMpeg mpeg;
	u32 gp;
} SceMpegRingbuffer2;

typedef struct SceMpegBufferHeader {
	// "LIBMPEG"
	char magic[8];
	// "001"
	char version[4];
	// Always -1?
	u32 pad1;
	SceMpegRingbuffer2 *ringbuffer;
	// Same as ringbuffer->dataEnd?
	void *dataEnd;
	// Points to start of struct + 0x0740.
	void *unknown1;
	// Points to start of struct + 0x0740 + 0x0740.
	void *unknown2;
	// Points to start of struct + 0x0740 + 0x0740 + 0x1E00.
	void *unknown3;
	// Always 0?
	u32 unknown4;
	// Always 99?
	int unknownListLength;
} SceMpegBufferHeader;

struct SceMpegBufferUnknownList1;
typedef struct SceMpegBufferUnknownList1 {
	u32 unknown1;
	u32 unknown2;
	struct SceMpegBufferUnknownList1 *next;
	u32 unknown3;
} SceMpegBufferUnknownList1;

typedef struct SceMpegBuffer {
	SceMpegBufferHeader header;
	SceMpegBufferUnknownList1 unknownList[99];
	u32 unknown4[8];
	SceUID unknownSema1;
	u32 unknown5[7];
	SceUID unknownSema2;
} SceMpegBuffer;

typedef struct SceMpegAu2 {
	u32 presentedTimeHigh;
	u32 presentedTimeLow;
	u32 decodeTimeHigh;
	u32 decodeTimeLow;
	u32 esBuffer;
	u32 auSize;
} SceMpegAu2;

// Helpers just to make individual tests easier, mpegs are complex.

extern SceMpeg g_mpeg;
extern SceMpegRingbuffer2 g_ringbuffer;
extern void *g_mpegData;
extern void *g_ringbufferData;
extern void *g_atracData;
extern void *g_avc_buf;
extern SceMpegAu g_avc_au;
extern SceMpegAu g_atrac_au;
extern SceUID g_mpegFile;
extern SceInt32 g_streamOffset;
extern SceMpegStream *g_avc_stream;
extern SceMpegStream *g_atrac_stream;

extern int loadVideoModules();
extern void unloadVideoModules();

extern int createTestMpeg(int numPackets);
extern void deleteTestMpeg();
extern void registMpegStreams(int vid, int aud);
extern void loadMpegFile(const char *filename);
extern void schedfAu(SceMpegAu *au);