#include <pspkernel.h>
#include <pspdebug.h>
#include <pspdisplay.h>
#include <pspsdk.h>
#include <pspiofilemgr.h>
#include <pspctrl.h>
#include <string.h>
#include <libc\time.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>

#include "file.h"
#include "common.h"

// COMMENT THIS OUT TO DISABLE LOGS
#define LOGGING_ON 1

PSP_MODULE_INFO("PRXdecrypter", 0, 1, 1);
PSP_MAIN_THREAD_ATTR(0);

#define printf 		pspDebugScreenPrintf
#define setc	 	pspDebugScreenSetXY
#define clearscreen	pspDebugScreenClear
#define DEST 		"ms0:/enc/"
#define OUT		"ms0:/dec/"
#define VERSION 	"2.6"

#define buffer_size 		10000000

#define row_offset 6
u16 row;
u16 rowprev;
u16 maxrows;

typedef enum { false, true } bool;

// file list struct for ms0:/enc
FileList filelist;

// various loop variables
int x, w;

// global variable for updater prx extraction
int updatermods = 0;
int overwriteall = 0;
bool is_updater = false;

bool rlzdecompressor = false;
bool kl4edecompressor = false;
bool kl3edecompressor = false;
bool unsign_only = false;
bool resign_only = false;
bool logging_enabled = false;
SceUID logfile = 0;
int kernelver;

/************ keys *************/

#include "keys_144_byte.h"
#include "keys_16_byte.h"

/*******************************/

////////////////////////////////////////////////////////////////////
// big buffers for data. Some system calls require 64 byte alignment

// 10mb in
u8 g_dataOut[buffer_size] __attribute__((aligned(0x40)));

// 10mb out
u8 g_dataOut2[buffer_size] __attribute__((aligned(0x40)));

////////////////////////////////////////////////////////////////////
// File functions

bool g_bWriteError = false;

bool SaveFile(const char* szFile, const void *addr, int len) {
	int fd = sceIoOpen(szFile, PSP_O_CREAT | PSP_O_TRUNC | PSP_O_WRONLY, 0777);
	if (fd < 0) return false;
	bool ok = true;
	if (sceIoWrite(fd, addr, len) != len) ok = false;
	if (sceIoClose(fd) != 0) ok = false;
	return ok;
}

int ReadFile(const char* file, int offset, void *buf, u32 size) {
	SceUID fd = sceIoOpen(file, PSP_O_RDONLY, 0777);
	int read;

	if (fd < 0) return fd;

	if (offset != 0) sceIoLseek(fd, offset, PSP_SEEK_SET);

	read = sceIoRead(fd, buf, size);
	sceIoClose(fd);

	return read;
}

// menu draw functions

void refreshArrow(void)
{
	setc(1, rowprev + row_offset);
	printf(" ");
	setc(1, row + row_offset);
	printf(">");
}

void wait_release(unsigned int buttons)
{
	SceCtrlData pad;

	sceCtrlReadBufferPositive(&pad, 1);
	while (pad.Buttons & buttons)
	{
		sceKernelDelayThread(10000);
		sceCtrlReadBufferPositive(&pad, 1);
	}
}

unsigned int wait_press(unsigned int buttons)
{
	SceCtrlData pad;

	sceCtrlReadBufferPositive(&pad, 1);
	while (1)
	{
		if (pad.Buttons & buttons)
			return pad.Buttons & buttons;
		sceKernelDelayThread(10000);
		sceCtrlReadBufferPositive(&pad, 1);
	}
	return 0;   /* should never reach here */
}


///////////////////////////////////////////////////////////////////
// Logging functions

#ifdef LOGGING_ON
static int my_openlogfile(const char* szFile) {
	return sceIoOpen(szFile, PSP_O_CREAT | PSP_O_TRUNC | PSP_O_WRONLY, 0777);
}
static void my_print(int fd, const char* sz) {
	if (logging_enabled) sceIoWrite(fd, sz, strlen(sz));
}
static void my_close(int fd) {
	if (logging_enabled) sceIoClose(fd);
}
#endif

static void printfLogged(char *message, ...) {
	va_list list;
	char msg[256];	

	va_start(list, message);
	vsprintf(msg, message, list);
	va_end(list);

	if (pspDebugScreenGetY() == 33) clearscreen();
	pspDebugScreenPrintf(msg);

	#ifdef LOGGING_ON
	my_print(logfile, msg);
	#endif
}

int InitSysEntries(void);
int SetupRLZ(char *path);

int sceUtilsBufferCopyWithRange_01g(void *inbuf, SceSize insize, void *outbuf, int outsize, int cmd);
int sceUtilsBufferCopyWithRange_02g(void *inbuf, SceSize insize, void *outbuf, int outsize, int cmd);
int pspDecompress_01g(const u8 *inbuf, u8 *outbuf, u32 outcapacity);
int pspDecompress_02g(const u8 *inbuf, u8 *outbuf, u32 outcapacity);

int pspDecompress(const u8 *inbuf, u8 *outbuf, u32 outcapacity) {
	if (kernelver == 0x01050001) {
		return pspDecompress_01g(inbuf, outbuf, outcapacity);
	}
	else return pspDecompress_02g(inbuf, outbuf, outcapacity);
}

int sceUtilsBufferCopyWithRange_(void *inbuf, SceSize insize, void *outbuf, int outsize, int cmd) {
	if (kernelver == 0x01050001) {
		return sceUtilsBufferCopyWithRange_01g(inbuf, insize, outbuf, outsize, cmd);
	}
	else return sceUtilsBufferCopyWithRange_02g(inbuf, insize, outbuf, outsize, cmd);
}

/*
KIRK ERRORS:
0 - success
1 - KIRK disabled
2 - invalid mode
3 - header check failed
4 - data check failed
5 - sigcheck failed
*/

////////// Decryption 1 //////////

static TAG_INFO const* GetTagInfo(u32 tagFind) {
	int iTag;

	for (iTag = 0; iTag < sizeof(g_tagInfo)/sizeof(TAG_INFO); iTag++)
		if (g_tagInfo[iTag].tag == tagFind)
			return &g_tagInfo[iTag];

	return NULL; // not found
}

static void ExtraV2Mangle(u8* buffer1, u8 codeExtra) {
	static u8 g_dataTmp[20+0xA0] __attribute__((aligned(0x40)));
	u8* buffer2 = g_dataTmp; // aligned

	memcpy(buffer2+20, buffer1, 0xA0);
	u32* pl2 = (u32*)buffer2;
	pl2[0] = 5;
	pl2[1] = pl2[2] = 0;
	pl2[3] = codeExtra;
	pl2[4] = 0xA0;

	int ret = sceUtilsBufferCopyWithRange_(buffer2, 20+0xA0, buffer2, 20+0xA0, 7);
	if (ret != 0)
	printfLogged("extra de-mangle returns %d, ", ret);
	// copy result back
	memcpy(buffer1, buffer2, 0xA0);
}

