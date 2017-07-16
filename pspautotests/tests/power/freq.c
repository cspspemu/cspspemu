#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psppower.h>

extern int scePowerGetPllClockFrequencyInt();
extern float scePowerGetPllClockFrequencyFloat();

int powerHandler(int unknown, int powerInfo, void *arg) {
	schedf("powerHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)powerInfo & 0x00000080, (uint)arg);

	return 0;
}

void testCurrentClock() {
	sceKernelCheckCallback();

	int cpu1 = scePowerGetCpuClockFrequency();
	int cpu2 = scePowerGetCpuClockFrequencyInt();
	float cpuf = scePowerGetCpuClockFrequencyFloat();

	schedf("cpu: %d / %d / %f\n", cpu1, cpu2, cpuf);

	int bus1 = scePowerGetBusClockFrequency();
	int bus2 = scePowerGetBusClockFrequencyInt();
	float busf = scePowerGetBusClockFrequencyFloat();

	schedf("bus: %d / %d / %f\n", bus1, bus2, busf);

	int pll2 = scePowerGetPllClockFrequencyInt();
	float pllf = scePowerGetPllClockFrequencyFloat();

	schedf("pll: %d / %0.4f\n", pll2, pllf);
}

int main(int argc, char **argv) {
	int powerCbCallbackId = sceKernelCreateCallback("powerHandler", powerHandler, (void *)0x1234);
	scePowerRegisterCallback(-1, powerCbCallbackId);
	sceKernelCheckCallback();

	int freq;
	for (freq = -1; freq <= 223; ++freq)
	{
		int result = scePowerSetCpuClockFrequency(freq);
		schedf("scePowerSetCpuClockFrequency %d: %08x\n", freq, result);
		testCurrentClock();
	}

	scePowerSetCpuClockFrequency(222);
	flushschedf();

	for (freq = -1; freq <= 112; ++freq)
	{
		int result = scePowerSetBusClockFrequency(freq);
		schedf("scePowerSetBusClockFrequency %d: %08x\n", freq, result);
		testCurrentClock();
	}

	scePowerSetBusClockFrequency(111);
	flushschedf();

	for (freq = -1; freq <= 334; ++freq)
	{
		int result = scePowerSetClockFrequency(freq, freq, freq / 2);
		schedf("scePowerSetClockFrequency %d: %08x\n", freq, result);
		testCurrentClock();
	}

	scePowerSetClockFrequency(222, 222, 111);
	flushschedf();

	return 0;
}
