.set noreorder

#include "pspimport.s"

IMPORT_START "sceDisplay",0x40010000
IMPORT_FUNC "sceDisplay",0x0E20F177,sceDisplaySetMode
IMPORT_FUNC "sceDisplay",0xDEA197D4,sceDisplayGetMode
IMPORT_FUNC "sceDisplay",0xDBA6C4C4,sceDisplayGetFramePerSec
IMPORT_FUNC "sceDisplay",0x7ED59BC4,sceDisplaySetHoldMode
IMPORT_FUNC "sceDisplay",0xA544C486,sceDisplaySetResumeMode
IMPORT_FUNC "sceDisplay",0xBF79F646,sceDisplayGetResumeMode
IMPORT_FUNC "sceDisplay",0x289D82FE,sceDisplaySetFrameBuf
IMPORT_FUNC "sceDisplay",0xEEDA2E54,sceDisplayGetFrameBuf
IMPORT_FUNC "sceDisplay",0x31C4BAA8,sceDisplayGetBrightness
IMPORT_FUNC "sceDisplay",0x9C6EAAD7,sceDisplayGetVcount
IMPORT_FUNC "sceDisplay",0xB4F378FA,sceDisplayIsForeground
IMPORT_FUNC "sceDisplay",0x4D4E10EC,sceDisplayIsVblank
IMPORT_FUNC "sceDisplay",0x21038913,sceDisplayIsVsync
IMPORT_FUNC "sceDisplay",0x36CDFADE,sceDisplayWaitVblank
IMPORT_FUNC "sceDisplay",0x8EB9EC49,sceDisplayWaitVblankCB
IMPORT_FUNC "sceDisplay",0x984C27E7,sceDisplayWaitVblankStart
IMPORT_FUNC "sceDisplay",0x46F186C3,sceDisplayWaitVblankStartCB
IMPORT_FUNC "sceDisplay",0x40f1469c,sceDisplayWaitVblankStartMulti
IMPORT_FUNC "sceDisplay",0x77ed8b3a,sceDisplayWaitVblankStartMultiCB
IMPORT_FUNC "sceDisplay",0x773DD3A3,sceDisplayGetCurrentHcount
IMPORT_FUNC "sceDisplay",0x210EAB3A,sceDisplayGetAccumulatedHcount
IMPORT_FUNC "sceDisplay",0xA83EF139,sceDisplayAdjustAccumulatedHcount
IMPORT_FUNC "sceDisplay",0x69B53541,sceDisplayGetVblankRest