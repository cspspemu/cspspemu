#include "shared.h"

unsigned int __attribute__((aligned(16))) list[262144];

static SceUtilitySavedataParam2 lastParam;
SceUtilitySavedataMsFreeInfo lastMsFree;
SceUtilitySavedataMsDataInfo lastMsData;
SceUtilitySavedataUsedDataInfo lastUtilityData;
SceUtilitySavedataIdListInfo lastIdList;
SceUtilitySavedataIdListEntry lastIdListEntries[100];
SceUtilitySavedataFileListInfo lastFileList;
SceUtilitySavedataFileListEntry lastFileListNormal[100];
SceUtilitySavedataFileListEntry lastFileListSecure[100];
SceUtilitySavedataFileListEntry lastFileListSystem[100];
SceUtilitySavedataSizeInfo lastSizeInfo;
SceUtilitySavedataSizeEntry lastSizeInfoNormal[100];
SceUtilitySavedataSizeEntry lastSizeInfoSecure[100];

void setLastSaveParam(SceUtilitySavedataParam2 *param) {
	memcpy(&lastParam, param, sizeof(lastParam));
	if (param->msFree != NULL) {
		memcpy(&lastMsFree, param->msFree, sizeof(lastMsFree));
	}
	if (param->msData != NULL) {
		memcpy(&lastMsData, param->msData, sizeof(lastMsData));
	}
	if (param->utilityData != NULL) {
		memcpy(&lastUtilityData, param->utilityData, sizeof(lastUtilityData));
	}
	if (param->idList != NULL) {
		memcpy(&lastIdList, param->idList, sizeof(lastIdList));
		if (param->idList->entries != NULL) {
			memcpy(&lastIdListEntries, param->idList->entries, sizeof(lastIdListEntries));
		}
	}
	if (param->fileList != NULL) {
		memcpy(&lastFileList, param->fileList, sizeof(lastFileList));
		if (param->fileList->normalEntries != NULL) {
			memcpy(&lastFileListNormal, param->fileList->normalEntries, sizeof(lastFileListNormal));
		}
		if (param->fileList->secureEntries != NULL) {
			memcpy(&lastFileListSecure, param->fileList->secureEntries, sizeof(lastFileListSecure));
		}
		if (param->fileList->systemEntries != NULL) {
			memcpy(&lastFileListSystem, param->fileList->systemEntries, sizeof(lastFileListSystem));
		}
	}
	if (param->sizeInfo != NULL) {
		memcpy(&lastSizeInfo, param->sizeInfo, sizeof(lastSizeInfo));
		if (param->sizeInfo->normalEntries != NULL) {
			memcpy(&lastSizeInfoNormal, param->sizeInfo->normalEntries, sizeof(lastSizeInfoNormal));
		}
		if (param->sizeInfo->secureEntries != NULL) {
			memcpy(&lastSizeInfoSecure, param->sizeInfo->secureEntries, sizeof(lastSizeInfoSecure));
		}
	}

	// TODO: Need to copy other ptr data...
}

