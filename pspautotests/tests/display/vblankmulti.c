#include <common.h>
#include <pspgu.h>
#include <psprtc.h>
#include <pspdisplay.h>

int sceDisplayWaitVblankStartMulti(int vblanks);
int sceDisplayWaitVblankStartMultiCB(int vblanks);
int sceDisplayIsVblank();
int sceDisplayIsVsync();

void testWait(const char *title, int (*wait)()) {
	// Wait until just a bit after a vblank.
	sceDisplayWaitVblankStart();
	sceKernelDelayThread(750);
	int vbase = sceDisplayGetVcount();
	
	checkpointNext(title);
	int i;
	char temp[256];
	for (i = 0; i < 4; ++i) {
		sprintf(temp, "i=%-4d vcount=%-4d ", i, sceDisplayGetVcount() - vbase);

		int result = wait();
		checkpoint("%swait=%08x  vblank=%d", temp, result, sceDisplayIsVblank());
	}
}

void testWaitMulti(const char *title, int (*multi)(int), int vblanks) {
	// Wait until just a bit after a vblank.
	sceDisplayWaitVblankStart();
	sceKernelDelayThread(750);
	int vbase = sceDisplayGetVcount();

	checkpointNext(title);
	int i;
	char temp[256];
	for (i = 0; i < 4; ++i) {
		sprintf(temp, "i=%-4d vcount=%-4d ", i, sceDisplayGetVcount() - vbase);

		int result = multi(vblanks);
		checkpoint("%swait=%08x  vblank=%d", temp, result, sceDisplayIsVblank());
	}
}

int main(int argc, char *argv[]) {
	testWait("Start:", &sceDisplayWaitVblankStart);
	testWait("StartCB:", &sceDisplayWaitVblankStartCB);
	testWait("Vblank:", &sceDisplayWaitVblank);
	testWait("VblankCB:", &sceDisplayWaitVblankCB);
	testWaitMulti("Multi:", &sceDisplayWaitVblankStartMultiCB, 3);
	testWaitMulti("MultiCB:", &sceDisplayWaitVblankStartMulti, 3);
	testWaitMulti("Multi:", &sceDisplayWaitVblankStartMultiCB, 0);
	testWaitMulti("MultiCB:", &sceDisplayWaitVblankStartMulti, 0);
	testWaitMulti("Multi:", &sceDisplayWaitVblankStartMultiCB, -1);
	testWaitMulti("MultiCB:", &sceDisplayWaitVblankStartMulti, -1);
	testWaitMulti("Multi:", &sceDisplayWaitVblankStartMultiCB, 0x80000000);
	testWaitMulti("MultiCB:", &sceDisplayWaitVblankStartMulti, 0x80000000);

	return 0;
}