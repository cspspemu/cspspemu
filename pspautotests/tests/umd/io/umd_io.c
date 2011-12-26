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
	
	if ((directory = sceIoDopen("disc0:/PSP_GAME")) > 0) {
		do {
			result = sceIoDread(directory, &directoryEntry);
			printf("%d : '%s'\n", result, directoryEntry.d_name);
		} while(result > 0);
	} else {
		printf("Unknown path\n");
	}

	if ((directory = sceIoDopen("disc0:/unexistant_path")) > 0) {
		do {
			result = sceIoDread(directory, &directoryEntry);
			printf("%d : '%s'\n", result, directoryEntry.d_name);
		} while (result > 0);
	} else {
		printf("Unknown path\n");
	}

	printf("%08X\n", sceIoOpen("_unexistant_file.vag", PSP_O_RDONLY, 0777));
	printf("%08X\n", sceIoOpen("/_unexistant_file.vag", PSP_O_RDONLY, 0777));
	printf("%08X\n", sceIoOpen("disc0:/_unexistant_file.vag", PSP_O_RDONLY, 0777));
	printf("%08X\n", sceIoOpen("disc0:/path/to/unexistant/file.vag", PSP_O_RDONLY, 0777));

	return 0;
}