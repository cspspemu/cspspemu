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

	// First, let's create some savedata.
	param.mode = (PspUtilitySavedataMode) SCE_UTILITY_SAVEDATA_TYPE_MAKEDATASECURE;
	param.saveNameList = saveNameList;

	param.dataBuf = savedata;
	param.dataBufSize = sizeof(savedata);
	param.dataSize = sizeof(savedata);

	checkpointExists(&param);
	runStandardSavedataLoop(&param);

	// A bit more, non-secure mode.
	param.mode = (PspUtilitySavedataMode) SCE_UTILITY_SAVEDATA_TYPE_WRITEDATA;
	param.saveNameList = saveNameList;
	strcpy(param.fileName, "OTHER.BIN");

	param.dataBuf = savedata;
	param.dataBufSize = sizeof(savedata);
	param.dataSize = sizeof(savedata);

	checkpointExists(&param);
	runStandardSavedataLoop(&param);

	// Now the actual sizes call.
	param.mode = (PspUtilitySavedataMode) SCE_UTILITY_SAVEDATA_TYPE_GETSIZE;
	param.saveNameList = saveNameList;
	// NEW / OTHER same?
	strcpy(param.fileName, "NEW.BIN");

	SceUtilitySavedataSizeInfo sizeInfo;
	memset(&sizeInfo, 0, sizeof(sizeInfo));
	param.sizeInfo = &sizeInfo;

	param.dataBuf = savedata;
	param.dataBufSize = sizeof(savedata);
	param.dataSize = sizeof(savedata);

	checkpointExists(&param);
	runStandardSavedataLoop(&param);

	checkpoint("Result: %08x", param.base.result);

	checkpointExists(&param);

	// TODO: Better way of cleaning up...
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901ABC/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901ABC/OTHER.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901ABC/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901ABC"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901/OTHER.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901F1/DATA.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901F1/OTHER.BIN"));
	checkpoint("sceIoRemove: %08x", sceIoRemove("ms0:/PSP/SAVEDATA/TEST99901F1/PARAM.SFO"));
	checkpoint("sceIoRmdir: %08x", sceIoRmdir("ms0:/PSP/SAVEDATA/TEST99901F1"));

	return 0;
}
