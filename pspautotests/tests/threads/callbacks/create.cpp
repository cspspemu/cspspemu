#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	return 0;
}

int cbFunc2(int arg1, int arg2, void *arg) {
	return 0;
}

void testCreate(const char *title, const char *name, SceKernelCallbackFunction func, void *arg) {
	SceUID cb = sceKernelCreateCallback(name, func, arg);
	if (cb >= 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfCallback(cb);
		sceKernelDeleteCallback(cb);
	} else {
		checkpoint("%s: Failed (%08x)", title, cb);
	}
}

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Names:");
	testCreate("  NULL name", NULL, &cbFunc, NULL);
	testCreate("  Normal name", "test", &cbFunc, NULL);
	testCreate("  Blank name", "", &cbFunc, NULL);
	testCreate("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", &cbFunc, NULL);

	SceUID dup = sceKernelCreateCallback("create", &cbFunc, NULL);
	testCreate("  Two with same name", "create", &cbFunc2, NULL);
	sceKernelDeleteCallback(dup);

	checkpointNext("Funcs:");
	testCreate("  NULL func", "test", NULL, NULL);
	testCreate("  Invalid func", "test", (SceKernelCallbackFunction)0xDEADBEEF, NULL);
	testCreate("  Invalid func #2", "test", (SceKernelCallbackFunction)0x07ADBEEF, NULL);
	testCreate("  NULL func with arg", "test", NULL, (void *)0x1337);

	dup = sceKernelCreateCallback("create", &cbFunc, NULL);
	testCreate("  Two with same func", "create2", &cbFunc, NULL);
	sceKernelDeleteCallback(dup);

	checkpointNext("Args:");
	testCreate("  Invalid arg pointer", "test", &cbFunc, (void *)0xDEADBEEF);

	checkpointNext("Create 1024:");
	SceUID cbs[1024];
	int result;
	int i;
	for (i = 0; i < 1024; i++)
	{
		result = sceKernelCreateCallback("create", &cbFunc, NULL);
		cbs[i] = result;
		if (cbs[i] < 0)
			break;
	}

	if (result < 0)
		checkpoint("  Failed at %d (%08x)", i, result);
	else
		checkpoint("  OK");

	while (--i >= 0)
		sceKernelDeleteCallback(cbs[i]);

	return 0;
}