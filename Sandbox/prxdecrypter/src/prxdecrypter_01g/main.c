#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdebug.h>
#include <pspthreadman_kernel.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

PSP_MODULE_INFO("prxdecrypter_prx_01g", 0x1006, 1, 0);
PSP_MAIN_THREAD_ATTR(0);

typedef u32 (*PROC_LZRC)(void* destP, u32 cb, const void* scrP, u32* retP);
PROC_LZRC sceKernelLzrcDecode;
typedef u32 (*PROC_DEFLATE)(void* destP, u32 cb, const void* scrP, u32* retP);
PROC_DEFLATE sceKernelGzipDecompress;
typedef int (*PROC_MANGLE)(void* r4, u32 r5, void* r6, u32 r7, u32 r8);
PROC_MANGLE sceUtilsBufferCopyWithRange;

u8 sysmemBuffer[300000] __attribute__((aligned(0x40)));

int ReadFile(const char* file, int offset, void *buf, u32 size) {
	SceUID fd = sceIoOpen(file, PSP_O_RDONLY, 0777);
	int read;

	if (fd < 0) return fd;

	if (offset != 0) sceIoLseek(fd, offset, PSP_SEEK_SET);

	read = sceIoRead(fd, buf, size);
	sceIoClose(fd);

	return read;
}

u32 FindProc(const char* szMod, const char* szLib, u32 nid) {
	struct SceLibraryEntryTable *entry;
	SceModule *pMod;
	void *entTab;
	int entLen;

	pMod = sceKernelFindModuleByName(szMod);

	if (!pMod) return 0;
	
	int i = 0;
	entTab = pMod->ent_top;
	entLen = pMod->ent_size;

	while(i < entLen) {
		int count;
		int total;
		unsigned int *vars;

		entry = (struct SceLibraryEntryTable *) (entTab + i);
		if(entry->libname && !strcmp(entry->libname, szLib)) {
			total = entry->stubcount + entry->vstubcount;
			vars = entry->entrytable;

			if(entry->stubcount > 0) {
				for(count = 0; count < entry->stubcount; count++) {
					if (vars[count] == nid)
						return vars[count+total];					
				}
			}
		}
		i += (entry->len * 4);
	}
	return 0;
}

int InitSysEntries() {
	int k1 = pspSdkSetK1(0);
	sceUtilsBufferCopyWithRange = (PROC_MANGLE)FindProc("sceMemlmd", "semaphore", 0x4C537C72);
	sceKernelGzipDecompress = (PROC_DEFLATE)FindProc("sceKernelUtils", "UtilsForKernel", 0x78934841);
	pspSdkSetK1(k1);
	return (sceUtilsBufferCopyWithRange != NULL && sceKernelGzipDecompress != NULL);
}

int InitRlzDecompressor() {
	int k1 = pspSdkSetK1(0);
	sceKernelLzrcDecode = (PROC_LZRC)FindProc("rlzSystemMemoryManager", "UtilsForKernel", 0x7DD07271);
	pspSdkSetK1(k1);
	return (sceKernelLzrcDecode != NULL);
}

int SetupRLZ(char *path) {
	int k1 = pspSdkSetK1(0);
	int SysMemSize = ReadFile(path, 0, sysmemBuffer, sizeof(sysmemBuffer)); // load external sysmem to sysmemBuffer
	if (SysMemSize < 0) {
		pspSdkSetK1(k1);
		return 1;
	}

	int SysMemSeek;
	for (SysMemSeek = SysMemSize; SysMemSeek > 0; SysMemSeek--) {
		if (memcmp(sysmemBuffer+SysMemSeek, "sceSystemMemoryManager", 22) == 0) {
			memcpy(sysmemBuffer+SysMemSeek, "rlz", 3);

			pspSdkInstallNoDeviceCheckPatch();
			pspSdkInstallNoPlainModuleCheckPatch();
			pspSdkInstallKernelLoadModulePatch();
			SceUID SysMemMod = sceKernelLoadModuleBuffer(sysmemBuffer, SysMemSize, 0, NULL);

			if (SysMemMod < 0) {
				pspSdkSetK1(k1);
				return SysMemMod;
			}

			if (!InitRlzDecompressor()) {
				pspSdkSetK1(k1);
				return 3;
			}

			pspSdkSetK1(k1);
			return 0;
		}
	}
	pspSdkSetK1(k1);
	return 4;
}

int sceUtilsBufferCopyWithRange_01g(void *inbuf, SceSize insize, void *outbuf, int outsize, int cmd) {
	int k1 = pspSdkSetK1(0);
	int err = sceUtilsBufferCopyWithRange(inbuf, insize, outbuf, outsize, cmd);
	pspSdkSetK1(k1);
	return err;
}

int sceKernelGzipDecompress_(u8 *dest, u32 destSize, const u8 *src, void *unknown) {
	int k1 = pspSdkSetK1(0);
	int err = sceKernelGzipDecompress(dest, destSize, src, unknown);
	pspSdkSetK1(k1);
	return err;
}

int sceKernelLzrcDecode_(u32 *arg) {
	int k1 = pspSdkSetK1(0);
	int err = sceKernelLzrcDecode((u8 *)arg[0], arg[1], (u8 *)arg[2], (void *)arg[3]);
	pspSdkSetK1(k1);
	return err;
}

int pspDecompress_01g(const u8 *inbuf, u8 *outbuf, u32 outcapacity) {
	int k1 = pspSdkSetK1(0);
	int retsize;
	
	if (inbuf[0] == 0x1F && inbuf[1] == 0x8B) {
		retsize = sceKernelGzipDecompress_(outbuf, outcapacity, inbuf, NULL);
	}
	else if (memcmp(inbuf+1, "RLZ", 3) == 0) { // 2RLZ
		u32 arg[4];

		arg[0] = (u32)outbuf;
		arg[1] = outcapacity;
		arg[2] = (u32)(inbuf+4);
		arg[3] = 0;
		
		retsize = sceKernelExtendKernelStack(0x2000, (void *)sceKernelLzrcDecode_, arg);
	}
	else {
		retsize = -1;
	}

	pspSdkSetK1(k1);
	return retsize;
}

int module_start(SceSize args, void *argp)
{
	return 0;
}

int module_stop(void)
{
	return 0;
}
