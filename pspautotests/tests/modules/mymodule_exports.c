#include <pspmoduleexport.h>
#define NULL ((void *) 0)

extern void module_start;
extern void module_info;
static const unsigned int __syslib_exports[4] __attribute__((section(".rodata.sceResident"))) = {
	0xD632ACDB,
	0xF01D73A7,
	(unsigned int) &module_start,
	(unsigned int) &module_info,
};

extern void getModuleInfo;
extern void getConst1;
extern void getConst2;
static const unsigned int __MyLib_exports[6] __attribute__((section(".rodata.sceResident"))) = {
	0x563FF2B2,
	0xC046DFDB,
	0xCEDF91CD,
	(unsigned int) &getModuleInfo,
	(unsigned int) &getConst1,
	(unsigned int) &getConst2,
};

const struct _PspLibraryEntry __library_exports[2] __attribute__((section(".lib.ent"), used)) = {
	{ NULL, 0x0000, 0x8000, 4, 1, 1, &__syslib_exports },
	{ "MyLib", 0x0000, 0x0001, 4, 0, 3, &__MyLib_exports },
};
