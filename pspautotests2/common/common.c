#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <assert.h>
#include <pspdebug.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspctrl.h>
#include <pspdisplay.h>
#include <pspiofilemgr.h>
//#include <pspkdebug.h>

#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <sys/lock.h>
#include <sys/fcntl.h>

//#include "local.h"

#include "sysmem-imports.h"

/*
enum PspThreadAttributes
{
	PSP_THREAD_ATTR_VFPU = 0x00004000,
	PSP_THREAD_ATTR_USER = 0x80000000,
	PSP_THREAD_ATTR_USBWLAN = 0xa0000000,
	PSP_THREAD_ATTR_VSH = 0xc0000000,
	PSP_THREAD_ATTR_SCRATCH_SRAM = 0x00008000,
	PSP_THREAD_ATTR_NO_FILLSTACK = 0x00100000,
	PSP_THREAD_ATTR_CLEAR_STACK = 0x00200000,
};
*/
/*
enum PspModuleInfoAttr
{
	PSP_MODULE_USER			= 0,
	PSP_MODULE_NO_STOP		= 0x0001,
	PSP_MODULE_SINGLE_LOAD	= 0x0002,
	PSP_MODULE_SINGLE_START	= 0x0004,
	PSP_MODULE_KERNEL		= 0x1000,
};
*/

PSP_MODULE_INFO("TESTMODULE", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_USER | PSP_THREAD_ATTR_VFPU);
//PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_USER);

#define EMULATOR_DEVCTL__GET_HAS_DISPLAY 0x00000001
#define EMULATOR_DEVCTL__SEND_OUTPUT     0x00000002
#define EMULATOR_DEVCTL__IS_EMULATOR     0x00000003
#define EMULATOR_DEVCTL__SEND_CTRLDATA   0x00000010
#define EMULATOR_DEVCTL__EMIT_SCREENSHOT 0x00000020

unsigned int RUNNING_ON_EMULATOR = 0;
unsigned int CHECKPOINT_ENABLE_TIME = 0;
unsigned int CHECKPOINT_OUTPUT_DIRECT = 0;
unsigned int HAS_DISPLAY = 1;

// 21 MB to give space for thread stacks and etc.
unsigned int sce_newlib_heap_kb_size = 21504;

extern int test_main(int argc, char *argv[]);

FILE stdout_back = {NULL};
//int KprintfFd = 0;

char schedfBuffer[65536];
unsigned int schedfBufferPos = 0;

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	if (CHECKPOINT_OUTPUT_DIRECT) {
		// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
		vprintf(format, args);
	} else {
		schedfBufferPos += vsprintf(schedfBuffer + schedfBufferPos, format, args);
	}
	va_end(args);
}

void flushschedf() {
	printf("%s", schedfBuffer);
	schedfBuffer[0] = '\0';
	schedfBufferPos = 0;
}

SceUID reschedThread;
volatile int didResched = 0;
int reschedFunc(SceSize argc, void *argp) {
	didResched = 1;
	return 0;
}

u64 lastCheckpoint = 0;
void checkpoint(const char *format, ...) {
	u64 currentCheckpoint = sceKernelGetSystemTimeWide();
	if (CHECKPOINT_ENABLE_TIME) {
		schedf("[%s/%lld] ", didResched ? "r" : "x", currentCheckpoint - lastCheckpoint);
	} else {
		schedf("[%s] ", didResched ? "r" : "x");
	}

	sceKernelTerminateThread(reschedThread);

	if (format != NULL) {
		va_list args;
		va_start(args, format);
		if (CHECKPOINT_OUTPUT_DIRECT) {
			// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
			vprintf(format, args);
		} else {
			schedfBufferPos += vsprintf(schedfBuffer + schedfBufferPos, format, args);
		}
		va_end(args);
	}

	didResched = 0;
	sceKernelStartThread(reschedThread, 0, NULL);

	if (format != NULL) {
		schedf("\n");
	}
	
	lastCheckpoint = currentCheckpoint;
}

void checkpointNext(const char *title) {
	if (schedfBufferPos != 0) {
		schedf("\n");
	}
	flushschedf();
	didResched = 0;
	if (title != NULL)
		checkpoint(title);
}

static int writeStdoutHook(struct _reent *ptr, void *cookie, const char *buf, int buf_len) {
	char temp[1024 + 1];

	//if (KprintfFd > 0) sceIoWrite(KprintfFd, buf, buf_len);
	if (RUNNING_ON_EMULATOR) {
		sceIoDevctl("emulator:", EMULATOR_DEVCTL__SEND_OUTPUT, (void *)buf, buf_len, NULL, 0);
	}

	if (buf_len < sizeof(temp)) {
		if (HAS_DISPLAY) {
			memcpy(temp, buf, buf_len);
			temp[buf_len] = 0;

			//Kprintf("%s", temp);
			pspDebugScreenPrintf("%s", temp);
		}
	}
	
	if (stdout_back._write != NULL) {
		return stdout_back._write(ptr, cookie, buf, buf_len);
	} else {
		return buf_len;
	}
}

