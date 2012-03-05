	.set noreorder

#include "pspstub.s"

	STUB_START "prxdecrypter_prx_01g",0x40090000,0x00040005
	STUB_FUNC  0xCE5490BF,InitSysEntries
	STUB_FUNC  0x03F48266,SetupRLZ
	STUB_FUNC  0xFC9CA564,sceUtilsBufferCopyWithRange_01g
	STUB_FUNC  0x2FF35722,pspDecompress_01g
	STUB_END
