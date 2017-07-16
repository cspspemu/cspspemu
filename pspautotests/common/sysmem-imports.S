.set noreorder

#include "pspimport.s"

IMPORT_START "SysMemUserForUser",0x40000000

IMPORT_FUNC  "SysMemUserForUser",0xA291F107,sceKernelMaxFreeMemSize
IMPORT_FUNC  "SysMemUserForUser",0xF919F628,sceKernelTotalFreeMemSize
IMPORT_FUNC  "SysMemUserForUser",0x2A3E5280,sceKernelQueryMemoryInfo

IMPORT_FUNC  "SysMemUserForUser",0x3FC9AE6A,sceKernelDevkitVersion
IMPORT_FUNC  "SysMemUserForUser",0x13A5ABEF,sceKernelPrintf
IMPORT_FUNC  "SysMemUserForUser",0x39F49610,sceKernelGetPTRIG
IMPORT_FUNC  "SysMemUserForUser",0x6231A71D,sceKernelSetPTRIG

IMPORT_FUNC  "SysMemUserForUser",0x237DBD4F,sceKernelAllocPartitionMemory
IMPORT_FUNC  "SysMemUserForUser",0xB6D61D02,sceKernelFreePartitionMemory
IMPORT_FUNC  "SysMemUserForUser",0x9D9A5BA1,sceKernelGetBlockHeadAddr

IMPORT_FUNC  "SysMemUserForUser",0xDB83A952,sceKernelGetMemoryBlockPtr
IMPORT_FUNC  "SysMemUserForUser",0xFE707FDF,sceKernelAllocMemoryBlock
IMPORT_FUNC  "SysMemUserForUser",0x50F61D8A,sceKernelFreeMemoryBlock

IMPORT_FUNC  "SysMemUserForUser",0xF77D77CB,sceKernelSetCompilerVersion
IMPORT_FUNC  "SysMemUserForUser",0xFC114573,sceKernelGetCompiledSdkVersion
IMPORT_FUNC  "SysMemUserForUser",0x7591C7DB,sceKernelSetCompiledSdkVersion
IMPORT_FUNC  "SysMemUserForUser",0x342061E5,sceKernelSetCompiledSdkVersion370
IMPORT_FUNC  "SysMemUserForUser",0x315AD3A0,sceKernelSetCompiledSdkVersion380_390
IMPORT_FUNC  "SysMemUserForUser",0xEBD5C3E6,sceKernelSetCompiledSdkVersion395
IMPORT_FUNC  "SysMemUserForUser",0x057E7380,sceKernelSetCompiledSdkVersion401_402
IMPORT_FUNC  "SysMemUserForUser",0x91DE343C,sceKernelSetCompiledSdkVersion500_505
IMPORT_FUNC  "SysMemUserForUser",0x7893F79A,sceKernelSetCompiledSdkVersion507
IMPORT_FUNC  "SysMemUserForUser",0x35669D4C,sceKernelSetCompiledSdkVersion600_602
IMPORT_FUNC  "SysMemUserForUser",0x1B4217BC,sceKernelSetCompiledSdkVersion603_605
IMPORT_FUNC  "SysMemUserForUser",0x358CA1BB,sceKernelSetCompiledSdkVersion606

# Not sure if correct.
IMPORT_FUNC  "SysMemUserForUser",0xA6848DF8,sceKernelSetUsersystemLibWork
IMPORT_FUNC  "SysMemUserForUser",0xACBD88CA,sceKernelTotalMemSize
IMPORT_FUNC  "SysMemUserForUser",0xD8DE5C1E,SysMemUserForUser_D8DE5C1E
IMPORT_FUNC  "SysMemUserForUser",0x945E45DA,SysMemUserForUser_945E45DA