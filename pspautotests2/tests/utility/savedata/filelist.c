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
	strcpy(param.saveName, "");

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
	param.mode = (PspUtilitySavedataMode) SCE_UTILITY_SAVEDATA_TYPE_FILES;
	param.saveNameList = saveNameList;
	// NEW / OTHER same?
	strcpy(param.fileName, "NEW.BIN");

	SceUtilitySavedataFileListInfo fileList;
	memset(&fileList, 0, sizeof(fileList));
	SceUtilitySavedataFileListEntry fileListNormal[10];
	memset(fileListNormal, 0, sizeof(fileListNormal));
	SceUtilitySavedataFileListEntry fileListSecure[10];
	memset(fileListSecure, 0, sizeof(fileListSecure));
	SceUtilitySavedataFileListEntry fileListSystem[10];
	memset(fileListSystem, 0, sizeof(fileListSystem));
	fileList.maxNormalEntries = 10;
	fileList.maxSecureEntries = 10;
	fileList.maxSystemEntries = 10;
	fileList.normalEntries = fileListNormal;
	fileList.secureEntries = fileListSecure;
	fileList.systemEntries = fileListSystem;
	param.fileList = &fileList;

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
