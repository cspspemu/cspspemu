#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspmp3.h>
#include <pspaudio.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>

#include <psputility.h>
#include <psputility_avmodules.h>

static char	mp3Buf[128*1024]  __attribute__((aligned(64)));
static short pcmBuf[128*(1152/2)]  __attribute__((aligned(64)));

static int error_file_handle;

int fillStreamBuffer(int fd, int handle)
{
	char* dst;
	SceInt32 write;
	SceInt32 pos;
	// Get Info on the stream (where to fill to, how much to fill, where to fill from)
	int status = sceMp3GetInfoToAddStreamData(handle, (SceUChar8**)&dst, &write, &pos);
	if (status < 0)
		return 0;

	// Seek file to position requested
	status = sceIoLseek32( fd, pos, SEEK_SET );
	if (status < 0)
		return 0;

	// Read the amount of data
	int read = sceIoRead( fd, dst, write );
	if (read < 0)
		return 0;
	else if (read == 0)
		// End of file?
		return 0;

	// Notify mp3 library about how much we really wrote to the stream buffer
	status = sceMp3NotifyAddStreamData( handle, read );
	if (status < 0)
		return 0;

	return (pos > 0);
}

int main(int argc, char *argv[]) {
	int file_handle;
	int mp3_handle;

	int status;

	int id = sceUtilityLoadModule(PSP_MODULE_AV_AVCODEC);
	int id2 = sceUtilityLoadModule(PSP_MODULE_AV_MP3);

	if ((id > 0 || (u32) id == 0x80020139UL) && (id2 > 0 || (u32) id2 == 0x80020139UL)) {
		printf("Audio modules: OK\n");
	} else {
		printf("Audio modules: Failed %08x %08x\n", id, id2);
	}

	file_handle = sceIoOpen( "sample.mp3", PSP_O_RDONLY, 0777 );
	if(file_handle < 0) {
		return -1;
	}

	status = sceMp3InitResource();
	if(status < 0) {
		return -1;
	}


	// Reserve a mp3 handle for our playback
	SceMp3InitArg mp3Init;
	mp3Init.mp3StreamStart = 0;
	mp3Init.mp3StreamEnd = sceIoLseek32( file_handle, 0, SEEK_END );
	mp3Init.unk1 = 0;
	mp3Init.unk2 = 0;
	mp3Init.mp3Buf = mp3Buf;
	mp3Init.mp3BufSize = sizeof(mp3Buf);
	mp3Init.pcmBuf = pcmBuf;
	mp3Init.pcmBufSize = sizeof(pcmBuf);

	printf("sizeof(mp3Buf): %d\n", sizeof(mp3Buf));
	printf("sizeof(pcmBuf): %d\n", sizeof(pcmBuf));

	mp3_handle = sceMp3ReserveMp3Handle( &mp3Init );

	if (mp3_handle < 0)
		return -1;

	// Fill the stream buffer with some data so that sceMp3Init has something to
	// work with
	fillStreamBuffer(file_handle, mp3_handle);

	status = sceMp3Init( mp3_handle );
	if (status < 0)
		return -1;

	int channel = -1;
	int samplingRate = sceMp3GetSamplingRate( mp3_handle );
	int numChannels = sceMp3GetMp3ChannelNum( mp3_handle );
	int lastDecoded = 0;
	int volume = PSP_AUDIO_VOLUME_MAX - 400;
	int numPlayed = 0;

	printf(" %i Hz\n", samplingRate);
	printf(" %i kbit/s\n", (int)sceMp3GetBitRate( mp3_handle ));
	printf(" %s\n", numChannels==2?"Stereo":"Mono");


	int running = 200;

	while(--running > 0) {
		 // Check if we need to fill our stream buffer
		if (sceMp3CheckStreamDataNeeded( mp3_handle ) > 0)
			fillStreamBuffer(file_handle, mp3_handle);

		// Decode some samples
		short* buf;
		unsigned int bytesDecoded;
		int retries = 0;

		// We retry in case it's just that we reached the end of the stream and need
		// to loop
		for (;retries < 1; retries++)
		{
			bytesDecoded = sceMp3Decode(mp3_handle, &buf);
			if (bytesDecoded > 0)
			    break;

			if (sceMp3CheckStreamDataNeeded( mp3_handle ) <= 0)
			    break;

			if (!fillStreamBuffer(file_handle, mp3_handle)) {
				numPlayed = 0;
				running = 0;
				break;
			}
		}

		if (bytesDecoded < 0 && bytesDecoded != 0x80671402)
		{
			printf("sceMp3Decode returned 0x%08X\n", bytesDecoded);
			running = 0;
			break;
		}

		// Nothing more to decode? Must have reached end of input buffer
		if (bytesDecoded == 0 || bytesDecoded == 0x80671402)
		{
			running = 0;
			break;
		} else {
			// Reserve the Audio channel for our output if not yet done
			if (channel < 0 || lastDecoded != bytesDecoded)
			{
				if (channel >= 0)
					sceAudioSRCChRelease();

				channel = sceAudioSRCChReserve( bytesDecoded/(2*numChannels),
					samplingRate, numChannels );
			}

			// Output the decoded samples and accumulate the 
			// number of played samples to get the playtime
			numPlayed += sceAudioSRCOutputBlocking( volume, buf );
		}
	}

	// Reset the state of the player to the initial starting state
	sceMp3ResetPlayPosition( mp3_handle );
	numPlayed = 0;

	// Cleanup time...
	if (channel>=0)
	  sceAudioSRCChRelease();

	status = sceMp3ReleaseMp3Handle( mp3_handle );
	if (status<0)
		printf("sceMp3ReleaseMp3Handle returned 0x%08X\n", status);

	status = sceMp3TermResource();
	if (status<0)
		printf("sceMp3TermResource returned 0x%08X\n", status);

	status = sceIoClose( file_handle );
	if (status<0)
		printf("sceIoClose returned 0x%08X\n", status);

	file_handle = -1;
	mp3_handle = -1;

	return 0;
}