#include <common.h>

#include <pspkernel.h>

float sceDisplayGetFramePerSec(void);

int main(int argc, char *argv[]) {
	printf("Started\n");
	
	printf("sceDisplayGetFramePerSec: %f\n", sceDisplayGetFramePerSec());

	return 0;
}
