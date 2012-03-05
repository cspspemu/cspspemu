	.set noreorder

#include "pspimport.s"

        IMPORT_START "sceSystemMemoryManager",0x00010000
	# gz decompress
	IMPORT_FUNC "UtilsForKernel",0x78934841,sceKernelGzipDecompress
	
