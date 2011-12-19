#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <psprtc.h>

void testControllerTimmings() {
	u64 tick0, tick1;
	int n;
	SceCtrlData pad_data;
	SceCtrlLatch latch;
	
	// Test sceCtrlReadBufferPositive
	sceRtcGetCurrentTick(&tick0);
	for (n = 0; n < 5; n++) sceCtrlReadBufferPositive(&pad_data, 1);
	sceRtcGetCurrentTick(&tick1);
	printf("%d\n", (tick1 - tick0 > 5000));

	// Test sceCtrlReadLatch
	sceRtcGetCurrentTick(&tick0);
	for (n = 0; n < 5; n++) sceCtrlReadLatch(&latch);
	sceRtcGetCurrentTick(&tick1);
	printf("%d\n", (tick1 - tick0 > 5000));

	// Test sceCtrlPeekBufferPositive
	sceRtcGetCurrentTick(&tick0);
	for (n = 0; n < 5; n++) sceCtrlPeekBufferPositive(&pad_data, 1);
	sceRtcGetCurrentTick(&tick1);
	printf("%d\n", (tick1 - tick0 < 5000));

	// Test sceCtrlPeekLatch
	/*
	sceRtcGetCurrentTick(&tick0);
	for (n = 0; n < 5; n++) sceCtrlPeekLatch(&pad_data, 1);
	sceRtcGetCurrentTick(&tick1);
	printf("%d\n", (tick1 - tick0 < 5000));
	*/
}

int main(int argc, char *argv[]) {
	testControllerTimmings();

	return 0;
}