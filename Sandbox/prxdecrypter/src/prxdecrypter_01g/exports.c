#include <pspmoduleexport.h>
#define NULL ((void *) 0)

void extern module_start;
void extern module_info;
static const unsigned int __syslib_exports[4] __attribute__((section(".rodata.sceResident"))) = {
	0xD632ACDB,
	0xF01D73A7,
	(unsigned int) &module_start,
	(unsigned int) &module_info,
};

void extern InitSysEntries;
void extern SetupRLZ;
void extern sceUtilsBufferCopyWithRange_01g;
void extern pspDecompress_01g;
static const unsigned int __prxdecrypter_prx_01g_exports[8] __attribute__((section(".rodata.sceResident"))) = {
	0xCE5490BF,
	0x03F48266,
	0xFC9CA564,
	0x2FF35722,
	(unsigned int) &InitSysEntries,
	(unsigned int) &SetupRLZ,
	(unsigned int) &sceUtilsBufferCopyWithRange_01g,
	(unsigned int) &pspDecompress_01g,
};

const struct _PspLibraryEntry __library_exports[2] __attribute__((section(".lib.ent"), used)) = {
	{ NULL, 0x0000, 0x8000, 4, 1, 1, &__syslib_exports },
	{ "prxdecrypter_prx_01g", 0x0000, 0x4001, 4, 0, 4, &__prxdecrypter_prx_01g_exports },
};
