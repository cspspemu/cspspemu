#include <common.h>

#include <pspkernel.h>
#include <psprtc.h>

static __inline__ int MScmRegisterMSInsertEjectCallback(SceUID cbid)
{
	return sceIoDevctl("fatms0:", 0x02415821, &cbid, sizeof(cbid), 0, 0);
}

static __inline__ int MScmUnregisterMSInsertEjectCallback(SceUID cbid)
{
	return sceIoDevctl("fatms0:", 0x02415822, &cbid, sizeof(cbid), 0, 0);
}

int ms_callback(int arg1, int arg2, void *arg)
{
	printf("ms_callback: %08X, %08X, %08X\n", (unsigned int)arg1, (unsigned int)arg2, (unsigned int)arg);
    return 0;
}

int main(int argc, char **argv) {
    SceUID cb_id;
 
    cb_id = sceKernelCreateCallback("MSCB", ms_callback, (void *)0x777);
	printf("cb_id: %08X\n", (cb_id > 0) ? 1 : 0);
    printf("MScmRegisterMSInsertEjectCallback: %08X\n", MScmRegisterMSInsertEjectCallback(cb_id));
    printf("sceKernelCheckCallback: %08X\n", sceKernelCheckCallback());
    printf("MScmUnregisterMSInsertEjectCallback: %08X\n", MScmUnregisterMSInsertEjectCallback(cb_id));
	printf("sceKernelDeleteCallback: %08X\n", sceKernelDeleteCallback(cb_id));
	
    return 0;
}
