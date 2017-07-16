#include <common.h>

int makeTestFile(const char *name) {
	SceUID fd = sceIoOpen(name, PSP_O_WRONLY | PSP_O_CREAT, 0777);
	if (fd < 0) {
		return fd;
	}
	return sceIoClose(fd);
}

int fileExists(const char *name) {
	SceUID fd = sceIoOpen(name, PSP_O_RDONLY, 0777);
	if (fd >= 0)
		sceIoClose(fd);
	return fd >= 0;
}

int main(int argc, char *argv[]) {
	checkpoint("  Create test file 1: %08x", makeTestFile("ms0:/__iorename_test1.txt"));
	checkpoint("  Create test file 2: %08x", makeTestFile("ms0:/__iorename_test2.txt"));
	checkpoint("  Create test file 3: %08x", makeTestFile("ms0:/__iorename_test3.txt"));

	checkpointNext("Rename behavior:");

	checkpoint("  sceIoChdir host0:/: %08x", sceIoChdir("host0:/"));
	checkpoint("  sceIoRename abs -> abs: %08x", sceIoRename("ms0:/__iorename_test1.txt", "ms0:/__iorename_test1a.txt"));
	checkpoint("  sceIoRename abs -> abs overwrite: %08x", sceIoRename("ms0:/__iorename_test1a.txt", "ms0:/__iorename_test2.txt"));

	checkpoint("  sceIoChdir ms0:/PSP: %08x", sceIoChdir("ms0:/PSP"));
	checkpoint("  sceIoRename rel -> rel: %08x", sceIoRename("../__iorename_test2.txt", "__iorename_test2a.txt"));
	checkpoint("    renamed to: %s", fileExists("ms0:/__iorename_test2a.txt") ? "origin dir" : (fileExists("ms0:/PSP/__iorename_test2a.txt") ? "cwd" : "nowhere?"));

	checkpoint("  sceIoRename rel -> abs: %08x", sceIoRename("../__iorename_test3.txt", "ms0:/PSP/__iorename_test3a.txt"));
	checkpoint("    renamed to: %s", fileExists("ms0:/__iorename_test3a.txt") ? "origin dir" : (fileExists("ms0:/PSP/__iorename_test3a.txt") ? "dest dir" : "nowhere?"));

	// Okay, we've moved everything around a lot, let's clean it up.
	sceIoRemove("ms0:/__iorename_test1.txt");
	sceIoRemove("ms0:/__iorename_test1a.txt");
	sceIoRemove("ms0:/__iorename_test2.txt");
	sceIoRemove("ms0:/__iorename_test2a.txt");
	sceIoRemove("ms0:/__iorename_test3.txt");
	sceIoRemove("ms0:/__iorename_test3a.txt");
	sceIoRemove("ms0:/PSP/__iorename_test2a.txt");
	sceIoRemove("ms0:/PSP/__iorename_test3a.txt");

	checkpointNext("Error handling:");
	checkpoint("  Recreate test file 1: %08x", makeTestFile("ms0:/__iorename_test1.txt"));
	checkpoint("  Recreate test file 2: %08x", makeTestFile("ms0:/__iorename_test2.txt"));

	checkpoint("  sceIoRename cross device: %08x", sceIoRename("ms0:/__iorename_test1.txt", "host0:/__iorename_test1.txt"));
	checkpoint("  sceIoRename bad dest path: %08x", sceIoRename("ms0:/__iorename_test2.txt", "ms0:/__DOES_NOT_EXIST/__iorename_test2.txt"));
	checkpoint("  sceIoRename bad src path: %08x", sceIoRename("ms0:/_DOES_NOT_EXIST/__iorename_test3.txt", "ms0:/__iorename_test3.txt"));
	checkpoint("  sceIoRename bad src file: %08x", sceIoRename("ms0:/__iorename_test3.txt", "ms0:/__iorename_test3a.txt"));
	checkpoint("  sceIoRename same path: %08x", sceIoRename("ms0:/__iorename_test1.txt", "ms0:/__iorename_test1.txt"));
	// Crash.
	//checkpoint("  sceIoRename NULL -> rel", sceIoRename(NULL, "test.txt"));
	//checkpoint("  sceIoRename rel -> NULL", sceIoRename("test.txt", NULL));
	//checkpoint("  sceIoRename NULL -> NULL", sceIoRename(NULL, NULL));
	checkpoint("  sceIoRename wild -> rel: %08x", sceIoRename("ms0:/__iorename_test*.txt", "ms0:/__iorename_test1a.txt"));
	checkpoint("  sceIoRename rel -> wild: %08x", sceIoRename("ms0:/__iorename_test1.txt", "ms0:/__iorename_test*.txt"));
	checkpoint("  sceIoRename wild -> wild: %08x", sceIoRename("ms0:/__iorename_test?.txt", "ms0:/__iorename_test?.txt"));

	sceIoRemove("ms0:/__iorename_test1.txt");
	sceIoRemove("ms0:/__iorename_test2.txt");
	sceIoRemove("host0:/__iorename_test1.txt");

	return 0;
}