#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspatrac3.h>
#include <pspaudio.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <psputility.h>

int sceAtracGetSecondBufferInfo(int atracID, u32 *puiPosition, u32 *puiDataByte);
int sceAtracGetNextDecodePosition(int atracID, u32 *puiSamplePosition);

int main(int argc, char *argv[]) {
	char *at3_data;
	int at3_size;

	char *decode_data;
	int decode_size;
	int n;

	FILE *file;

	int atracID;
	int maxSamples = 0;
	int result;
	int channel;
	
	u32 puiPosition;
	u32 puiDataByte;
	
	if ((file = fopen("sample.at3", "rb")) != NULL) {
		fseek(file, 0, SEEK_END);
		at3_size = ftell(file);
		
		fseek(file, 0, SEEK_SET);
		
		at3_data = malloc(at3_size);
		decode_data = malloc(decode_size = 512 * 1024);
		memset(at3_data, 0, at3_size);
		memset(decode_data, 0, decode_size);
		
		fread(at3_data, at3_size, 1, file);

		fclose(file);
	}

	int id = sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	int id2 = sceUtilityLoadModule(PSP_MODULE_AV_ATRAC3PLUS);

	if ((id >= 0 || (u32) id == 0x80020139UL) && (id2 >= 0 || (u32) id2 == 0x80020139UL)) {
		printf("Audio modules: OK\n");
	} else {
		printf("Audio modules: Failed %08x %08x\n", id, id2);
	}

	printf("at3: %08X, %08X\n", (unsigned int)at3_data != 0, at3_size);
	printf("Header: %s\n", (char *)at3_data);
		
	atracID = sceAtracSetDataAndGetID(at3_data, at3_size);
	if (atracID < 0) {
		printf("sceAtracSetDataAndGetID: Failed %08x\n", atracID);
		return 1;
	} else {
		printf("sceAtracSetDataAndGetID: OK\n");
	}
	
  u32 bitrate;
  result = sceAtracGetBitrate(atracID, &bitrate);
  printf("%i=sceAtracGetBitrate: %i\n", result, bitrate);

  u32 channelNum;
  result = sceAtracGetChannel(atracID, &channelNum);
  printf("%i=sceAtracGetChannel: %i\n", result, channelNum);

	result = sceAtracSetLoopNum(atracID, 2);
	printf("sceAtracSetLoopNum: %08X\n", result);

	result = sceAtracGetMaxSample(atracID, &maxSamples);
	printf("sceAtracGetMaxSample: %08X, %d\n", result, maxSamples);
	
	channel = sceAudioChReserve(0, maxSamples, PSP_AUDIO_FORMAT_STEREO);
	printf("sceAudioChReserve: %08X\n", channel);
	
	result = sceAtracGetSecondBufferInfo(atracID, &puiPosition, &puiDataByte);
	printf("sceAtracGetSecondBufferInfo: %08X, %u, %u\n", result, (unsigned int)puiPosition, (unsigned int)puiDataByte);
	
	int end = 0;
	int steps = 0;
	while (!end && steps < 65536) {
		//int remainFrame = -1;
		int remainFrame = 0;
		//int decodeBufferPosition = 0;
		int samples = 0;
		int nextSample = 0;
		u32 nextPosition = 0;
		
		if (steps < 6) {
			result = sceAtracGetNextSample(atracID, &nextSample);
			printf("sceAtracGetNextSample(%d): %d\n", result, nextSample);
			result = sceAtracGetNextDecodePosition(atracID, &nextPosition);
			printf("sceAtracGetNextDecodePosition(%d): %u\n", result, (unsigned int)nextPosition);
		}

		result = sceAtracDecodeData(atracID, (u16 *)decode_data, &samples, &end, &remainFrame);
		
		sceAudioSetChannelDataLen(channel, samples);
		sceAudioOutputBlocking(channel, 0x8000, decode_data);
		
		result = sceAtracGetRemainFrame(atracID, &remainFrame);

		if (steps < 6) {
			printf("sceAtracDecodeData: %08X, at3_size: %d, decode_size: %d, samples: %d, end: %d, remainFrame: %d\n\n", result, at3_size, decode_size, samples, end, remainFrame);
			if (steps == 1) {
				for (n = 0; n < 32; n++) printf("%04X ", (u16)decode_data[n]);
				printf("\n");
			}
			printf("sceAtracGetRemainFrame: %08X\n", result);
		}

    //u32 loopStatus = 0xc0c0c0c0;
    //result = sceAtracGetLoopStatus(atracID, 0, &loopStatus);
    //printf("%i=sceAtracGetLoopStatus: %08x\n", result, loopStatus);

    return 0;
		steps++;
	}
	
	result = sceAudioChRelease(channel);
	printf("sceAudioChRelease: %08X\n", result);
	result = sceAtracReleaseAtracID(atracID);
	printf("sceAtracReleaseAtracID: %08X\n", result);

	return 0;
}
