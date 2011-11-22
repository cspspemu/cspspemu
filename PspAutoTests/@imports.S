.set noreorder

#include "pspimport.s"

IMPORT_START "Emulator",0x40010000
IMPORT_FUNC	 "Emulator",0x00000000,emitInt
IMPORT_FUNC	 "Emulator",0x00000001,emitFloat
IMPORT_FUNC	 "Emulator",0x00000002,emitString
IMPORT_FUNC	 "Emulator",0x00000003,emitMemoryBlock
IMPORT_FUNC	 "Emulator",0x00000004,emitHex
IMPORT_FUNC	 "Emulator",0x00000005,emitUInt
