#include <common.h>

#include <pspkernel.h>
#include <stdio.h>
#include <pspsdk.h>
#include "sascore.h"

//PSP_MODULE_INFO("sascore test", 0, 1, 1);
//PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

typedef struct {
	unsigned char* pointer;
	int length;
} ByteArray;

ByteArray loadData()
{
	int n;
	FILE *file;
	ByteArray data = {0};
	if ((file = fopen("test.vag", "rb")) != NULL) {
		fseek(file, 0, SEEK_END);
		data.length = ftell(file);
		data.pointer = (unsigned char *)malloc(data.length);
		memset(data.pointer, 0, data.length);
		fseek(file, 0, SEEK_SET);
		fread(data.pointer, data.length, 1, file);
		fclose(file);
	}
	if (data.length == 0) {
		printf("DATA:Can't read file\n");
	} else {
		printf("DATA:");
		for (n = 0; n < 0x20; n++) printf("%02X", data.pointer[n]);
		printf("\n");
	}
	return data;
}

// PSP_SAS_ERROR_ADDRESS = 0x80420005;
__attribute__((aligned(64))) SasCore sasCore;

__attribute__((aligned(64))) short samples[256 * 2 * 16] = {0};
ByteArray data;
int result;

int sasCoreThread(SceSize argsize, void *argdata) {
	int sasVoice = 0;
	int n, m;
	SasCore sasCoreInvalidAddress;

	printf("__sceSasInit         : 0x%08X\n", result = __sceSasInit(&sasCoreInvalidAddress, PSP_SAS_GRAIN_SAMPLES, PSP_SAS_VOICES_MAX, PSP_SAS_OUTPUTMODE_STEREO, 44100));
	printf("__sceSasInit         : 0x%08X\n", result = __sceSasInit(&sasCore, PSP_SAS_GRAIN_SAMPLES, PSP_SAS_VOICES_MAX, PSP_SAS_OUTPUTMODE_STEREO, 44100));
	printf("__sceSasSetOutputmode: 0x%08X\n", result = __sceSasSetOutputmode(&sasCore, PSP_SAS_OUTPUTMODE_STEREO));
	printf("__sceSasSetKeyOn     : 0x%08X\n", result = __sceSasSetKeyOn(&sasCore, sasVoice));
	printf("__sceSasSetVoice     : 0x%08X\n", result = __sceSasSetVoice(&sasCore, sasVoice, (char *)data.pointer, data.length, 0));
	printf("__sceSasSetPitch     : 0x%08X\n", result = __sceSasSetPitch(&sasCore, sasVoice, 4096));
	printf("__sceSasSetVolume    : 0x%08X\n", result = __sceSasSetVolume(&sasCore, sasVoice, 0x1000, 0x1000));
	printf("Data(%d):\n", data.length);
	
	//for (m = 0; m < 10000; m++) printf("%d,", data.pointer[m]);
	
	//return 0;
	
	for (m = 0; m < 6; m++) {
		printf("__sceSasCore: 0x%08X\n", result = __sceSasCore(&sasCore, samples));
		for (n = 0; n < 512; n++) printf("%d,", samples[n]);
		printf("\n");
	}
	printf("End\n");
	
	fflush(stdout);
	
	return 0;
}

// http://www.psp-programming.com/forums/index.php?action=printpage;topic=4404.0
int main(int argc, char *argv[]) {
	int thid;

	// TO AVOID: ERROR_KERNEL_LIBRARY_IS_NOT_LINKED : 0x8002013a
	printf("%08X\n", pspSdkLoadStartModule("flash0:/kd/sc_sascore.prx", PSP_MEMORY_PARTITION_KERNEL));
	
	data = loadData();

	thid = sceKernelCreateThread(
		"sas_core",
		sasCoreThread,
		0x11,
		0xFA0,
		PSP_THREAD_ATTR_USER,
		NULL
	);
	sceKernelStartThread(thid, 0, 0);
	sceKernelWaitThreadEnd(thid, NULL);
	
	return 0;
}
