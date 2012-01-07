#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>

int umdHandler(int unknown, int info, void *arg) {
	printf("umdHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)info, (uint)arg);

	return 0;
}

int main(int argc, char **argv) {
	int umdCbCallbackId;
	int result;
	
	umdCbCallbackId = sceKernelCreateCallback("umdHandler", umdHandler, (void *)0x1234);
	
	printf("sceUmdRegisterUMDCallBack: %08X\n", result = sceUmdRegisterUMDCallBack(umdCbCallbackId));
	printf("sceUmdActivate: %08X\n", sceUmdActivate(1, "disc0:"));
	printf("sceKernelCheckCallback: %d\n", sceKernelCheckCallback());
	
	printf("sceUmdUnRegisterUMDCallBack: %08X\n", result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId));
	printf("sceUmdUnRegisterUMDCallBack: %08X\n", result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId));

	return 0;
}