void printSaveParamChanges(SceUtilitySavedataParam2 *param) {
#define CHECK_CHANGE_U32(var) \
	if (param->var != lastParam.var) { \
		schedf("CHANGE: %s: %08x => %08x\n", #var, (unsigned int) lastParam.var, (unsigned int) param->var); \
		lastParam.var = param->var; \
	}

#define CHECK_CHANGE_U64(var) \
	if (param->var != lastParam.var) { \
		schedf("CHANGE: %s: %016llx => %016llx\n", #var, (unsigned long long) lastParam.var, (unsigned long long) param->var); \
		lastParam.var = param->var; \
	}

#define CHECK_CHANGE_STR(var) \
	if (strcmp((char *) param->var, (char *) lastParam.var)) { \
		schedf("CHANGE: %s: %s => %s\n", #var, (char *) lastParam.var, (char *) param->var); \
		strcpy((char *) &lastParam.var, (char *) &param->var); \
	}

#define CHECK_CHANGE_STRN(var, n) \
	if (strcmp((char *) param->var, (char *) lastParam.var)) { \
		schedf("CHANGE: %s: %.*s => %.*s\n", #var, n, (char *) lastParam.var, n, (char *) param->var); \
		strncpy((char *) &lastParam.var, (char *) &param->var, n); \
	}

#define CHECK_CHANGE_U32_PTR(ptr, lastPtr, var) \
	if (param->ptr->var != lastPtr.var) { \
		schedf("CHANGE: %s: %08x => %08x\n", #var, (unsigned int) lastPtr.var, (unsigned int) param->ptr->var); \
		lastPtr.var = param->ptr->var; \
	}

#define CHECK_CHANGE_U64_PTR(ptr, lastPtr, var) \
	if (param->ptr->var != lastPtr.var) { \
		schedf("CHANGE: %s: %016llx => %016llx\n", #var, (unsigned long long) lastPtr.var, (unsigned long long) param->ptr->var); \
		lastPtr.var = param->ptr->var; \
	}

#define CHECK_CHANGE_STR_PTR(ptr, lastPtr, var) \
	if (strcmp((char *) param->ptr->var, (char *) lastPtr.var)) { \
		schedf("CHANGE: %s: %s => %s\n", #var, (char *) lastPtr.var, (char *) param->ptr->var); \
		strcpy((char *) &lastPtr.var, (char *) &param->ptr->var); \
	}

#define CHECK_CHANGE_STRN_PTR(ptr, lastPtr, var, n) \
	if (strcmp((char *) param->ptr->var, (char *) lastPtr.var)) { \
		schedf("CHANGE: %s: %.*s => %.*s\n", #var, n, (char *) lastPtr.var, n, (char *) param->ptr->var); \
		strncpy((char *) &lastPtr.var, (char *) &param->ptr->var, n); \
	}

#define CHECK_CHANGE_FILEDATA(var) \
	CHECK_CHANGE_U32(var.buf); \
	CHECK_CHANGE_U32(var.bufSize); \
	CHECK_CHANGE_U32(var.size); \
	CHECK_CHANGE_U32(var.unknown);


	CHECK_CHANGE_U32(base.size);
	CHECK_CHANGE_U32(base.language);
	CHECK_CHANGE_U32(base.buttonSwap);
	CHECK_CHANGE_U32(base.graphicsThread);
	CHECK_CHANGE_U32(base.accessThread);
	CHECK_CHANGE_U32(base.fontThread);
	CHECK_CHANGE_U32(base.soundThread);
	CHECK_CHANGE_U32(base.result);
	CHECK_CHANGE_U32(base.reserved[0]);
	CHECK_CHANGE_U32(base.reserved[1]);
	CHECK_CHANGE_U32(base.reserved[2]);
	CHECK_CHANGE_U32(base.reserved[3]);

	CHECK_CHANGE_U32(mode);
	CHECK_CHANGE_U32(bind);
	CHECK_CHANGE_U32(overwrite);
	CHECK_CHANGE_STR(gameName);
	CHECK_CHANGE_STR(reserved);
	CHECK_CHANGE_STR(saveName);
	CHECK_CHANGE_U32(saveNameList);
	// TODO
	if (param->saveNameList != NULL) {
		CHECK_CHANGE_STR(saveNameList);
	}
	CHECK_CHANGE_STR(fileName);
	CHECK_CHANGE_STR(reserved1);

	CHECK_CHANGE_U32(dataBuf);
	CHECK_CHANGE_U32(dataBufSize);
	CHECK_CHANGE_U32(dataSize);

	CHECK_CHANGE_STR(sfoParam.title);
	CHECK_CHANGE_STR(sfoParam.savedataTitle);
	CHECK_CHANGE_STR(sfoParam.detail);
	CHECK_CHANGE_U32(sfoParam.parentalLevel);
	CHECK_CHANGE_U32(sfoParam.unknown[0]);
	CHECK_CHANGE_U32(sfoParam.unknown[1]);
	CHECK_CHANGE_U32(sfoParam.unknown[2]);

	CHECK_CHANGE_FILEDATA(icon0FileData);
	CHECK_CHANGE_FILEDATA(icon1FileData);
	CHECK_CHANGE_FILEDATA(pic1FileData);
	CHECK_CHANGE_FILEDATA(snd0FileData);

	// TODO
	CHECK_CHANGE_U32(newData);

	CHECK_CHANGE_U32(focus);
	CHECK_CHANGE_U32(abortStatus);
	if (param->msFree != NULL) {
		CHECK_CHANGE_U32_PTR(msFree, lastMsFree, clusterSize);
		CHECK_CHANGE_U32_PTR(msFree, lastMsFree, freeClusters);
		CHECK_CHANGE_U32_PTR(msFree, lastMsFree, freeSpaceKB);
		CHECK_CHANGE_STRN_PTR(msFree, lastMsFree, freeSpaceStr, 8);
		CHECK_CHANGE_U32_PTR(msFree, lastMsFree, unknownSafetyPad);
	}
	if (param->msData != NULL) {
		CHECK_CHANGE_STRN_PTR(msData, lastMsData, gameName, 16);
		CHECK_CHANGE_STRN_PTR(msData, lastMsData, saveName, 20);
		CHECK_CHANGE_U32_PTR(msData, lastMsData, info.usedClusters);
		CHECK_CHANGE_U32_PTR(msData, lastMsData, info.usedSpaceKB);
		CHECK_CHANGE_STRN_PTR(msData, lastMsData, info.usedSpaceStr, 8);
		CHECK_CHANGE_U32_PTR(msData, lastMsData, info.usedSpace32KB);
		CHECK_CHANGE_STRN_PTR(msData, lastMsData, info.usedSpace32Str, 8);
		CHECK_CHANGE_U64_PTR(msData, lastMsData, info.unknownSafetyPad[0]);
		CHECK_CHANGE_U64_PTR(msData, lastMsData, info.unknownSafetyPad[1]);
		CHECK_CHANGE_U64_PTR(msData, lastMsData, info.unknownSafetyPad[2]);
		CHECK_CHANGE_U64_PTR(msData, lastMsData, info.unknownSafetyPad[3]);
		CHECK_CHANGE_U64_PTR(msData, lastMsData, info.unknownSafetyPad2);
	}
	if (param->utilityData != NULL) {
		CHECK_CHANGE_U32_PTR(utilityData, lastUtilityData, usedClusters);
		CHECK_CHANGE_U32_PTR(utilityData, lastUtilityData, usedSpaceKB);
		CHECK_CHANGE_STRN_PTR(utilityData, lastUtilityData, usedSpaceStr, 8);
		CHECK_CHANGE_U32_PTR(utilityData, lastUtilityData, usedSpace32KB);
		CHECK_CHANGE_STRN_PTR(utilityData, lastUtilityData, usedSpace32Str, 8);
		CHECK_CHANGE_U64_PTR(utilityData, lastUtilityData, unknownSafetyPad[0]);
		CHECK_CHANGE_U64_PTR(utilityData, lastUtilityData, unknownSafetyPad[1]);
		CHECK_CHANGE_U64_PTR(utilityData, lastUtilityData, unknownSafetyPad[2]);
		CHECK_CHANGE_U64_PTR(utilityData, lastUtilityData, unknownSafetyPad[3]);
		CHECK_CHANGE_U64_PTR(utilityData, lastUtilityData, unknownSafetyPad2);
	}

	// TODO: key

	CHECK_CHANGE_U32(secureVersion);
	CHECK_CHANGE_U32(multiStatus);

	if (param->idList != NULL) {
		CHECK_CHANGE_U32_PTR(idList, lastIdList, maxCount);
		CHECK_CHANGE_U32_PTR(idList, lastIdList, resultCount);

		if (param->idList->entries != NULL) {
			CHECK_CHANGE_U32_PTR(idList->entries, lastIdListEntries[0], st_mode);
			// TODO: st_ctime, etc.?
			CHECK_CHANGE_STRN_PTR(idList->entries, lastIdListEntries[0], name, 20);
		}
	}
	if (param->fileList != NULL) {
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, maxSecureEntries);
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, maxNormalEntries);
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, maxSystemEntries);
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, resultNumSecureEntries);
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, resultNumNormalEntries);
		CHECK_CHANGE_U32_PTR(fileList, lastFileList, resultNumSystemEntries);

		if (param->fileList->secureEntries != NULL) {
			CHECK_CHANGE_U32_PTR(fileList->secureEntries, lastFileListSecure[0], st_mode);
			// TODO: st_ctime, etc.?
			CHECK_CHANGE_STRN_PTR(fileList->secureEntries, lastFileListSecure[0], name, 16);
		}
		if (param->fileList->normalEntries != NULL) {
			CHECK_CHANGE_U32_PTR(fileList->normalEntries, lastFileListNormal[0], st_mode);
			// TODO: st_ctime, etc.?
			CHECK_CHANGE_STRN_PTR(fileList->normalEntries, lastFileListNormal[0], name, 16);
		}
		if (param->fileList->systemEntries != NULL) {
			CHECK_CHANGE_U32_PTR(fileList->systemEntries, lastFileListSystem[0], st_mode);
			// TODO: st_ctime, etc.?
			CHECK_CHANGE_STRN_PTR(fileList->systemEntries, lastFileListSystem[0], name, 16);
		}
	}
	if (param->sizeInfo != NULL) {
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, numSecureEntries);
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, numNormalEntries);

		if (param->sizeInfo->secureEntries != NULL) {
			CHECK_CHANGE_U64_PTR(sizeInfo->secureEntries, lastSizeInfoSecure[0], size);
			CHECK_CHANGE_STRN_PTR(sizeInfo->secureEntries, lastSizeInfoSecure[0], name, 16);
		}
		if (param->sizeInfo->normalEntries != NULL) {
			CHECK_CHANGE_U64_PTR(sizeInfo->normalEntries, lastSizeInfoNormal[0], size);
			CHECK_CHANGE_STRN_PTR(sizeInfo->normalEntries, lastSizeInfoNormal[0], name, 16);
		}

		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, sectorSize);
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, freeSectors);
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, freeKB);
		CHECK_CHANGE_STRN_PTR(sizeInfo, lastSizeInfo, freeString, 8);
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, neededKB);
		CHECK_CHANGE_STRN_PTR(sizeInfo, lastSizeInfo, neededString, 8);
		CHECK_CHANGE_U32_PTR(sizeInfo, lastSizeInfo, overwriteKB);
		CHECK_CHANGE_STRN_PTR(sizeInfo, lastSizeInfo, overwriteString, 8);
	}
	// TODO
	CHECK_CHANGE_U32(sizeInfo);
}

