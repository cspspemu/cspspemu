#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <psprtc.h>

void outputPadData(int result, SceCtrlData *data) {
	// The PSP doesn't natively support a right analog stick, but official emulators do.
	// Zero it out so it compares the same when in the center.
	if (data->Rsrv[0] == 0x80) {
		data->Rsrv[0] = 0;
	}
	if (data->Rsrv[1] == 0x80) {
		data->Rsrv[1] = 0;
	}

	if (data) {
		schedf("      * PAD: %08x  %08x L(%d, %d) R(%d, %d) %02x%02x%02x%02x\n", result, data->Buttons, data->Lx, data->Ly, data->Rsrv[0], data->Rsrv[1], data->Rsrv[2], data->Rsrv[3], data->Rsrv[4], data->Rsrv[5]);
	} else {
		schedf("      * PAD: %08x with bad pointer\n", result);
	}
}

void outputLatchData(int result, SceCtrlLatch *data) {
	if (data) {
		schedf("      * LATCH: %08x  make=%08x, break=%08x, press=%08x, release=%08x\n", result, data->uiMake, data->uiBreak, data->uiPress, data->uiRelease);
	} else {
		schedf("      * LATCH: %08x with bad pointer\n", result);
	}
}

void testSamplingMode(const char *title, int mode) {
	int result = sceCtrlSetSamplingMode(mode);
	int data;

	if (result >= 0) {
		int getResult = sceCtrlGetSamplingMode(&data);
		if (getResult == 0) {
			checkpoint("%s: OK (%08x), value=%d", title, result, data);
		} else {
			checkpoint("%s: Get failed (%08x), value=%d", title, getResult, data);
		}
	} else {
		int getResult = sceCtrlGetSamplingMode(&data);
		if (getResult == 0) {
			checkpoint("%s: Set failed (%08x), value = %d", title, result, data);
		} else {
			checkpoint("%s: Both failed (%08x, %08x)", title, result, getResult);
		}
	}
}

void testSamplingCycle(const char *title, int cycle) {
	int result = sceCtrlSetSamplingCycle(cycle);
	int data;

	if (result >= 0) {
		int getResult = sceCtrlGetSamplingCycle(&data);
		if (getResult == 0) {
			checkpoint("%s: OK (%08x), value=%d", title, result, data);
		} else {
			checkpoint("%s: Get failed (%08x), value=%d", title, getResult, data);
		}
	} else {
		int getResult = sceCtrlGetSamplingCycle(&data);
		if (getResult == 0) {
			checkpoint("%s: Set failed (%08x), value = %d", title, result, data);
		} else {
			checkpoint("%s: Both failed (%08x, %08x)", title, result, getResult);
		}
	}
}

int main(int argc, char *argv[]) {
	int result, data;
	SceCtrlData pad_data[128];
	SceCtrlLatch latch;

	checkpointNext("sceCtrlGetSamplingMode:");
	result = sceCtrlGetSamplingMode(&data);
	checkpoint("  Initial mode: %08x, value=%d", result, data);
	testSamplingMode("  Mode 0", 0);
	testSamplingMode("  Mode 1", 1);
	testSamplingMode("  Mode 2", 2);
	testSamplingMode("  Mode -1", -1);
	testSamplingMode("  Mode 0 again", 0);
	// Crashes.
	//checkpoint("  NULL ptr: %08x", sceCtrlGetSamplingMode(NULL));
	
	checkpointNext("sceCtrlGetSamplingCycle:");
	result = sceCtrlGetSamplingCycle(&data);
	checkpoint("  Initial cycle: %08x, value=%d", result, data);
	testSamplingCycle("  Cycle 1", 1);
	testSamplingCycle("  Cycle 20000", 20000);
	testSamplingCycle("  Cycle 0", 0);
	testSamplingCycle("  Cycle -1", -1);
	// Crashes.
	//checkpoint("  NULL ptr: %08x", sceCtrlGetSamplingCycle(NULL));

	checkpointNext("Buffers:");
	result = sceCtrlReadBufferPositive(&pad_data[0], 1);
	outputPadData(result, &pad_data[0]);
	checkpoint("  sceCtrlReadBufferPositive 1: %08x", result);

	result = sceCtrlReadBufferPositive(&pad_data[0], 64);
	outputPadData(result, &pad_data[0]);
	checkpoint("  sceCtrlReadBufferPositive 64: %08x", result);

	result = sceCtrlPeekBufferPositive(&pad_data[0], 64);
	outputPadData(result, &pad_data[0]);
	checkpoint("  sceCtrlPeekBufferPositive 64: %08x", result);

	result = sceCtrlReadBufferPositive(&pad_data[0], 96);
	outputPadData(result, &pad_data[0]);
	checkpoint("  sceCtrlReadBufferPositive 96: %08x", result);

	checkpointNext("Latch:");
	result = sceCtrlPeekLatch(&latch);
	// Result is # of reads, which won't match headless.
	result = result >= 1 ? 1 : 0;
	outputLatchData(result, &latch);
	checkpoint("  sceCtrlPeekLatch: %08x", result);

	result = sceCtrlReadLatch(&latch);
	// Result is # of reads, which won't match headless.
	result = result >= 1 ? 1 : 0;
	outputLatchData(result, &latch);
	checkpoint("  sceCtrlReadLatch: %08x", result);

	result = sceCtrlPeekLatch(&latch);
	outputLatchData(result, &latch);
	checkpoint("  sceCtrlPeekLatch: %08x", result);

	return 0;
}