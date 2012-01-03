#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psppower.h>

/**
 * Power Callback Function Definition
 *
 * @param unknown   - unknown function, appears to cycle between 1,2 and 3
 * @param powerInfo - combination of PSP_POWER_CB_ flags
 */
int powerHandler(int unknown, int powerInfo, void *arg) {
	printf("powerHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)powerInfo, (uint)arg);

	return 0;
}

int main(int argc, char **argv) {
	int powerCbCallbackId;
	int powerCbSlot;
	int result;
	
	powerCbCallbackId = sceKernelCreateCallback("powerHandler", powerHandler, (void *)0x1234);
	printf("%d\n", powerCbCallbackId != 0);
	
	powerCbSlot = scePowerRegisterCallback(-1, powerCbCallbackId);
	printf("%d\n", powerCbSlot);
	
	sceKernelCheckCallback();
	
	result = scePowerUnregisterCallback(powerCbSlot);
	printf("%08X\n", result);

	result = scePowerUnregisterCallback(powerCbSlot);
	printf("%08X\n", result);
	
	return 0;
}