int checkpointStatusChange() {
	static int lastStatus = -1;
	int status = sceUtilitySavedataGetStatus();
	if (status != lastStatus || status < 0) {
		checkpoint("sceUtilitySavedataGetStatus: %08x (duplicates skipped)", status);
		lastStatus = status;
	}
	return status;
}

void initDisplay()
{
	sceGuInit();
	sceGuStart(GU_DIRECT, list);
	sceGuDrawBuffer(GU_PSM_8888, NULL, BUF_WIDTH);
	sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, NULL, BUF_WIDTH);
	sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuEnable(GU_SCISSOR_TEST);
	sceGuFinish();
	sceGuSync(0, 0);
 
	sceDisplaySetMode(0, SCR_WIDTH, SCR_HEIGHT);
	sceDisplayWaitVblankStart();
	sceGuDisplay(1);
}

void initStandardSavedataParams(SceUtilitySavedataParam2 *param) {
	memset(param, 0, sizeof(SceUtilitySavedataParam2));

	param->base.size = sizeof(SceUtilitySavedataParam2);
	param->base.language = 1;
	param->base.buttonSwap = 1;
	param->base.graphicsThread = 0x21;
	param->base.accessThread = 0x22;
	param->base.fontThread = 0x23;
	param->base.soundThread = 0x20;

#define SET_STR(str, val) \
	strncpy(param->str, val, sizeof(param->str) - 1);

	SET_STR(gameName, "TEST99901");
	SET_STR(saveName, "ABC");
	SET_STR(fileName, "DATA.BIN");

	SET_STR(sfoParam.title, "pspautotests");
	SET_STR(sfoParam.savedataTitle, "pspautotests test savedata");
	SET_STR(sfoParam.detail, "Created during an automated or manual test.");

	u32 temp_key[] = {0x12345678, 0x12345678, 0x12345678, 0x12345678};
	memcpy(&param->key[0], &temp_key[0], sizeof(param->key));
}

