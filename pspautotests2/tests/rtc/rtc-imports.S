.set noreorder

#include "pspimport.s"

IMPORT_START "sceRtc",0x40010000

IMPORT_FUNC "sceRtc",0xC663B3B9,sceRtcFormatRFC2822
IMPORT_FUNC "sceRtc",0x7DE6711B,sceRtcFormatRFC2822LocalTime
IMPORT_FUNC "sceRtc",0x0498FB3C,sceRtcFormatRFC3339
IMPORT_FUNC "sceRtc",0x27F98543,sceRtcFormatRFC3339LocalTime
IMPORT_FUNC "sceRtc",0xDFBC5F16,sceRtcParseDateTime
IMPORT_FUNC "sceRtc",0x28E1E988,sceRtcParseRFC3339
IMPORT_FUNC "sceRtc",0xe1c93e47,sceRtcGetTime64_t
IMPORT_FUNC "sceRtc",0x1909c99b,sceRtcSetTime64_t