typedef int (*SdkVerFunc)(u32 ver);
typedef struct SdkVerFuncTable {
	u32 id;
	SdkVerFunc func;
} SdkVerFuncTable;

static SdkVerFuncTable sdkVerFuncs[] = {
	{0, sceKernelSetCompiledSdkVersion},
	{370, sceKernelSetCompiledSdkVersion370},
	{380, sceKernelSetCompiledSdkVersion380_390},
	{395, sceKernelSetCompiledSdkVersion395},
	{401, sceKernelSetCompiledSdkVersion401_402},
	{500, sceKernelSetCompiledSdkVersion500_505},
	{507, sceKernelSetCompiledSdkVersion507},
	{600, sceKernelSetCompiledSdkVersion600_602},
	{603, sceKernelSetCompiledSdkVersion603_605},
	{606, sceKernelSetCompiledSdkVersion606},
};

static void updateSdkVer(int argc, char *argv[]) {
	int i = 0;
	u32 ver = 0xFFFFFFFF;
	u32 funcID = 0;
	for (i = 1; i < argc; ++i) {
		if (!strncmp(argv[i], "--sdkver=", strlen("--sdkver="))) {
			ver = strtoul(argv[i] + strlen("--sdkver="), NULL, 16);
		}
		if (!strncmp(argv[i], "--sdkver-func=", strlen("--sdkver-func="))) {
			funcID = strtol(argv[i] + strlen("--sdkver-func="), NULL, 10);
		}
	}

	SdkVerFunc func = NULL;
	for (i = 0; i < sizeof(sdkVerFuncs) / sizeof(sdkVerFuncs[0]); ++i) {
		if (sdkVerFuncs[i].id == funcID) {
			func = sdkVerFuncs[i].func;
		}
	}

	if (func == NULL) {
		fprintf(stderr, "Unknown sdkver-func value.\n");
		exit(1);
	}

	if (ver != 0xFFFFFFFF) {
		if (func(ver) != 0) {
			printf("WARNING: Setting sdkver returned failure.\n");
		}
	}
}

void test_begin() {
	if (HAS_DISPLAY) {
		pspDebugScreenInit();
	}

	if (RUNNING_ON_EMULATOR && !HAS_DISPLAY) {
		fclose(stdout);
		stdout = fmemopen(alloca(4), 4, "wb");
		stdout_back._write = NULL;
		
		//stderr = stdout;
		setbuf(stdout, NULL);
	} else {
    // Send the output to the host.
		freopen("host0:/__testoutput.txt", "wb", stdout);
		freopen("host0:/__testerror.txt", "wb", stderr);
		stdout_back._write = stdout->_write;
	}
	stdout->_write = writeStdoutHook;

	setvbuf(stdout, NULL, _IONBF, 0);
	setvbuf(stderr, NULL, _IONBF, 0);
	
	setbuf(stderr, NULL);

	reschedThread = sceKernelCreateThread("resched", &reschedFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);
}

void test_end() {
	flushschedf();

	fflush(stdout);
	fflush(stderr);
	
	fclose(stdout);
	fclose(stderr);

	if (!RUNNING_ON_EMULATOR) {
		FILE *finish = fopen("host0:/__testfinish.txt", "wb");
		if (finish)
		{
			fwrite("1", sizeof(char), 1, finish);
			fclose(finish);
		}
	}

  // Disabled the wait, much more convienent when running automated.
	if (0 && !RUNNING_ON_EMULATOR) {
		SceCtrlData key;
		while (1) {
			sceCtrlReadBufferPositive(&key, 1);
			if (key.Buttons & PSP_CTRL_CROSS) break;
		}
	}
	
	//fclose(stdout);
	sceKernelExitGame();
	
	exit(0);
}

int test_psp_exit_callback(int arg1, int arg2, void *common) {
	exit(0);
	return 0;
}

int test_psp_callback_thread(SceSize args, void *argp) {
	int cbid;
	cbid = sceKernelCreateCallback("Exit Callback", test_psp_exit_callback, NULL);
	sceKernelRegisterExitCallback(cbid);
	sceKernelSleepThreadCB();
	return 0;
}

int test_psp_setup_callbacks(void) {
	int thid = 0;
	thid = sceKernelCreateThread("update_thread",  test_psp_callback_thread, 0x11, 0xFA0, 0, 0);
	if (thid >= 0) sceKernelStartThread(thid, 0, 0);
	return thid;
}

