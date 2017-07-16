#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>

int main(int argc, char **argv) {
	int result;
	int directory;
	SceIoDirent directoryEntry;
	
	directory = sceIoDopen("disc0:/PSP_GAME");
	do {
		result = sceIoDread(directory, &directoryEntry);
		printf("%d : '%s'\n", result, directoryEntry.d_name);
	} while(result > 0);

	return 0;
}