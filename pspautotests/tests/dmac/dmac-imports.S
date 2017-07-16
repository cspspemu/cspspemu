.set noreorder

#include "pspimport.s"

IMPORT_START "sceDmac",0x40010000
IMPORT_FUNC	 "sceDmac",0x617F3FE6,sceDmacMemcpy
IMPORT_FUNC	 "sceDmac",0xD97F94D8,sceDmacTryMemcpy