//#define START_WITH "ms0:/PSP/GAME/virtual"

/*
void emitInt(int v) {
	asm("syscall 0x1010");
}

void emitFloat(float v) {
	asm("syscall 0x1011");
}

void emitString(char *v) {
	asm("syscall 0x1012");
}

void emitComment(char *v) {
	asm("syscall 0x1012");
}

void emitMemoryBlock(void *address, unsigned int size) {
	asm("syscall 0x1013");
}

void emitHex(void *address, unsigned int size) {
	asm("syscall 0x1014");
}
*/

unsigned char bmpHeader[54] = {
	0x42, 0x4D, 0x38, 0x80, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00,
	0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x10, 0x01,
	0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x80,
	0x08, 0x00, 0x12, 0x0B, 0x00, 0x00, 0x12, 0x0B, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};


uint extractBits(uint value, int offset, int size) {
	return (value >> offset) & ((1 << size) - 1);
}

void emulatorEmitScreenshot() {
	int file;

	if (RUNNING_ON_EMULATOR) {
		sceIoDevctl("kemulator:", EMULATOR_DEVCTL__EMIT_SCREENSHOT, NULL, 0, NULL, 0);
	}
	else
	{
		uint topaddr;
		int bufferwidth;
		int pixelformat;

		sceDisplayGetFrameBuf((void **)&topaddr, &bufferwidth, &pixelformat, 0);
		
        if (topaddr & 0x80000000) {
            topaddr |= 0xA0000000;
        } else {
            topaddr |= 0x40000000;
        }
	
		if ((file = sceIoOpen("host0:/__screenshot.bmp", PSP_O_CREAT | PSP_O_WRONLY | PSP_O_TRUNC, 0777)) >= 0) {
			int y, x;
			uint c;
			uint* vram_row;
			uint* row_buf = (uint *)malloc(512 * 4);
			sceIoWrite(file, &bmpHeader, sizeof(bmpHeader));
			for (y = 0; y < 272; y++) {
				vram_row = (uint *)(topaddr + 512 * 4 * (271 - y));
				for (x = 0; x < 512; x++) {
					c = vram_row[x];
					/*
					row_buf[x] = (
						((extractBits(c,  0, 8)) <<  0) |
						((extractBits(c,  8, 8)) <<  8) |
						((extractBits(c, 16, 8)) << 16) |
						((                0x00 ) << 24) |
					0);
					*/
					row_buf[x] = (
						((extractBits(c, 16, 8)) <<  0) |
						((extractBits(c,  8, 8)) <<  8) |
						((extractBits(c,  0, 8)) << 16) |
						((                0x00 ) << 24) |
					0);
				}
				sceIoWrite(file, row_buf, 512 * 4);
			}
			free(row_buf);
			//sceIoWrite(file, (void *)topaddr, bufferwidth * 272 * 4);
			//sceIoFlush();
			sceIoClose(file);
		}
	}
}

void emulatorSendSceCtrlData(SceCtrlData* pad_data) {
	sceIoDevctl("kemulator:", EMULATOR_DEVCTL__SEND_CTRLDATA, pad_data, sizeof(SceCtrlData), NULL, 0);
}

int main(int argc, char *argv[]) {
	int retval = 0;

	//KprintfFd = sceIoOpen("emulator:/Kprintf", O_WRONLY, 0777);
	//RUNNING_ON_EMULATOR = (KprintfFd > 0);
	
	if (sceIoDevctl("kemulator:", EMULATOR_DEVCTL__IS_EMULATOR, NULL, 0, NULL, 0) == 0) {
		RUNNING_ON_EMULATOR = 1;
	}
	
	// TEMP HACK
	//RUNNING_ON_EMULATOR = 0;

	if (RUNNING_ON_EMULATOR) {
		sceIoDevctl("kemulator:", EMULATOR_DEVCTL__GET_HAS_DISPLAY, NULL, 0, &HAS_DISPLAY, sizeof(HAS_DISPLAY));
	}
	
	#ifdef GRAPHIC_TEST
		HAS_DISPLAY = 0;
	#endif

	//if (strncmp(argv[0], START_WITH, strlen(START_WITH)) == 0) RUNNING_ON_EMULATOR = 1;

	if (!RUNNING_ON_EMULATOR) {
		test_psp_setup_callbacks();
	}
	atexit(sceKernelExitGame);

	test_begin();
	{
		pspDebugScreenPrintf("RUNNING_ON_EMULATOR: %s - %s\n", RUNNING_ON_EMULATOR ? "yes" : "no", argv[0]);
		updateSdkVer(argc, argv);

		retval = test_main(argc, argv);
	}
	test_end();
	
	return retval;
}
