#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psppower.h>

static uint testResult;

#define TEST_NAMED_RES(name, func, args...) \
	testResult = func(args); \
	schedf("%s: %08X\n", name, testResult);

#define TEST_NAMED_NOTZERO(name, func, args...) \
	testResult = func(args); \
	schedf("%s: %s\n", name, testResult != 0 ? "OK" : "Failed");

#define TEST_RES(func, args...) TEST_NAMED_RES(#func, func, args)
#define TEST_NOTZERO(func, args...) TEST_NAMED_NOTZERO(#func, func, args)

/**
 * Power Callback Function Definition
 *
 * @param unknown   - unknown function, appears to cycle between 1,2 and 3
 * @param powerInfo - combination of PSP_POWER_CB_ flags
 */
int powerHandler(int unknown, int powerInfo, void *arg) {
	schedf("powerHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)powerInfo & 0x00000080, (uint)arg);

	return 0;
}

int powerHandler2(int unknown, int powerInfo, void *arg) {
	schedf("powerHandler2 called: %08X, %08X, %08X\n", (uint)unknown, (uint)powerInfo & 0x00000080, (uint)arg);

	return 0;
}

int main(int argc, char **argv) {
	int powerCbCallbackId = TEST_NOTZERO(sceKernelCreateCallback, "powerHandler", powerHandler, (void *)0x1234);
	int powerCbCallbackId2 = TEST_NOTZERO(sceKernelCreateCallback, "powerHandler2", powerHandler2, (void *)0x4567);

	int powerCbSlot1 = TEST_RES(scePowerRegisterCallback, -1, powerCbCallbackId);
	int powerCbSlot2 = TEST_RES(scePowerRegisterCallback, -1, powerCbCallbackId2);

	// Register the same callback in two slots, make sure it's only called once.
	int powerCbSlot3 = TEST_RES(scePowerRegisterCallback, -1, powerCbCallbackId2);

	TEST_RES(sceKernelCheckCallback);
	
	TEST_RES(scePowerUnregisterCallback, powerCbSlot1);
	TEST_NAMED_RES("scePowerUnregisterCallback twice", scePowerUnregisterCallback, powerCbSlot1);
	TEST_NAMED_RES("scePowerUnregisterCallback never registered", scePowerUnregisterCallback, 8);
	TEST_NAMED_RES("scePowerUnregisterCallback too low", scePowerUnregisterCallback, -4);
	TEST_NAMED_RES("scePowerUnregisterCallback too high (31)", scePowerUnregisterCallback, 31);
	TEST_NAMED_RES("scePowerUnregisterCallback too high (32)", scePowerUnregisterCallback, 32);

	TEST_RES(scePowerUnregisterCallback, powerCbSlot2);
	TEST_RES(scePowerUnregisterCallback, powerCbSlot3);

	checkpointNext("---");
	
	powerCbSlot1 = TEST_RES(scePowerRegisterCallback, -1, powerCbCallbackId);
	TEST_RES(sceKernelPowerTick, -1);
	TEST_RES(sceKernelCheckCallback);

	// Just testing scheduling.
	scePowerIsBatteryCharging();
	schedf("scePowerIsBatteryCharging\n");
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerIsBatteryExist);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerIsPowerOnline);
	TEST_RES(sceKernelCheckCallback);

	TEST_NOTZERO(scePowerGetBatteryLifePercent);
	TEST_RES(sceKernelCheckCallback);

	// Just testing scheduling.
	scePowerGetBatteryChargingStatus();
	schedf("scePowerGetBatteryChargingStatus\n");
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerIsLowBattery);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(sceKernelPowerLock, 0);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(sceKernelPowerUnlock, 0);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(sceKernelPowerUnlock, 0);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(sceKernelPowerLock, 1);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(sceKernelPowerTick, -1);
	TEST_RES(sceKernelCheckCallback);

	TEST_NAMED_RES("scePowerSetBusClockFrequency to 66", scePowerSetBusClockFrequency, 66);
	TEST_RES(sceKernelCheckCallback);

	TEST_NAMED_RES("scePowerSetBusClockFrequency to 111", scePowerSetBusClockFrequency, 111);
	TEST_RES(sceKernelCheckCallback);

	TEST_NAMED_RES("scePowerSetCpuClockFrequency to 111", scePowerSetCpuClockFrequency, 111);
	TEST_RES(sceKernelCheckCallback);

	TEST_NAMED_RES("scePowerSetCpuClockFrequency to 222", scePowerSetCpuClockFrequency, 222);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerUnregisterCallback, powerCbSlot1);

	checkpointNext("---");

	powerCbSlot1 = TEST_NAMED_RES("scePowerRegisterCallback: Normal", scePowerRegisterCallback, 0, powerCbCallbackId);
	TEST_NAMED_RES("scePowerRegisterCallback: Invalid CB", scePowerRegisterCallback, 0, 0);
	TEST_NAMED_RES("scePowerRegisterCallback: Too low slot", scePowerRegisterCallback, -4, powerCbCallbackId);
	TEST_NAMED_RES("scePowerRegisterCallback: Too high (31)", scePowerRegisterCallback, 31, powerCbCallbackId);
	TEST_NAMED_RES("scePowerRegisterCallback: Too high (32)", scePowerRegisterCallback, 32, powerCbCallbackId);
	TEST_NAMED_RES("scePowerRegisterCallback: Twice (same)", scePowerRegisterCallback, 0, powerCbCallbackId);
	TEST_NAMED_RES("scePowerRegisterCallback: Twice (different)", scePowerRegisterCallback, 0, powerCbCallbackId2);
	TEST_RES(scePowerUnregisterCallback, powerCbSlot1);

	TEST_RES(sceKernelCheckCallback);

	powerCbSlot1 = TEST_RES(scePowerRegisterCallback, 0, powerCbCallbackId);
	TEST_RES(scePowerUnregisterCallback, powerCbSlot1);
	TEST_RES(sceKernelDeleteCallback, powerCbCallbackId);

	TEST_RES(sceKernelCheckCallback);

	checkpointNext("---");

	int i;
	for (i = 0; i < 17; i++) {
		TEST_RES(scePowerRegisterCallback, -1, powerCbCallbackId2);
	}

	checkpointNext("---");

	return 0;
}
