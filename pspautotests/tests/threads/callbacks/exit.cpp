#include "shared.h"
#include <psploadexec.h>

int cbFunc(int arg1, int arg2, void *arg) {
	checkpoint(" * cbFunc hit: %08x, %08x, %08x", arg1, arg2, arg);
	return 0;
}

struct ExitCallbackArgParams {
	int size;
	int unk1;
	int unk2;
};

struct ExitCallbackArg {
	int unknown1;
	ExitCallbackArgParams *params;
	int common;
};

extern "C" {
	int LoadExecForUser_362A956B();
	#include "sysmem-imports.h"
}

void testLoadExec362A956B(const char *title, ExitCallbackArg &arg) {
	int result = LoadExecForUser_362A956B();
	checkpoint("%s: %08x, %d, %d, %d", title, result, arg.params->size, arg.params->unk1, arg.params->unk2);
}

extern "C" int main(int argc, char *argv[]) {
	ExitCallbackArg arg;
	ExitCallbackArgParams params;

	arg.unknown1 = 0;
	arg.params = &params;
	params.size = 12;
	params.unk1 = 0x1337;
	params.unk2 = 0x1337;

	Callback cb1("count1", &cbFunc, NULL);
	Callback cb2("count2", &cbFunc, &arg.common);

	checkpointNext("Objects:");
	checkpoint("  Normal: %08x", sceKernelRegisterExitCallback(cb1));
	checkpoint("  NULL: %08x", sceKernelRegisterExitCallback(0));
	checkpoint("  Invalid: %08x", sceKernelRegisterExitCallback(0xDEADBEEF));
	cb1.Delete();
	checkpoint("  Deleted: %08x", sceKernelRegisterExitCallback(cb1));

	sceKernelSetCompiledSdkVersion(0x3090500);
	cb1.Create("count1", &cbFunc, NULL);

	checkpointNext("Objects (SDK 3.95+):");
	checkpoint("  Normal: %08x", sceKernelRegisterExitCallback(cb1));
	checkpoint("  NULL: %08x", sceKernelRegisterExitCallback(0));
	checkpoint("  Invalid: %08x", sceKernelRegisterExitCallback(0xDEADBEEF));
	cb1.Delete();
	checkpoint("  Deleted: %08x", sceKernelRegisterExitCallback(cb1));
	
	cb1.Create("count1", &cbFunc, NULL);
	checkpointNext("LoadExecForUser_362A956B:");
	testLoadExec362A956B("  With invalid CB", arg);
	checkpoint("  Register without arg: %08x", sceKernelRegisterExitCallback(cb1));
	testLoadExec362A956B("  Without arg", arg);
	checkpoint("  Register invalid: %08x", sceKernelRegisterExitCallback(0));
	testLoadExec362A956B("  With invalid again", arg);
	checkpoint("  Register valid: %08x", sceKernelRegisterExitCallback(cb2));

	int unknown1s[] = {-1, 0, 1, 2, 3, 4};
	for (size_t i = 0; i < ARRAY_SIZE(unknown1s); ++i) {
		char temp[32];
		snprintf(temp, sizeof(temp), "  arg.unknown1 = %d", unknown1s[i]);
		arg.unknown1 = unknown1s[i];
		params.unk1 = 0x1337;
		params.unk2 = 0x1337;
		testLoadExec362A956B(temp, arg);
	}
	arg.unknown1 = 0;

	int sizes[] = {-1, 0, 1, 2, 3, 4, 11, 12, 13, 16};
	for (size_t i = 0; i < ARRAY_SIZE(sizes); ++i) {
		char temp[32];
		snprintf(temp, sizeof(temp), "  arg.param->size = %d", sizes[i]);
		params.size = sizes[i];
		params.unk1 = 0x1337;
		params.unk2 = 0x1337;
		testLoadExec362A956B(temp, arg);
	}
	params.size = 12;

	cb1.Delete();
	cb2.Delete();
	return 0;
}