static int DecryptPRX1(const u8* pbIn, u8* pbOut, int cbTotal, u32 tag) {
	TAG_INFO const* pti = GetTagInfo(tag);
	if (pti == NULL) return -1;

 	// build conversion into pbOut
	memcpy(pbOut, pbIn, cbTotal);
	memset(pbOut, 0, 0x150);
	memset(pbOut, 0x55, 0x40); // first $40 bytes ignored

	// step3 demangle in place
	u32* pl = (u32*)(pbOut+0x2C);
	pl[0] = 5; // number of ulongs in the header
	pl[1] = pl[2] = 0;
	pl[3] = pti->code; // initial seed for PRX
	pl[4] = 0x70;   // size

	// redo part of the SIG check (step2)
	u8 buffer1[0x150];
	memcpy(buffer1+0x00, pbIn+0xD0, 0x80);
	memcpy(buffer1+0x80, pbIn+0x80, 0x50);
	memcpy(buffer1+0xD0, pbIn+0x00, 0x80);
	if (pti->codeExtra != 0)
		ExtraV2Mangle(buffer1+0x10, pti->codeExtra);
	memcpy(pbOut+0x40 /* 0x2C+20 */, buffer1+0x40, 0x40);

	int ret;
	int iXOR;
	for (iXOR = 0; iXOR < 0x70; iXOR++)
		pbOut[0x40+iXOR] = pbOut[0x40+iXOR] ^ pti->key[0x14+iXOR];

	ret = sceUtilsBufferCopyWithRange_(pbOut+0x2C, 20+0x70, pbOut+0x2C, 20+0x70, 7);
	if (ret != 0)
	{
		printfLogged("mangle#7 returned 0x%08X, ", ret);
		return -1;
	}

	for (iXOR = 0x6F; iXOR >= 0; iXOR--)
		pbOut[0x40+iXOR] = pbOut[0x2C+iXOR] ^ pti->key[0x20+iXOR];

	memset(pbOut+0x80, 0, 0x30); // $40 bytes kept, clean up
	pbOut[0xA0] = 1;
	// copy unscrambled parts from header
	memcpy(pbOut+0xB0, pbIn+0xB0, 0x20); // file size + lots of zeros
	memcpy(pbOut+0xD0, pbIn+0x00, 0x80); // ~PSP header

	// step4: do the actual decryption of code block
	//  point 0x40 bytes into the buffer to key info
	ret = sceUtilsBufferCopyWithRange_(pbOut, cbTotal, pbOut+0x40, cbTotal-0x40, 0x1);
	if (ret != 0) {
		printfLogged("mangle#1 returned 0x%08X, ", ret);
		return -1;
	}

	return *(u32*)&pbIn[0xB0]; // size of actual data
}

////////// Decryption 2 //////////

static TAG_INFO2 *GetTagInfo2(u32 tagFind) {
	int iTag;

	for (iTag = 0; iTag < sizeof(g_tagInfo2) / sizeof(TAG_INFO2); iTag++)
		if (g_tagInfo2[iTag].tag == tagFind)
			return &g_tagInfo2[iTag];

	return NULL; // not found
}

static int Scramble(u32 *buf, u32 size, u32 code) {
	buf[0] = 5;
	buf[1] = buf[2] = 0;
	buf[3] = code;
	buf[4] = size;

	if (sceUtilsBufferCopyWithRange_(buf, size+0x14, buf, size+0x14, 7) < 0) {
		return -1;
	}

	return 0;
}

static int DecryptPRX2(const u8 *inbuf, u8 *outbuf, u32 size, u32 tag) {
	TAG_INFO2 * pti = GetTagInfo2(tag);

	if (!pti) return -1;

	int retsize = *(int *)&inbuf[0xB0];
	u8 tmp1[0x150], tmp2[0x90+0x14], tmp3[0x60+0x14];

	memset(tmp1, 0, 0x150);
	memset(tmp2, 0, 0x90+0x14);
	memset(tmp3, 0, 0x60+0x14);

	memcpy(outbuf, inbuf, size);

	if (size < 0x160) {
		printfLogged("buffer not big enough, ");
		return -2;
	}

	if (((u32)outbuf & 0x3F)) {
		printfLogged("buffer not aligned to 64 bytes, ");
		return -3;
	}

	if ((size - 0x150) < retsize) {
		printfLogged("not enough data, ");
		return -4;
	}

	memcpy(tmp1, outbuf, 0x150);

	int i, j;
	u8 *p = tmp2+0x14;

	for (i = 0; i < 9; i++) {
		for (j = 0; j < 0x10; j++) {
			p[(i << 4) + j] = pti->key[j];
		}

		p[(i << 4)] = i;
	}	

	if (Scramble((u32 *)tmp2, 0x90, pti->code) < 0) {
		printfLogged("error in Scramble#1, ");
		return -5;
	}

	memcpy(outbuf, tmp1+0xD0, 0x5C);
	memcpy(outbuf+0x5C, tmp1+0x140, 0x10);
	memcpy(outbuf+0x6C, tmp1+0x12C, 0x14);
	memcpy(outbuf+0x80, tmp1+0x080, 0x30);
	memcpy(outbuf+0xB0, tmp1+0x0C0, 0x10);
	memcpy(outbuf+0xC0, tmp1+0x0B0, 0x10);
	memcpy(outbuf+0xD0, tmp1+0x000, 0x80);

	memcpy(tmp3+0x14, outbuf+0x5C, 0x60);	

	if (Scramble((u32 *)tmp3, 0x60, pti->code) < 0) {
		printfLogged("error in Scramble#2, ");
		return -6;
	}

	memcpy(outbuf+0x5C, tmp3, 0x60);
	memcpy(tmp3, outbuf+0x6C, 0x14);
	memcpy(outbuf+0x70, outbuf+0x5C, 0x10);
	memset(outbuf+0x18, 0, 0x58);
	memcpy(outbuf+0x04, outbuf, 0x04);

	*((u32 *)outbuf) = 0x014C;
	memcpy(outbuf+0x08, tmp2, 0x10);

	/* sha-1 */
	if (sceUtilsBufferCopyWithRange_(outbuf, 3000000, outbuf, 3000000, 0x0B) != 0) {
		printfLogged("error in sceUtilsBufferCopyWithRange 0xB, ");
		return -7;
	}	

	if (memcmp(outbuf, tmp3, 0x14) != 0) {
		printfLogged("WARNING (SHA-1 incorrect), ");
		//return -8;
	}
	
	int iXOR;

	for (iXOR = 0; iXOR < 0x40; iXOR++) {
		tmp3[iXOR+0x14] = outbuf[iXOR+0x80] ^ tmp2[iXOR+0x10];
	}

	if (Scramble((u32 *)tmp3, 0x40, pti->code) != 0) {
		printfLogged("error in Scramble#3, ");
		return -9;
	}
	
	for (iXOR = 0x3F; iXOR >= 0; iXOR--) {
		outbuf[iXOR+0x40] = tmp3[iXOR] ^ tmp2[iXOR+0x50]; // uns 8
	}

	memset(outbuf+0x80, 0, 0x30);
	*(u32 *)&outbuf[0xA0] = 1;

	memcpy(outbuf+0xB0, outbuf+0xC0, 0x10);
	memset(outbuf+0xC0, 0, 0x10);

	// the real decryption
	int ret = sceUtilsBufferCopyWithRange_(outbuf, size, outbuf+0x40, size-0x40, 0x1);
	if (ret != 0) {
		printfLogged("error in sceUtilsBufferCopyWithRange 0x1 (0x%08X), ", ret);
		return -1;
	}

	if (retsize < 0x150) {
		// Fill with 0
		memset(outbuf+retsize, 0, 0x150-retsize);		
	}

	return retsize;
}

