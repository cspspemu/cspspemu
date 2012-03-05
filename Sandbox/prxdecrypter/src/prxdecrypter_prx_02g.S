	.set noreorder

#include "pspstub.s"

	STUB_START "prxdecrypter_prx_02g",0x40090000,0x00020005
	STUB_FUNC  0xA9DF755D,sceUtilsBufferCopyWithRange_02g
	STUB_FUNC  0x4DC709A0,pspDecompress_02g
	STUB_END
