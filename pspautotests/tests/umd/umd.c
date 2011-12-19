#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>

int umdHandler(int unknown, int info, void *arg) {
	printf("umdHandler called: %08X, %08X, %08X\n", unknown, info, (u32)arg);

	return 0;
}

int main(int argc, char **argv) {
	int umdCbCallbackId;
	int result;
	
	umdCbCallbackId = sceKernelCreateCallback("umdHandler", umdHandler, (void *)0x1234);
	
	result = sceUmdRegisterUMDCallBack(umdCbCallbackId);
	printf("%08X\n", result);
	
	printf("%d\n", sceKernelCheckCallback());
	
	result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId);
	printf("%08X\n", result);

	result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId);
	printf("%08X\n", result);

	return 0;
}