int pspDecryptPRX(u8 *inbuf, u8 *outbuf, u32 size) {
	int retsize = DecryptPRX1(inbuf, outbuf, size, *(u32 *)&inbuf[0xD0]);

	if (retsize <= 0) {
		retsize = DecryptPRX2(inbuf, outbuf, size, *(u32 *)&inbuf[0xD0]);
	}

	return retsize;
}

void ExtractReboot(char *loadexec, int type) {
	// type = 1 old reboot.bin
	// type = 2 new reboot02g.bin

	// needs rewriting to handle 03g, psp go etc...

	char *rebootname = (type == 1 ? "reboot.bin" : "reboot_02g.bin");
	char *loadexecname = (type == 1 ? "loadexec_reboot.prx" : "loadexec_reboot_02g.prx");
	char rebootpath[255];
	sprintf(rebootpath, "%s%s", DEST, rebootname);

	int size = ReadFile(loadexec, 0, g_dataOut, sizeof(g_dataOut));
	printfLogged("Opened %s.\n", loadexecname);
	if (size > 0) {
		int i;
		for (i = 0; i < size-0xD0; i++) {
			if (!memcmp(g_dataOut+i, "~PSP", 4)) {
				printfLogged("Found %s, decrypting into buffer... ", rebootname);
				int pspsize, decsize, savesize;
				u32 tag;
				u8  *psave;
				
				pspsize = *(u32 *)&g_dataOut[i+0x2C];
				tag = *(u32 *)&g_dataOut[i+0xD0];
				
				decsize = pspDecryptPRX(g_dataOut+i, g_dataOut2, pspsize);

				if (decsize > 0) {
					printfLogged("done.\n");
					psave = g_dataOut2;
					savesize = decsize;

					if (*(u16*)&g_dataOut2[0] == 0x8B1F) {
						printfLogged("Extracting and decompressing %s...\n", rebootname);
						printfLogged("Decompressing GZ... ");

						decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));

						if (decsize > 0) {
							psave = g_dataOut;
							savesize = decsize;
						}
						printfLogged("done.\n");
					}

					else if (!memcmp(g_dataOut2, "2RLZ", 4)) {
						printfLogged("Extracting and decompressing %s...\n", rebootname);
						printfLogged("Decompressing RLZ... ");

						if (!rlzdecompressor) {
							printfLogged("RLZ decompressor unavailable, not decompressing...\n");
						}

						else {
							decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));
							if (decsize > 0) {
								psave = g_dataOut;
								savesize = decsize;
								printfLogged("done.\n");
							}
							else {
								printfLogged("failed: 0x%08X.\n", decsize);
							}
						}
					}

					else if (!memcmp(g_dataOut2, "KL3E", 4)) {
						printfLogged("Extracting and decompressing %s...\n", rebootname);
						printfLogged("Decompressing KL3E... ");

						if (!kl3edecompressor) {
							printfLogged("KL3E decompressor unavailable, not decompressing...\n");
						}

						else { 
							decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));
							if (decsize > 0) {
								psave = g_dataOut;
								savesize = decsize;
								printfLogged("done.\n");
							}
							else {
								printfLogged("failed: 0x%08X.\n", decsize);
							}
						}
					}

					if (!SaveFile(rebootpath, psave, savesize)) {
						printfLogged("Failed to save");
						return;
					}
					printfLogged("Saved");
					return;
				}

				else {
					printfLogged("Cannot decrypt %s.", rebootname);
				}
				break;
			}
		}
	}
	else {
		printfLogged("Error reading %s.", loadexecname);
	}
}

// sigcheck keys
u8 check_keys0[0x10] = {
	0x71, 0xF6, 0xA8, 0x31, 0x1E, 0xE0, 0xFF, 0x1E,
	0x50, 0xBA, 0x6C, 0xD2, 0x98, 0x2D, 0xD6, 0x2D
}; 

u8 check_keys1[0x10] = {
	0xAA, 0x85, 0x4D, 0xB0, 0xFF, 0xCA, 0x47, 0xEB,
	0x38, 0x7F, 0xD7, 0xE4, 0x3D, 0x62, 0xB0, 0x10
};

int DecryptSC(u32 *buf, int size) {
	buf[0] = 5;
	buf[1] = buf[2] = 0;
	buf[3] = 0x100;
	buf[4] = size;

	if (sceUtilsBufferCopyWithRange_(buf, size+0x14, buf, size+0x14, 8) < 0) {
		return -1;
	}

	return 0;
}

int EncryptSC(u32 *buf, int size) {
	buf[0] = 4;
	buf[1] = buf[2] = 0;
	buf[3] = 0x100;
	buf[4] = size;
 
	if (sceUtilsBufferCopyWithRange_(buf, size+0x14, buf, size+0x14, 5) < 0) {
		return -1;
	}
 
	return 0;
}

int UnsignCheck(u8 *buf) {
	u8 enc[0xD0+0x14];
	int iXOR, res;

	memcpy(enc+0x14, buf+0x80, 0xD0);

	for (iXOR = 0; iXOR < 0xD0; iXOR++) {
		enc[iXOR+0x14] ^= check_keys1[iXOR&0xF]; 
	}

	if ((res = DecryptSC((u32 *)enc, 0xD0)) < 0) {
		return res;
	}

	for (iXOR = 0; iXOR < 0xD0; iXOR++) {
		enc[iXOR] ^= check_keys0[iXOR&0xF];
	}

	memcpy(buf+0x80, enc+0x40, 0x90);
	memcpy(buf+0x110, enc, 0x40);

	return 0;
}

int ResignCheck(u8 *buf) {
	u8 enc[0xD0+0x14];
	int iXOR, res;
 
	memcpy(enc+0x14, buf+0x110, 0x40);
	memcpy(enc+0x14+0x40, buf+0x80, 0x90);
 
	for (iXOR = 0; iXOR < 0xD0; iXOR++) {
		enc[0x14+iXOR] ^= check_keys0[iXOR&0xF];
	}
 
	if ((res = EncryptSC((u32 *)enc, 0xD0)) < 0) {
		return res;
	}
 
	for (iXOR = 0; iXOR < 0xD0; iXOR++) {
		enc[0x14+iXOR] ^= check_keys1[iXOR&0xF];
	}
 
	memcpy(buf+0x80, enc+0x14, 0xD0);

	return 0;
}

// check 0xD4 to 0x12B inclusive. if any of it is not 0x00, the file is sigchecked
int IsSignChecked(u8 *buf) {
	int i;
	for (i = 0; i < 0x58; i++) {
		if (buf[0xD4+i] != 0) return 1;
	}
	return 0;
}

bool isSpecialPrx (u32 magic) {
	if (magic == 0x11724A68) return true;
	else return false;
}

