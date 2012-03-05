#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdebug.h>
#include <pspcrypt.h>
#include <psputilsforkernel.h>
#include <pspthreadman_kernel.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include <pspdecrypt.h>
#include <systemctrl.h>

PSP_MODULE_INFO("prxdecrypter_prx_02g", 0x5006, 1, 0);
PSP_MAIN_THREAD_ATTR(0);

int sceUtilsBufferCopyWithRange_02g(void *inbuf, SceSize insize, void *outbuf, int outsize, int cmd) {
	int k1 = pspSdkSetK1(0);
	int err = sceUtilsBufferCopyWithRange(inbuf, insize, outbuf, outsize, cmd);
	pspSdkSetK1(k1);
	return err;
}

int pspDecompressKl3e(void *outbuf, u32 outcapacity, void *inbuf, void *unk) {
	int (* decompress)(void *, u32, void *, void *);
	
	u32 *mod = (u32 *)sceKernelFindModuleByName("sceLoadExec");
	u32 text_addr = *(mod+27);
	decompress = (void *)(text_addr+0);

	return decompress(outbuf, outcapacity, inbuf, unk);
}

static int _pspDecompress_02g(u32 *arg) {
	int retsize;
	u8 *inbuf = (u8 *)arg[0];
	u8 *outbuf = (u8 *)arg[1];
	u32 outcapacity = arg[2];
	
	if (inbuf[0] == 0x1F && inbuf[1] == 0x8B) {
		retsize = sceKernelGzipDecompress(outbuf, outcapacity, inbuf, NULL);
	}
	else if (memcmp(inbuf, "2RLZ", 4) == 0) {
		int (*lzrc)(void *outbuf, u32 outcapacity, void *inbuf, void *unk) = NULL;
		
		if (sceKernelDevkitVersion() >= 0x03080000) {
			u32 *mod = (u32 *)sceKernelFindModuleByName("sceNp9660_driver");
			if (!mod) {
				SceUID modload = sceKernelLoadModule("flash0:/kd/np9660.prx", 0, 0);
				if (!modload) return -1;
				mod = (u32 *)sceKernelFindModuleByUID(modload);
				if (!mod) return -3;
			}
			u32 *code = (u32 *)mod[27];

			int i;
			
			for (i = 0; i < 0x8000; i++) {
				if (code[i] == 0x27bdf4f0 && code[i+20] == 0x34018080) {
					lzrc = (void *)&code[i];
					break;
				} 
			}

			if (i == 0x8000) return -2;
		}
		else {
			lzrc = (void *)sctrlHENFindFunction("sceSystemMemoryManager", "UtilsForKernel", 0x7DD07271);
		}
		
		retsize = lzrc(outbuf, outcapacity, inbuf+4, NULL);
	}
	else if (memcmp(inbuf, "KL4E", 4) == 0)	{
		int (*kl4e)(void *outbuf, u32 outcapacity, void *inbuf, void *unk) = NULL;
		kl4e = (void *)sctrlHENFindFunction("sceSystemMemoryManager", "UtilsForKernel", 0x6C6887EE);

		retsize = kl4e(outbuf, outcapacity, inbuf+4, NULL);
	}
	else if (memcmp(inbuf, "KL3E", 4) == 0) {
		retsize = pspDecompressKl3e(outbuf, outcapacity, inbuf+4, NULL);
	}
	else {
		retsize = -1;
	}

	return retsize;
}

int pspDecompress_02g(const u8 *inbuf, u8 *outbuf, u32 outcapacity)
{
	int k1 = pspSdkSetK1(0);
	u32 arg[3];

	arg[0] = (u32)inbuf;
	arg[1] = (u32)outbuf;
	arg[2] = outcapacity;
	
	int res = sceKernelExtendKernelStack(0x2000, (void *)_pspDecompress_02g, arg);
	
	pspSdkSetK1(k1);
	return res;
}

int module_start(SceSize args, void *argp)
{
	return 0;
}

int module_stop(void)
{
	return 0;
}
