.set noreorder

#include "pspimport.s"

IMPORT_START "Kernel_Library",0x00010000
IMPORT_FUNC	"Kernel_Library",0x092968F4,sceKernelCpuSuspendIntr
IMPORT_FUNC	"Kernel_Library",0x5F10D406,sceKernelCpuResumeIntr
IMPORT_FUNC	"Kernel_Library",0x3B84732D,sceKernelCpuResumeIntrWithSync
IMPORT_FUNC	"Kernel_Library",0x47A0B729,sceKernelIsCpuIntrSuspended
IMPORT_FUNC	"Kernel_Library",0xB55249D2,sceKernelIsCpuIntrEnable
IMPORT_FUNC "Kernel_Library",0xA089ECA4,sce_paf_private_memset
IMPORT_FUNC "Kernel_Library",0x1839852A,sce_paf_private_memcpy
IMPORT_FUNC "Kernel_Library",0xDC692EE3,sceKernelTryLockLwMutex
IMPORT_FUNC "Kernel_Library",0x37431849,sceKernelTryLockLwMutex_600
IMPORT_FUNC "Kernel_Library",0xbea46419,sceKernelLockLwMutex
IMPORT_FUNC "Kernel_Library",0x1FC64E09,sceKernelLockLwMutexCB
IMPORT_FUNC "Kernel_Library",0x15b6446b,sceKernelUnlockLwMutex
IMPORT_FUNC "Kernel_Library",0x293b45b8,sceKernelGetThreadId