void new_dir(char *dir_name) {
	char new_name[512];
	int d, err, i;

	d = sceIoDopen(dir_name);

	if (d >= 0) {
		/* directory already exists, try to rename it */
		sceIoDclose(d);

		for (i=0; i<10000; i++) {
			sprintf(new_name, "%s_%04d", dir_name, i);
			d = sceIoDopen(new_name);
			if (d < 0) break; /* directory with this name doesn't exist */
			sceIoDclose(d);
		}
		if (i == 10000) printfLogged("\n! unable to rename folder %s !\n", dir_name);
		else {
			err = sceIoRename(dir_name, new_name);
			if (err < 0) printfLogged("\n! error renaming: %X !\n", err);
			else printfLogged("\n! renamed folder %s to %s !\n", dir_name, new_name);
		}
	}
	sceIoMkdir(dir_name, 0777);
}

/*  // code to decrypt lfatfs_updater and nand_updater from the 6.00+ DATA.PSP 
    // need to integrate it to the module extractor below
    // while maintaining the compatibility with the old method of obfuscation, simple xor

void decryptXor(u8 *buf, int bufsize, u8 a, u8 b, u8 c)
{
    u8 x = a;
    u8 y = b;
    u8 z = c;
    u8 temp;
    
    int i;

    for(i=0; i<bufsize; i++)
    {
        x += y;
        buf[i] ^= x;
        
        temp = y%(z+1);
        y = z;
        z = x;
        x = temp;
    }
}

void decryptHiddenModule1(u8 *buf, int bufsize)
{
    decryptXor(buf, bufsize, 0x0F, 0x01, 0x0D);
}

void decryptHiddenModule2(u8 *buf, int bufsize)
{
    decryptXor(buf, bufsize, 0x07, 0x05, 0x13);
}

*/

void SaveOutput (char *fileout, u8 *psave, int savesize) {
	if (!savesize) {
		printfLogged("nothing to save.");
		return;
	}

   	SceIoStat stat;
	int screenX, screenY;

   	memset(&stat, 0, sizeof(SceIoStat)); 
   	sceIoGetstat(fileout, &stat);

	if (stat.st_attr & 0x1) {
		if (overwriteall) {
   			stat.st_attr &= ~0xFF;
			sceIoChstat(fileout, &stat, 6);
			printfLogged("changed file attribs, ");
		}
		else {
			screenX = pspDebugScreenGetX();
			screenY = pspDebugScreenGetY();
			printf("\n                  File is read-only. Overwrite?\n");
			printf("                %c - Yes  %c - Yes to All  O - No\n", 0xD8, 0xC8);

			unsigned int b;
			b = wait_press(PSP_CTRL_TRIANGLE|PSP_CTRL_SQUARE|PSP_CTRL_CIRCLE);
			wait_release(PSP_CTRL_TRIANGLE|PSP_CTRL_SQUARE|PSP_CTRL_CIRCLE);

			setc(screenX, screenY);
			printf("\n                                                             \n");
			printf("                                                               \n");
			setc(screenX, screenY);

			if ((b & PSP_CTRL_TRIANGLE) || (b & PSP_CTRL_SQUARE)) {
				stat.st_attr &= ~0xFF;
				sceIoChstat(fileout, &stat, 6);
				printfLogged("changed file attribs, ");
				if (b & PSP_CTRL_SQUARE) {
					overwriteall = 1;
				}
			}
		}
	}

	if (!SaveFile(fileout, psave, savesize)) printfLogged("save failed.");
	else printfLogged("saved.");

	if (is_updater) {
		is_updater = false;
		//printfLogged(" copying data.psp to secondary buffer...");
		//memcpy(g_dataOut, psave, savesize);
		//printfLogged(" done\n");
		printfLogged(" extracting updater modules...\n");
		new_dir("ms0:/enc/updaterprx");

		int i;
		int numextracted = 0;
		int key;
		int xorkey = 0;
		//int xor2count = 0;

		screenX = pspDebugScreenGetX();
		screenY = pspDebugScreenGetY();

		for (key = 0; key <= 0xFF; key += 0x01) {
			xorkey = (key * 0x1000000) + (key * 0x10000) + (key * 0x100) + key;
			setc(screenX, screenY);
			pspDebugScreenSetTextColor(COLOR_BLUE);
			printf("                                                          0x%08X", xorkey);
			pspDebugScreenSetTextColor(COLOR_WHITE);

			//search:
			for (i = 0; i < savesize; i+=4) {
				// look for "~PSP"
				if ((((*(u32 *)(psave+i))^xorkey) == 0x5053507E) || ((!key) && (*(u32*)(psave+i) == 0x11724A68))) {
					numextracted++;
					int decode;
					u32 modsize = 0;
					char modname[28];

					// decode and copy name to buffer
					if (isSpecialPrx(*(u32*)(psave+i))) {
						strcpy(modname, "sceLoadExecUpdater");
					}
					else {
						for (decode = 0; decode < 28; decode++) {
							modname[decode] = ((psave[i+0xA+decode])^xorkey);
						}
					}

					if (!key) printfLogged("--> %s @ 0x%08X .. ", modname, i);
					else printfLogged("--> %s @ 0x%08X/XOR 0x%02X .. ", modname, i, key);
					char fileoutpath[255];
					sprintf(fileoutpath, "%s%s%s", "ms0:/enc/updaterprx/", modname, ".prx");

					// decode output size
					if (isSpecialPrx(*(u32*)(psave+i))) {
						modsize = (0x150 + *(u32 *)(psave+i+0xB0));
					}
					else modsize = ((*(u32 *)(psave+i+0x2C))^xorkey);

					// decode the whole buffer directly
					if (key > 0) {
						for (decode = 0; decode < modsize; decode+=4) {
							*(u32*)(psave+i+decode) ^= xorkey;
						}
					}

					if (!SaveFile(fileoutpath, psave+i, modsize)) printfLogged("failed to save\n");
					else printfLogged("saved (%i bytes)\n", modsize);

					screenX = pspDebugScreenGetX();
					screenY = pspDebugScreenGetY();
				}
			}
			/*if (key == 0) { 
				if (xor2count == 0) {
					memcpy(psave, g_dataOut, savesize);
					decryptHiddenModule1(psave, savesize);
					xor2count++;
					goto search;
				}
				else if (xor2count == 1) {
					memcpy(psave, g_dataOut, savesize);
					decryptHiddenModule1(psave, savesize);
					xor2count++;
					goto search;
				}
			}*/
		}
		printfLogged("\n%i updater modules found", numextracted);
		if (numextracted > 0) { 
			printfLogged(" and extracted");
			updatermods = 1;
		}
	}
	return;
}

