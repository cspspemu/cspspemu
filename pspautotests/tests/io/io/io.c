#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <pspdebug.h>
#include <pspdisplay.h>
#include <psptypes.h>
#include <pspiofilemgr.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <dirent.h>
#include <sys/types.h>
#include <sys/unistd.h>

//#include "../common/emits.h"

char buf[MAXPATHLEN] = {0};
char startPath[MAXPATHLEN] = {0};

/**
 * Utility for .
 */
void checkChangePathsTry(const char *dest) {
	if (chdir(dest) < 0) {
		printf("(chdir error)\n");
	} else {
		printf("%s\n", getcwd(buf, MAXPATHLEN) ?: "(getcwd error)");
	}
}

/**
 * Check changing paths.
 */
void checkChangePaths() {
	printf("%s\n", getcwd(buf, MAXPATHLEN));
	checkChangePathsTry("");                 // empty string
	checkChangePathsTry("hello");            // nonexistent path
	checkChangePathsTry("..");               // parent dir
	checkChangePathsTry("../SAVEDATA");      // parent dir and subdir
	checkChangePathsTry("../..");            // multiple parents
	checkChangePathsTry(".");                // current dir
	checkChangePathsTry("./././//PSP");      // current dirs, extra slashes
	checkChangePathsTry("/PSP/./GAME");      // absolute with no drive
	checkChangePathsTry("/");                // root with no drive
	checkChangePathsTry("ms0:/PSP/GAME");    // absolute with drive
	checkChangePathsTry("flash0:/");         // different drive
	checkChangePathsTry("ms0:/PSP/../PSP/"); // mixed
	chdir(startPath);
}

/**
 * Check opening files.
 */
void checkFileOpen() {
	FILE *file = fopen("io.expected", "rb");
	printf("%d\n", file != NULL);
	if (file != NULL) {
		fseek(file, 0, SEEK_END);
		printf("%d\n", ftell(file) > 0);
		fclose(file);
	} else {
		printf("%d\n", 0);
	}
}

/**
 * Check listing directories.
 */
void checkDirectoryList() {
	DIR *dir;
	struct dirent *dp;
	dir = opendir(".");
	if (dir != NULL) {
		while ((dp = readdir(dir)) != NULL) {
			if (strncmp("io", dp->d_name, 2) == 0) {
				printf("%s\n", dp->d_name);
			}
		}
		closedir(dir);
	}
}

/**
 * Check listing directories extended.
 * Check that ms0:/PSP contains a directory called GAME with st_attr containing ONLY FIO_SO_IFDIR flag.
 */
void checkDirectoryListEx() {
	#define MAX_ENTRY 16
	SceIoDirent files[MAX_ENTRY];
	int fd, nfiles = 0;

	fd = sceIoDopen("ms0:/PSP");
	while (nfiles < MAX_ENTRY) {
		memset(&files[nfiles], 0x00, sizeof(SceIoDirent));
		if (sceIoDread(fd, &files[nfiles]) <= 0) break;
		if (files[nfiles].d_name[0] == '.') continue;

		if (strcmp(files[nfiles].d_name, "GAME") == 0) {
			// st_attr should only contain FIO_SO_IFDIR flag. (Required by NesterJ).
			if (files[nfiles].d_stat.st_attr == FIO_SO_IFDIR){
				printf("%s\n", files[nfiles].d_name);
				continue;
			}
		}
	}
	sceIoDclose(fd);
}

/**
 * Check that there are arguments passed to the app.
 * And the first argument should contain the path to the executable.
 * That path will be used to determine current working directory in libc/newlib.
 */
void checkMainArgs(int argc, char** argv) {
	int n;
	printf("%d\n", argc);
	for (n = 0; n < argc; n++) printf("%s\n", argv[n]);
}

int main(int argc, char** argv) {
	getcwd(startPath, MAXPATHLEN);
	checkMainArgs(argc, argv);
	checkChangePaths();
	checkFileOpen();
	checkDirectoryList();
	checkDirectoryListEx();
	return 0;
}