void checkpointExistsSaveName(const SceUtilitySavedataParam2 *param, const char *saveName) {
	char temp[512];
	snprintf(temp, sizeof(temp), "ms0:/PSP/SAVEDATA/%s%s/%s", param->gameName, saveName, param->fileName);

	SceUID fd = sceIoOpen(temp, PSP_O_RDONLY, 0);
	if (fd >= 0) {
		checkpoint("  File exists: %s (%08x)", temp, sceIoClose(fd));
	} else {
		checkpoint("  Does not exist: %s (%08x)", temp, fd);
	}
}

void checkpointExists(const SceUtilitySavedataParam2 *param) {
	checkpointNext("Checking for files:");
	checkpointExistsSaveName(param, "");
	checkpointExistsSaveName(param, param->saveName);

	if (param->saveNameList != NULL) {
		int i;
		for (i = 0; i < 100; ++i) {
			const char *saveName = param->saveNameList[i];
			if (saveName[0] == '\0') {
				break;
			}
			checkpointExistsSaveName(param, saveName);
		}
	}
}

// A little bit of a hack to have nice expected output.
extern volatile int didResched;
extern SceUID reschedThread;
void checkpointResetForSavedata() {
	didResched = 0;
	sceKernelStartThread(reschedThread, 0, NULL);
}