void AnalyzeSinglePRX(char *filein) {
	int size = ReadFile(filein, 0, g_dataOut, sizeof(g_dataOut));
	if (size > 0) {
		printfLogged("insize %iKB, ", (size/1024));
		if (!memcmp(g_dataOut+1, "ELF", 3)) {
			printfLogged("decrypted (ELF), ");
		}
		else if (*(u16 *)&g_dataOut[0] == 0x8B1F) {
			printfLogged("compressed (GZIP), ");
		}
		else if (!memcmp(g_dataOut+1, "RLZ", 3)) {
			printfLogged("compressed (RLZ), ");
		}
		else if (!memcmp(g_dataOut, "KL4E", 4)) {
			printfLogged("compressed (KL4E), ");
		}
		else if (!memcmp(g_dataOut, "KL3E", 4)) {
			printfLogged("compressed (KL3E), ");
		}
		else if (!memcmp(g_dataOut, "~PSP", 4)) {
			printfLogged("encrypted (~PSP)");

			// get tag
			u32 tag = *(u32 *)&g_dataOut[0xD0];
			bool tag_found = ((GetTagInfo(tag)) || (GetTagInfo2(tag)) ? true : false);

			char modname[28];
			int a;
			for (a = 0; a < 28; a++) {
				modname[a] = (g_dataOut[0xA+a]);
			}

			if (!tag_found) {
				if (IsSignChecked(g_dataOut)) {
					u32 origtag = tag;
					UnsignCheck(g_dataOut);
					printfLogged(", signchecked, ");

					int a;
					for (a = 0; a < 28; a++) {
						modname[a] = (g_dataOut[0xA+a]);
					}

					tag = *(u32 *)&g_dataOut[0xD0];
					tag_found = ((GetTagInfo(tag)) || (GetTagInfo2(tag)) ? true : false);

					if (!tag_found) {
						printfLogged("unknown tag 0x%08X/0x%08X, ", tag, origtag);
					}
					else printfLogged("known tag 0x%08X, ", tag);
				}
				else {
					printfLogged(", unknown tag 0x%08X, ", tag);
				}
			}
			else printfLogged(", known tag 0x%08X, ", tag);

			printfLogged("modname (%s), ", modname);

			int pspsize = *(u32 *)&g_dataOut[0x2C];
			if ((tag == 0x07000000) || (g_dataOut[0x04] > 0xA)) {
				pspsize = (size + *(u32 *)&g_dataOut[0xB0]);
				printfLogged("outsize ~%iKB, ", (pspsize/1024));
			}
			if ((pspsize > buffer_size) || (pspsize < 0)) {
				pspsize = (2 * (size + 100));
				printfLogged("outsize ~~%iKB, ", (pspsize/1024));
			}
			else {
				printfLogged("outsize %iKB, ", (pspsize/1024));
			}

			if (*(u16*)&g_dataOut[0x150] == 0x8B1F) {
				if ((*(u16*)&g_dataOut[0x1E] == 0x0000) && (*(u16*)&g_dataOut[0x3E] == 0x0000) && (*(u16*)&g_dataOut[0x58] == 0x0000)) {
					printfLogged("M33 GZ module, ");
				}
			}
			if ((!memcmp(g_dataOut+0xA, "updater", 7)) || (!memcmp(g_dataOut+0xA, "launcher", 7))) printfLogged("contains embedded modules, ");
		}
		else {
			printfLogged("unknown header (%c%c%c%c), ", g_dataOut[0], g_dataOut[1], g_dataOut[2], g_dataOut[3]);
		}
	}
	else {
		printfLogged("unable to open.");
	}
	printfLogged("done.");
	return;
}

void DecryptSinglePRX(char *filein, char *fileout) {
	int pspsize, decsize, savesize;
	u32 tag;
	u8 *psave;

	psave = 0;
	savesize = 0;
	pspsize = 0;
	tag = 0;
	decsize = 0;
	savesize = 0;
	is_updater = false;

	// read file into buffer
	int size = ReadFile(filein, 0, g_dataOut, sizeof(g_dataOut));
	if (size > 0) {	
		// get tag
		tag = *(u32 *)&g_dataOut[0xD0];
		
		// plain ELF or non-~PSP with zero tag		
		if ((!memcmp(g_dataOut+1, "ELF", 3)) || ((tag == 0x00000000) && (memcmp(g_dataOut, "~PSP", 4)))) {
			printfLogged("not encrypted. ");
			return;
		}

		// RLZ decompression
		else if ((!memcmp(g_dataOut+1, "RLZ", 3)) && (!unsign_only) && (!resign_only)) {
			if (!rlzdecompressor) {
				printfLogged("failed decomp. (RLZ unavailable).");
				return;
			}
			decsize = pspDecompress(g_dataOut, g_dataOut2, sizeof(g_dataOut2));
			if (decsize > 0) {
				printfLogged("decompressed (RLZ), ");
				psave = g_dataOut2;
				//savesize = decsize;
				SaveOutput(fileout, g_dataOut2, decsize);
				return;
			}
			else {
				printfLogged("failed decompress 0x%08X (RLZ).", decsize);
				return;
			}
		}
		// KL4E decompression
		else if ((!memcmp(g_dataOut, "KL4E", 4)) && (!unsign_only) && (!resign_only)) {
			if (!kl4edecompressor) {
				printfLogged("failed decomp. (KL4E unavailable).");
				return;
			}
			decsize = pspDecompress(g_dataOut, g_dataOut2, sizeof(g_dataOut2));
			if (decsize > 0) {
				printfLogged("decompressed (KL4E), ");
				psave = g_dataOut2;
				//savesize = decsize;
				SaveOutput(fileout, g_dataOut2, decsize);
				return;
			}
			else {
				printfLogged("failed decompress 0x%08X (KL4E).", decsize);
				return;
			}
		}
		else {
			// check for M33 prx (GZIP, FAKE HEADER)
			if ((*(u16*)&g_dataOut[0x150] == 0x8B1F) && (!unsign_only) && (!resign_only)) {
				if ((*(u16*)&g_dataOut[0x1E] == 0x0000) && (*(u16*)&g_dataOut[0x3E] == 0x0000) && (*(u16*)&g_dataOut[0x58] == 0x0000)) {
					// decompress embedded gzip
					decsize = pspDecompress(g_dataOut+0x150, g_dataOut2, sizeof(g_dataOut2));
					if (decsize > 0) {
						printfLogged("decompressed (M33 GZ), ");
						psave = g_dataOut2;
						//savesize = decsize;
						SaveOutput(fileout, g_dataOut2, decsize);
						return;
					}
					else {
						printfLogged("failed decompress 0x%08X (M33 GZ).", decsize);
						return;
					}
				}
			}

			if ((!memcmp(g_dataOut+0xA, "updater", 7)) || (!memcmp(g_dataOut+0xA, "launcher", 7))) is_updater = true;
			
			if ((!unsign_only) && (!resign_only)) { // get output size
				pspsize = *(u32 *)&g_dataOut[0x2C];

				// prx with mangled header -> use alternate size
				if ((tag == 0x07000000) || (g_dataOut[0x04] > 0xA)) {
					printfLogged("alternate output size, ");
					pspsize = (size + *(u32 *)&g_dataOut[0xB0]);
				}

				// unable to obtain size from header -> estimate output size based on gz compression
				if ((pspsize > buffer_size) || (pspsize < 0)) {
					printfLogged("estimating output size, ");
					pspsize = (2 * (size + 100));
				}
			}

			bool tag_found = ((GetTagInfo(tag)) || (GetTagInfo2(tag)) ? true : false);

			if (resign_only) {
				if (!IsSignChecked(g_dataOut)) {
					ResignCheck(g_dataOut);
					if (!IsSignChecked(g_dataOut)) {
						printfLogged("failed resigncheck.");
						return;
					}
					else {
						printfLogged("resignchecked, ");
						SaveOutput(fileout, g_dataOut, size);
						return;
					}
				}
				else {
					printfLogged("already signchecked.");
					return;
				}
			}
			
			if (unsign_only) {
				if ((IsSignChecked(g_dataOut)) && (!tag_found)) {
					UnsignCheck(g_dataOut);
					if (IsSignChecked(g_dataOut)) {
						printfLogged("failed unsigncheck.");
						return;
					}
					else {
						printfLogged("unsignchecked, ");
						SaveOutput(fileout, g_dataOut, size);
						return;
					}
				}
				else {
					printfLogged("not signchecked.");
					return;
				}
			}

			if (!tag_found) {
				if (IsSignChecked(g_dataOut)) {
					u32 origtag = tag;
					UnsignCheck(g_dataOut);
					printfLogged("unsignchecked, ");

					pspsize = *(u32 *)&g_dataOut[0x2C];
					// prx with mangled header -> use alternate size
					if ((tag == 0x07000000) || (g_dataOut[0x04] > 0xA)) pspsize = (size + *(u32 *)&g_dataOut[0xB0]);
					// unable to obtain size from header -> estimate output size based on gz compression
					if ((pspsize > buffer_size) || (pspsize < 0)) pspsize = (2 * (size + 100));

					tag = *(u32 *)&g_dataOut[0xD0];
					tag_found = ((GetTagInfo(tag)) || (GetTagInfo2(tag)) ? true : false);
					if (!tag_found) {
						printfLogged("failed (unk. tag 0x%08X).", origtag);
						return;
					}
				}
				else {
					printfLogged("failed (unk. tag 0x%08X).", tag);
					return;
				}
			}

			decsize = pspDecryptPRX(g_dataOut, g_dataOut2, pspsize);
			savesize = decsize;
			if (decsize < 0) {
				printfLogged("failed to decrypt (unknown).");
				return;
			}
			if (decsize > 0) {
				printfLogged("decrypted, ");
				// GZIP DECOMPRESSION
				if (*(u16 *)&g_dataOut2[0] == 0x8B1F) {
					decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));
					if (decsize > 0) {
						printfLogged("decompressed (GZ), ");
						SaveOutput(fileout, g_dataOut, decsize);
						return;
					}
					else {
						printfLogged("failed decomp. 0x%08X (GZ), ", decsize);
						SaveOutput(fileout, g_dataOut2, savesize);
						return;
					}
				}
				// RLZ DECOMPRESSION
				else if (!memcmp(g_dataOut2+1, "RLZ", 3)) {
					if (!rlzdecompressor) {
						printfLogged("saving as RLZ, ");
						SaveOutput(fileout, g_dataOut2, decsize);
						return;
					}
					decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));
					if (decsize > 0) {
						printfLogged("decompressed  (RLZ), ");
						SaveOutput(fileout, g_dataOut, decsize);
						return;
					}
					else {
						printfLogged("failed decomp. 0x%08X (RLZ), ", decsize);
						SaveOutput(fileout, g_dataOut2, savesize);
						return;
					}
				}
				// KL4E DECOMPRESSION
				else if (!memcmp(g_dataOut2, "KL4E", 4)) {
					if (!kl4edecompressor) {
						printfLogged("saving as KL4E, ");
						SaveOutput(fileout, g_dataOut2, decsize);
						return;
					}
					decsize = pspDecompress(g_dataOut2, g_dataOut, sizeof(g_dataOut));
					if (decsize > 0) {
						printfLogged("decompressed  (KL4E), ");
						SaveOutput(fileout, g_dataOut, decsize);
						return;
					}
					else {
						printfLogged("failed decomp. 0x%08X (KL4E), ", decsize);
						SaveOutput(fileout, g_dataOut2, savesize);
						return;
					}
				}

				SaveOutput(fileout, g_dataOut2, decsize);
				return;
			}
			return;
		}
	}
	// read 0 bytes
	printfLogged("unable to open.");
	return;
}

