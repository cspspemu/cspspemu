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
#include <sys/types.h>
#include <sys/unistd.h>
#include <sys/stat.h>
#include <dirent.h>

//#include "../common/emits.h"

char buf[MAXPATHLEN] = {0};
char startPath[MAXPATHLEN] = {0};
char currentPath[MAXPATHLEN] = {};

static int compare (const void * a, const void * b)
{
    /* The pointers point to offsets into "array", so we need to
       dereference them to get at the strings. */
    return strcmp (((struct dirent*)a)->d_name, ((struct dirent*)b)->d_name);
}

static int compare2 (const void * a, const void * b)
{
    /* The pointers point to offsets into "array", so we need to
       dereference them to get at the strings. */
    return strcmp (((struct SceIoDirent*)a)->d_name, ((struct SceIoDirent*)b)->d_name);
}


/**
 * Utility for .
 */
void checkChangePathsTry(const char *name, const char *dest, int offset) {
	if (chdir(dest) < 0) {
		printf("%s: (chdir error) %08x\n", name, chdir(dest));
	} else {
		char *result = getcwd(buf, MAXPATHLEN);
		int len = strlen(buf);
		if (len <= offset) {
			printf("%s: outside or at basedir\n", name);
		} else {
			printf("%s: %s\n", name, result ? result + offset : "(getcwd error)");
		}
	}
}

void checkChangePathsTryDirect(const char *name, const char *dest, int offset) {
	int result = sceIoChdir(dest);
	if (result < 0) {
		printf("%s: (sceIoChdir error) %08x\n", name, result);
	} else {
		printf("%s: %d\n", name, result);
	}
}


/**
 * Dump directory listing
 */
void dumpDir(const char* fileCheckName) {
	
	struct SceIoDirent files[32];
	int cnt = 0;
	int i = 0;

	int fd;
	fd = sceIoDopen(".");
	while (cnt < 32) {
		if (sceIoDread(fd, &files[cnt]) <= 0) break;
		if (fileCheckName != 0 && strcmp(fileCheckName,files[cnt].d_name) != 0) continue;
		cnt++;
	}
	sceIoDclose(fd);
	
	if(fileCheckName != 0 && cnt == 0)
	{
		printf("%s not found\n",fileCheckName);
	}
	
	qsort (files, cnt, sizeof (struct SceIoDirent), compare2);
	for(i = 0; i < cnt; i++)
	{
		printf("%s\n", files[i].d_name);
	}
}

/**
 * Check changing paths.
 */
void checkChangePaths() {
	char baseDir[MAXPATHLEN];
	int baseDirLen;

	printf("result: %d\n", getcwd(baseDir, MAXPATHLEN) == baseDir);
	baseDirLen = strlen(baseDir);
	strcpy(currentPath,baseDir);

	// Setup paths.
	mkdir("otherdir", 0777);
	mkdir("testdir", 0777);
	mkdir("testdir/testdir2", 0777);
	mkdir("testdir/testdir2/test", 0777);

	checkChangePathsTry("Initial", "testdir/testdir2", baseDirLen);
	checkChangePathsTry("Empty", "", baseDirLen);
	checkChangePathsTry("Non-existent", "hello", baseDirLen);
	checkChangePathsTry("Parent", "..", baseDirLen);
	checkChangePathsTry("Parent + subdirs", "../testdir/testdir2", baseDirLen);
	checkChangePathsTry("Multiple parents", "../..", baseDirLen);
	checkChangePathsTry("Back to testdir", "testdir", baseDirLen);
	checkChangePathsTry("Current dir", ".", baseDirLen);
	checkChangePathsTry("Current + extra slashes", "./././//testdir2", baseDirLen);
	checkChangePathsTry("Switch drive no slash", "ms0:", 0);
	checkChangePathsTry("Switch drive + slash", "ms0:/", 0);
	checkChangePathsTry("Absolute + drive", "ms0:/PSP/SAVEDATA", 0);
	checkChangePathsTry("Absolute no drive", "/PSP", 0);
	checkChangePathsTry("Root", "/", 0);
	//checkChangePathsTry("Flash drive", "flash0:/", 0);
	checkChangePathsTry("PSP and back again + trailing /", "ms0:/PSP/../PSP/", 0);
	
	strcat(startPath,"/");
	chdir(startPath);
	
	// Since we can't getcwd when using directly sceIoChdir, dump directory to compare.
	checkChangePathsTryDirect("Initial", "testdir/testdir2", baseDirLen);
	dumpDir("test");
	checkChangePathsTryDirect("Empty", "", baseDirLen);
	dumpDir("test");
	checkChangePathsTryDirect("Non-existent", "hello", baseDirLen);
	dumpDir(0);
	checkChangePathsTryDirect("Parent", "..", baseDirLen);
	dumpDir("test");
	checkChangePathsTryDirect("Parent", "..", baseDirLen);
	dumpDir("testdir2");
	checkChangePathsTryDirect("Parent + subdirs", "../testdir/testdir2", baseDirLen);
	dumpDir(0);
	checkChangePathsTryDirect("Multiple parents", "../..", baseDirLen);
	dumpDir("testdir");
	checkChangePathsTryDirect("Back to testdir", "testdir", baseDirLen);
	dumpDir("testdir2");
	checkChangePathsTryDirect("Current dir", ".", baseDirLen);
	dumpDir("testdir2");
	checkChangePathsTryDirect("Current + extra slashes", "./././//testdir2", baseDirLen);
	dumpDir("test");
	checkChangePathsTryDirect("Switch drive no slash", "ms0:", 0);
	dumpDir("PSP");
	checkChangePathsTryDirect("Switch drive + slash", "ms0:/", 0);
	dumpDir("PSP");
	checkChangePathsTryDirect("Absolute + drive", "ms0:/PSP/", 0);
	dumpDir("SAVEDATA");
	checkChangePathsTryDirect("Absolute no drive", "/PSP", 0);
	dumpDir("SAVEDATA");
	checkChangePathsTryDirect("Root", "/", 0);
	dumpDir("PSP");
	checkChangePathsTryDirect("Flash drive", "flash0:/", 0);
	dumpDir(".");
	checkChangePathsTryDirect("PSP and back again + trailing /", "ms0:/PSP/../PSP/", 0);
	dumpDir("SAVEDATA");
	
	strcat(startPath,"/");
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
	
	struct dirent dp_found[32];
	int cnt = 0;
	int i = 0;

	struct dirent *dp;
	DIR* dir;
	dir = opendir(".");
	if (dir != NULL) {
		while ((dp = readdir(dir)) != NULL) {
			if (strncmp("io", dp->d_name, 2) == 0) {
				memcpy(&dp_found[cnt],dp,sizeof(struct dirent));
				cnt++;
			}
		}
		closedir(dir);
	}
	qsort (dp_found, cnt, sizeof (struct dirent), compare);
	for(i = 0; i < cnt; i++)
	{
		printf("%s\n", dp_found[i].d_name);
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

	if (strlen(argv[0]) > strlen(startPath)) {
		printf("%s\n", argv[0] + strlen(startPath));
	} else {
		printf("%s\n", argv[0]);
	}

	for (n = 1; n < argc; n++) {
		printf("%s\n", argv[n]);
	}
}

int main(int argc, char** argv) {
	getcwd(startPath, MAXPATHLEN);
	//checkMainArgs(argc, argv);
	checkChangePaths();
	checkFileOpen();
	checkDirectoryList();
	checkDirectoryListEx();
	return 0;
}
