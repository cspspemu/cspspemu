.set noreorder

#include "pspimport.s"

IMPORT_START "semaphore",0x40010000
IMPORT_FUNC  "semaphore",0x4C537C72,sceUtilsBufferCopyWithRange