void errorexit(int errorcode) {
	pspDebugScreenSetTextColor(COLOR_RED);

	switch(errorcode)
	{
		case 1: printf("%s folder is missing", DEST); break;
		case 2: printf("Unable to initialize kernel functions"); break;
		case 3: printf("Failed to get list of files from %s", DEST); break;
		case 4:
			pspDebugScreenSetTextColor(COLOR_GREEN);
			printf("Exiting in 5 seconds...");
			sceKernelDelayThread(5 * 1000 * 1000);
			sceKernelExitGame();
			break;
		case 5:
			printf("\n\nExiting in 10 seconds...");
			sceKernelDelayThread(10 * 1000 * 1000);
			sceKernelExitGame();
			break;
		case 6: printf("Unknown kernel version"); break;
		default: printf("Unknown critical error");
	}

	printf("\n\nExiting in 10 seconds...");
	sceKernelDelayThread(10 * 1000 * 1000);
	sceKernelExitGame();
}

int main(void) {
	pspDebugScreenInit();
	pspDebugScreenSetBackColor(COLOR_BLACK);
	pspDebugScreenSetTextColor(COLOR_WHITE);
	pspDebugScreenClear();

	char loadexec150path[strlen(DEST)+21]; // loadexec_reboot.prx
	char loadexec360path[strlen(DEST)+25]; // loadexec_reboot_02g.prx

	char *rlzresult[] = { "OK", "\nFailed to buffer", "LoadModule failed", "Import failed", "\nFailed to find imports" };

	char sysmem270path[strlen(DEST)+16];

	bool notdecrypted = true;
	bool notunsignchecked = true;
	bool notresignchecked = true;

	bool analyze_only = false;

	char friendlyname2[255];

	bool rebootfiles = false;
	bool rebootfiles_02g = false;
	bool rebootnotextracted = true;
	bool reboot_02g_notextracted = true;

	#ifdef LOGGING_ON
	char logpath[strlen(DEST)+9];
	char logdata[255];
	#endif
	char inpath[256];

	// pad stuff
	SceCtrlData pad_data;
	sceCtrlSetSamplingCycle(0);
	sceCtrlSetSamplingMode(1);

	int y = 0;
	int z = 0;

	// intro
	pspDebugScreenSetTextColor(COLOR_GREEN);
	printf("PRXdecrypter %s \n-- jas0nuk (update by freeplay with thanks to coyotebean)\n\n", VERSION);
	pspDebugScreenSetTextColor(COLOR_WHITE);

	printf("Thanks M33, C+D, bbtgp, SilverSpring, PspPet, Mathieulh\n");
	printf("Greetings to Dark-AleX.org/LAN.st/Malloc/eXophase\n\n");

	// check ms0:/enc
	SceUID checkfolder = sceIoDopen(DEST);
	if (checkfolder < 0) errorexit(1);
	sceIoDclose(checkfolder);

	#ifdef LOGGING_ON
	sprintf(logpath, "%s%s", DEST, "log.txt");
	logfile = my_openlogfile(logpath);
	if (logfile < 0) {
		printf("Unable to create logfile, disabled logs...\n\n");
		logging_enabled = false;
	}
	else {
		logging_enabled = true;
	}
	sprintf(logdata, "PRXdecrypter %s started, found %s folder\nLogging started at %s...\n\n", VERSION, DEST, logpath);
	my_print(logfile, logdata);
	logdata[0] = '\0';
	#endif

	char outpath[256];
	sprintf(outpath, "%s", DEST);

	kernelver = sceKernelDevkitVersion();
	SceUID kmod;
	//SceUID npmod = 0;

	if (kernelver == 0x01050001) {
		kmod = pspSdkLoadStartModule("prxdecrypter_01g.prx", PSP_MEMORY_PARTITION_KERNEL);
		if (kmod < 0) {
			printfLogged("Error 0x%08X loading/starting PRXdecrypter01g module\n", kmod);
			my_close(logfile);
			errorexit(5);
		}
		else printfLogged("Successfully loaded PRXdecrypter01g module\n");
	}
	else {
		kmod = pspSdkLoadStartModule("prxdecrypter_02g.prx", PSP_MEMORY_PARTITION_KERNEL);
		if (kmod < 0) {
			printfLogged("Error 0x%08X loading/starting PRXdecrypter02g module\n", kmod);
			my_close(logfile);
			errorexit(5);
		}
		else printfLogged("Successfully loaded PRXdecrypter02g module\n\n");

		/*
		shouldnt be needed any more

		npmod = pspSdkLoadStartModule("flash0:/kd/np9660.prx" , PSP_MEMORY_PARTITION_KERNEL);
		if (npmod < 0) {
			printfLogged("Error 0x%08X loading/starting sceNp9660_driver\n", npmod);
		}
		else printfLogged("Successfully loaded sceNp9660_driver\n\n");*/
	}

	if (kernelver == 0x01050001) {
		sprintf(sysmem270path, "%s%s", DEST, "sysmem_rlz.prx");
		SceUID sysmem270 = sceIoOpen(sysmem270path, PSP_O_RDONLY, 0777);
		if (sysmem270 < 0) rlzdecompressor = false;
		else {
			rlzdecompressor = true;
			sceIoClose(sysmem270);
		}
		printfLogged("Setting up decryption and GZ decompression -> ");
		if (!InitSysEntries()) {
			my_print(logfile, "Failed\nExiting...\n");
			my_close(logfile);
			errorexit(2);
		}
		printfLogged("OK\n");
		if (rlzdecompressor) {
			printfLogged("Setting up RLZ decompression -> ");
			int rlz = SetupRLZ(sysmem270path);
			if (rlz < 0) printfLogged("%s 0x%08X\n", rlzresult[2], rlz);
			else printfLogged("%s\n", rlzresult[rlz]);
			if (rlz != 0) {
				rlzdecompressor = false;
			}
		}
		kl4edecompressor = false;
		kl3edecompressor = false;
		printf("\n");
	}
	else {
		if (sceKernelDevkitVersion() < 0x03080000) {
			rlzdecompressor = true;
			kl4edecompressor = false;
			kl3edecompressor = false;
		}
		else { 
			//if (npmod < 0) {
			//	rlzdecompressor = false;
			//}
			//else {
				rlzdecompressor = true;
			//}
			kl4edecompressor = true;
			kl3edecompressor = true;
		}
	}

	printfLogged("RLZ decompression ");
	if (!rlzdecompressor) {
		pspDebugScreenSetTextColor(COLOR_RED);
		printfLogged("unavailable\n");
	}
	else {
		pspDebugScreenSetTextColor(COLOR_GREEN);
		printfLogged("available\n");
	}
	pspDebugScreenSetTextColor(COLOR_WHITE);
	printfLogged("KL3E decompression ");
	if (!kl3edecompressor) {
		pspDebugScreenSetTextColor(COLOR_RED);
		printfLogged("unavailable\n");
	}
	else {
		pspDebugScreenSetTextColor(COLOR_GREEN);
		printfLogged("available\n");
	}
	pspDebugScreenSetTextColor(COLOR_WHITE);
	printfLogged("KL4E decompression ");
	if (!kl4edecompressor) {
		pspDebugScreenSetTextColor(COLOR_RED);
		printfLogged("unavailable\n");
	}
	else {
		pspDebugScreenSetTextColor(COLOR_GREEN);
		printfLogged("available\n");
	}
	pspDebugScreenSetTextColor(COLOR_WHITE);
	printfLogged("\n");

	sprintf(loadexec150path, "%s%s", DEST, "loadexec_reboot.prx");
	sprintf(loadexec360path, "%s%s", DEST, "loadexec_reboot_02g.prx");
	SceUID loadexec150 = sceIoOpen(loadexec150path, PSP_O_RDONLY, 0777);
	SceUID loadexec360 = sceIoOpen(loadexec360path, PSP_O_RDONLY, 0777);

	if (loadexec150 >= 0) {
		printfLogged("Extraction of reboot.bin enabled\n");
		sceIoClose(loadexec150);
		rebootfiles = true;
	}
	if (loadexec360 >= 0) {
		printfLogged("Extraction of reboot_02g.bin enabled\n");
		sceIoClose(loadexec360);
		rebootfiles_02g = true;
	}
	printfLogged("\n");

	// SHOCK HORROR - NEXT SECTION CONTAINS A FEW INSTANCES OF "GOTO"!
	// i was too lazy to rewrite it since the menu was added as a last minute thing and just works
	// its totally solid though and wont ever get stuck in an infinite loop (probably).

	restartapp:
	printfLogged("* Going to main menu...");
	sceKernelDelayThread(3 * 1000 * 1000);
	pspDebugScreenClear();
	pspDebugScreenSetTextColor(COLOR_GREEN);
	printf("PRXdecrypter %s \n-- jas0nuk (update by freeplay with thanks to coyotebean)\n\n", VERSION);
	pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("Press D-Pad up/down to select an option, then press X to\nexecute the selected option\n\n");
	y = 0;
	pspDebugScreenSetTextColor(COLOR_GREY);

	if (notdecrypted) pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Decrypt/decompress files\n");
	y++;
	//pspDebugScreenSetTextColor(COLOR_GREY);

	pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Analyze files\n");
	y++;
	pspDebugScreenSetTextColor(COLOR_GREY);

	if (notdecrypted && notunsignchecked) pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Unsigncheck files\n");
	y++;
	pspDebugScreenSetTextColor(COLOR_GREY);

	if (notdecrypted && notresignchecked) pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Signcheck files\n");
	y++;
	pspDebugScreenSetTextColor(COLOR_GREY);

	if (rebootfiles && rebootnotextracted) pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Extract reboot.bin\n");
	y++;
	pspDebugScreenSetTextColor(COLOR_GREY);

	if (rebootfiles_02g && reboot_02g_notextracted) pspDebugScreenSetTextColor(COLOR_WHITE);
	printf("   Extract reboot_02g.bin\n");
	y++;
	pspDebugScreenSetTextColor(COLOR_WHITE);

	printf("   Switch output folder between %s and %s\n", DEST, OUT);
	y++;
	printf("   Exit\n");
	y++;

	printfLogged("\n");

	row = 0;
	rowprev = 0;
	maxrows = (y - 1);
	if ((row == 0) && (!notdecrypted)) row++;
	if ((row == 2) && (!notunsignchecked || !notdecrypted)) row++;
	if ((row == 3) && (!notresignchecked || !notdecrypted)) row++;
	if ((row == 4) && (!rebootfiles || !rebootnotextracted)) row++;
	if ((row == 5) && (!rebootfiles_02g || !reboot_02g_notextracted)) row++;
	refreshArrow();

	while(1) {
		unsigned int b;
		b = wait_press(PSP_CTRL_UP|PSP_CTRL_DOWN|PSP_CTRL_CROSS);
		wait_release(PSP_CTRL_UP|PSP_CTRL_DOWN|PSP_CTRL_CROSS);

		if (b & PSP_CTRL_DOWN) {
			rowprev = row;
			// go back to top
			if (row == maxrows) row = 0;
			else row++;

			if ((row == 0) && (!notdecrypted)) row++;
			if ((row == 2) && (!notunsignchecked || !notdecrypted)) row++;
			if ((row == 3) && (!notresignchecked || !notdecrypted)) row++;
			if ((row == 4) && (!rebootfiles || !rebootnotextracted)) row++;
			if ((row == 5) && (!rebootfiles_02g || !reboot_02g_notextracted)) row++;

			refreshArrow();
		}
		else if (b & PSP_CTRL_UP) {
			rowprev = row;
			// go back to top
			if (row == 0) row = maxrows;
			else row--;

			if ((row == 5) && (!rebootfiles_02g || !reboot_02g_notextracted)) row--;
			if ((row == 4) && (!rebootfiles || !rebootnotextracted)) row--;
			if ((row == 3) && (!notresignchecked || !notdecrypted)) row--;
			if ((row == 2) && (!notunsignchecked || !notdecrypted)) row--;
			if ((row == 0) && (!notdecrypted)) row = maxrows;

			refreshArrow();
		}
		else if (b & PSP_CTRL_CROSS) {
			setc(0, (maxrows + row_offset + 2));

			if (row == maxrows) {
				my_print(logfile, "Exited PRXdecrypter\n");
				my_close(logfile);
				errorexit(4);
			}
			else if (row == (maxrows - 1)) {
				if (!strcmp(outpath, "ms0:/enc/")) {
					outpath[0] = '\0';
					sprintf(outpath, "%s", OUT);
				}
				else {
					outpath[0] = '\0';
					sprintf(outpath, "%s", DEST);
				}
				printfLogged("* Output folder is now %s\n\n", outpath);
				goto restartapp;
			}

			if (row == 0) {
				notdecrypted = false;
				notunsignchecked = false;
				notresignchecked = false;
				clearscreen();
				printfLogged("Decrypting files...\n\n");
				break;
			}
			else if (row == 1) {
				analyze_only = true;
				clearscreen();
				printfLogged("Analyzing files...\n\n");
				break;
			}
			else if (row == 2) {
				notunsignchecked = false;
				notresignchecked = true;
				unsign_only = true;
				clearscreen();
				printfLogged("Unsignchecking files...\n\n");
				break;
			}
			else if (row == 3) {
				notunsignchecked = true;
				notresignchecked = false;
				resign_only = true;
				clearscreen();
				printfLogged("Signchecking files...\n\n");
				break;
			}
			else if (row == 4) {
				rebootnotextracted = false;
				clearscreen();
				printfLogged("Extracting reboot.bin from loadexec_reboot.prx...\n");
				ExtractReboot(loadexec150path, 1);
				printfLogged("\n\n");
				sceKernelDelayThread(3 * 1000 * 1000);
				goto restartapp;
			}
			else if (row == 5) {
				reboot_02g_notextracted = false;
				clearscreen();
				printfLogged("Extracting reboot_02g.bin from loadexec_reboot_02g.prx...\n");
				ExtractReboot(loadexec360path, 2);
				printfLogged("\n\n");
				sceKernelDelayThread(3 * 1000 * 1000);
				goto restartapp;
			}
		}
	}

	char savepath[256];
	if (!strcmp(outpath, "ms0:/dec/")) {
		checkfolder = sceIoDopen("ms0:/dec/");
		if (checkfolder < 0) sceIoMkdir("ms0:/dec", 0777);
		else sceIoDclose(checkfolder);
	}

	strcpy(inpath, DEST);

	decryptall:
	z = 0;
	if (!(fileGetList(inpath, NULL, FILE_LIST_FILE | FILE_LIST_RECURSIVE, &filelist))) {
		if (filelist.count == 0) {
			printfLogged("No files found in %s.\n\n", inpath);
			goto restartapp;
		}

		while (z < filelist.count) {
			// remove "ms0:/enc/" from the path
			strcpy(friendlyname2, (filelist.list[z] + strlen(inpath)));

			if (!strcmp(friendlyname2, "log.txt")) goto nextfile;

			savepath[0] = '\0';
			sprintf(savepath, "%s%s", outpath, friendlyname2);

			printfLogged("* %s -> ", friendlyname2);
			if (analyze_only) AnalyzeSinglePRX(filelist.list[z]);
			else DecryptSinglePRX(filelist.list[z], savepath);
			printfLogged("\n");

			// move to next file
			nextfile:
			z++;
		}

		// Free the list
		fileFreeList(&filelist);
		if (analyze_only) { 
			printfLogged("\nPress O to return.\n\n");
			while(1) {
				sceCtrlReadBufferPositive(&pad_data, 1);
				if (pad_data.Buttons & PSP_CTRL_CIRCLE) break;
			}
		}
		else printfLogged("\nFinished.\n\n");

		if (updatermods) {
			updatermods = 0;
			clearscreen();
			printf("Decrypt updater modules which were just extracted?\n");
			printf("                %c - Yes   O - No\n\n", 0xD8);

			unsigned int b;
			b = wait_press(PSP_CTRL_TRIANGLE|PSP_CTRL_CIRCLE);
			wait_release(PSP_CTRL_TRIANGLE|PSP_CTRL_CIRCLE);

			if (b & PSP_CTRL_TRIANGLE) {
				inpath[0] = '\0';
				strcpy(inpath, "ms0:/enc/updaterprx/");

				if (!strcmp(outpath, "ms0:/dec/")) {
					checkfolder = sceIoDopen("ms0:/dec/updaterprx/");
					if (checkfolder < 0) sceIoMkdir("ms0:/dec/updaterprx", 0777);
					else sceIoDclose(checkfolder);

					outpath[0] = '\0';
					strcpy(outpath, "ms0:/dec/updaterprx/");
				}
				else {
					outpath[0] = '\0';
					strcpy(outpath, "ms0:/enc/updaterprx/");
				}
				clearscreen();
				printfLogged("Decompressing updater modules...\n\n");

				goto decryptall;
			}
			else if (b & PSP_CTRL_CIRCLE) {
				printfLogged("Not decompressing updater modules.\n\n");
			}
		}

	}
	else {
		printfLogged("Unable to list files in %s.\n\n", inpath);
	}
	unsign_only = false;
	resign_only = false;
	analyze_only = false;
	sceKernelDelayThread(2 * 1000 * 1000);
	goto restartapp;

	// never reaches here
	while(1) {
		sceCtrlReadBufferPositive(&pad_data, 1);
		if (pad_data.Buttons & PSP_CTRL_TRIANGLE) break;
	}
	
	my_print(logfile, "Exiting...\n");
	my_close(logfile);

	errorexit(4);
	return 0;
}
