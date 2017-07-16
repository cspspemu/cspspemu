#include <pspatrac3.h>

extern "C" {
	typedef struct {
		u8 *writePos;
		u32 writableBytes;
		u32 minWriteBytes;
		u32 filePos;
	} AtracSingleResetBufferInfo;

	typedef struct {
		AtracSingleResetBufferInfo first;
		AtracSingleResetBufferInfo second;
	} AtracResetBufferInfo;

	int sceAtracGetSecondBufferInfo(int atracID, u32 *puiPosition, u32 *puiDataByte);
	int sceAtracGetNextDecodePosition(int atracID, u32 *puiSamplePosition);
	int sceAtracGetAtracID(int codecType);
	int sceAtracReinit(int at3origCount, int at3plusCount);
	int sceAtracSetData(int atracID, void *buf, SceSize bufSize);
	int sceAtracResetPlayPosition(int atracID, int sampleCount, int bytesWrittenFirstBuf, int bytesWrittenSecondBuf);
	int sceAtracGetBufferInfoForResetting(int atracID, int position, AtracResetBufferInfo *bufferInfo);
	int sceAtracSetHalfwayBufferAndGetID(void *buf, int bufSize, int readBytes);
}