void runStandardSavedataLoop(SceUtilitySavedataParam2 *param) {
	setLastSaveParam(param);

	checkpointNext("Init");

	checkpoint("sceUtilitySavedataInitStart: %08x", sceUtilitySavedataInitStart((SceUtilitySavedataParam *) param));
	printSaveParamChanges(param);

	int i;
	int first = 1;
	for (i = 0; i < 400000; ++i) {
		int status = checkpointStatusChange();
		if (status == 3)
			break;
		printSaveParamChanges(param);

		if (status == 2) {
			int result = sceUtilitySavedataUpdate(1);
			if (result != 0 || first) {
				checkpoint("sceUtilitySavedataUpdate: %08x (duplicates skipped)", result);
				first = 0;
			}
		}
		printSaveParamChanges(param);

		sceKernelDelayThread(2000);
		checkpointResetForSavedata();
	}

	printSaveParamChanges(param);

	checkpoint("sceUtilitySavedataShutdownStart: %08x", sceUtilitySavedataShutdownStart());
	printSaveParamChanges(param);

	checkpointStatusChange();
	printSaveParamChanges(param);

	checkpoint("Delayed to allow for shutdown: %08x", sceKernelDelayThread(100000));

	checkpointStatusChange();
	printSaveParamChanges(param);
}