#include "shared.h"

static SceUtilitySavedataSaveName saveNameList[] = {
	"F1",
	"M2",
	"L3",
	"\0",
};

static char savedata[] = {
	1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6
};

int main(int argc, char **argv) {
	initDisplay();

	SceUtilitySavedataParam2 param;
	initStandardSavedataParams(&param);

	param.mode = (PspUtilitySavedataMode) SCE_UTILITY_SAVEDATA_TYPE_MAKEDATA;
	param.saveNameList = saveNameList;

	param.dataBuf = savedata;
	param.dataBufSize = sizeof(savedata);
	param.dataSize = sizeof(savedata);

	// We check what file was created, so make sure it doesn't already exist...
	checkpointExists(&param);

	runStandardSavedataLoop(&param);

	checkpoint("Result: %08x", param.base.result);

	checkpointExists(&param);

	// TODO: Better way of cleaning up...
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901ABC/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901ABC/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901ABC"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901F1/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901F1/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901F1"));

	return 0;
}
