/*
 * PSP Software Development Kit - http://www.pspdev.org
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE in PSPSDK root for details.
 *
 * main.c - Basic harness for loading prxes (for proving a point)
 *
 * Copyright (c) 2005 Marcus R. Brown <mrbrown@ocgnet.org>
 * Copyright (c) 2005 James Forshaw <tyranid@gmail.com>
 * Copyright (c) 2005 John Kelley <ps2dev@kelley.ca>
 *
 * $Id: main.c 1095 2005-09-27 21:02:16Z jim $
 */
#include <common.h>

#include <pspkernel.h>
#include <pspdebug.h>
#include <pspdisplay.h>
#include <pspsdk.h>
#include <string.h>

//PSP_MODULE_INFO("PRXLOADER", 0x1000, 1, 1);
/* Define the main thread's attribute value (optional) */
//PSP_MAIN_THREAD_ATTR(0);

/* Define printf, just to make typing easier */
//#define printf	pspDebugScreenPrintf
//#define printf	Kprintf

SceUID load_module(const char *path, int flags, int type)
{
	SceKernelLMOption option;
	SceUID mpid;

	/* If the type is 0, then load the module in the kernel partition, otherwise load it
	   in the user partition. */
	if (type == 0) {
		mpid = 1;
	} else {
		mpid = 2;
	}

	memset(&option, 0, sizeof(option));
	option.size = sizeof(option);
	option.mpidtext = mpid;
	option.mpiddata = mpid;
	option.position = 0;
	option.access = 1;

	return sceKernelLoadModule(path, flags, type > 0 ? &option : NULL);
}

/* Imported function */
void *getModuleInfo(void);

int main(void)
{
	SceUID modid;
	//SceUID semaid;
	SceModule *mod;
	int i;
	int ret;
	int fd;

	/*
	pspDebugScreenInit();

	// Install all our funky err thingamybobs
	pspDebugInstallKprintfHandler(NULL);
	pspDebugInstallErrorHandler(NULL);
	pspDebugInstallStdoutHandler(pspDebugScreenPrintData);
	pspSdkInstallNoPlainModuleCheckPatch();
	*/
	//SetupCallbacks();

	/* Start mymodule.prx and dump its information */
	printf("Start my module\n");
	//modid = load_module("ms0:/mymodule.prx", 0, 0);
	modid = load_module("mymodule.prx", 0, 0);
	printf("Module ID %08X\n", (modid != 0));
	mod = sceKernelFindModuleByUID(modid);
	printf("mod %p\n", mod);

	if (mod != NULL) {
		printf("Attr %04X, Version %x.%x\n", mod->attribute, mod->version[0], mod->version[1]);
		printf("Name %s\n", mod->modname);
		printf("Text %08X, Size %08X, Data Size %08X\n", mod->text_addr, mod->text_size, mod->data_size);
		printf("Entry Addr %08X\n", mod->entry_addr);
		printf("Stub %p %08X, Ent %p %08X\n", mod->stub_top, mod->stub_size, mod->ent_top, mod->ent_size);
		for(i = 0; i < mod->nsegment; i++)
		{
			printf("Segment[%d] %08X %08X\n", i, mod->segmentaddr[i], mod->segmentsize[i]);
		}
		
		//semaid = sceKernelCreateSema("wait_module", 0, 0, 1, NULL);
		{
			ret = sceKernelStartModule(modid, 0, NULL, &fd, NULL);
		}
		//sceKernelWaitSema(semaid, 1, NULL);

		/* Let's test the module's exports */
		printf("Module Info %p\n", getModuleInfo());
	}

	/* Let's bug out */
	sceKernelExitDeleteThread(0);

	return 0;
}
