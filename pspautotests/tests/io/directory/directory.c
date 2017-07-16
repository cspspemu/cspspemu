#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

static int compare (const void * a, const void * b)
{
    /* The pointers point to offsets into "array", so we need to
       dereference them to get at the strings. */
    return strcmp (((struct SceIoDirent*)a)->d_name, ((struct SceIoDirent*)b)->d_name);
}

const char *accessBits(int d) {
	static char bits[4];
	int i = 0;
	if (d & 0444) {
		bits[i++] = 'r';
	}
	if (d & 0222) {
		bits[i++] = 'w';
	}
	if (d & 0111) {
		bits[i++] = 'x';
	}
	bits[i++] = '\0';
	return bits;
}

static void printFileModeFlags(SceIoStat *stat) {
	int m = stat->st_mode;
	const int supported = FIO_S_IFMT | FIO_S_ISUID | FIO_S_ISGID | FIO_S_ISVTX | FIO_S_IRWXU | FIO_S_IRWXG | FIO_S_IRWXO;

	switch (m & FIO_S_IFMT) {
	case FIO_S_IFLNK: printf("fmt=link"); break;
	case FIO_S_IFREG: printf("fmt=file"); break;
	case FIO_S_IFDIR: printf("fmt=dir"); break;
	default: printf("fmt=UNKNOWN %x", m & FIO_S_IFMT); break;
	}
	if (m & FIO_S_ISUID) {
		printf(", setuid");
	}
	if (m & FIO_S_ISGID) {
		printf(", setgid");
	}
	if (m & FIO_S_ISVTX) {
		printf(", sticky");
	}
	if (m & FIO_S_IRWXU) {
		printf(", user=%s", accessBits(m & FIO_S_IRWXU));
	}
	if (m & FIO_S_IRWXG) {
		printf(", group=%s", accessBits(m & FIO_S_IRWXG));
	}
	if (m & FIO_S_IRWXO) {
		printf(", other=%s", accessBits(m & FIO_S_IRWXO));
	}

	if (m & ~supported) {
		printf(", UNKNOWN=%x", m & ~supported);
	}
}

void checkDatetime(ScePspDateTime *dt) {
	// ATTN: Person from the future.
	// Sorry, you may need to adjust this check.
	if (dt->year < 2004 || dt->year > 2100) {
		printf("ERROR: Unexpected year: %d\n", dt->year);
	}
	if (dt->month < 1 || dt->month > 12) {
		printf("ERROR: Unexpected month: %d\n", dt->month);
	}
	if (dt->day < 1 || dt->day > 31) {
		printf("ERROR: Unexpected day: %d\n", dt->day);
	}
	if (dt->hour >= 24) {
		printf("ERROR: Unexpected hour: %d\n", dt->hour);
	}
	if (dt->minute >= 60) {
		printf("ERROR: Unexpected minute: %d\n", dt->minute);
	}
	if (dt->second >= 60) {
		printf("ERROR: Unexpected second: %d\n", dt->second);
	}
	if (dt->microsecond >= 1000000UL) {
		printf("ERROR: Unexpected microsecond: %d\n", dt->microsecond);
	}
}

int cdir_filter(const char *fname) {
	return strcmp(fname, ".");
}

int savedata_filter(const char *fname) {
	return strcmp(fname, "SAVEDATA") == 0;
}

void checkDirectory(const char *dirname, int checkMode, int filter(const char *fname)) {
	struct SceIoDirent files[32];
	int cnt = 0;
	int i;
	
	int fd = sceIoDopen(dirname);
	if (fd < 0) {
		printf("sceIoDopen: %08x\n", fd);
		return;
	}

	while (sceIoDread(fd, &files[cnt]) > 0) {
		cnt++;
	}
	sceIoDclose(fd);
	
	qsort(files, cnt, sizeof (struct SceIoDirent), compare);
	
	for (i = 0; i < cnt; i++) {
		if (!filter(files[i].d_name)) {
			continue;
		}
		printf(
				"'%s': size : %lld\n",
				files[i].d_name,
				files[i].d_stat.st_size
			);
		printf(
				"'%s': attr : %d\n",
				files[i].d_name,
				files[i].d_stat.st_attr
			);
		// Checking these flags on host0:/ only tests if psplink is the same on all platforms...
		if (checkMode) {
			printf("'%s': mode : ", files[i].d_name);
			printFileModeFlags(&files[i].d_stat);
			printf("\n");
		}
		checkDatetime(&files[i].d_stat.st_ctime);
		checkDatetime(&files[i].d_stat.st_atime);
		checkDatetime(&files[i].d_stat.st_mtime);
	}
}

int main(int argc, char **argv) {
	struct SceIoDirent files[32];

	checkDirectory("FakeDir", 0, &cdir_filter);
	checkDirectory("folder", 0, &cdir_filter);

	checkDirectory("ms0:/PSP", 1, &savedata_filter);

	printf("sceIoDread bad fd: %08x\n", sceIoDread(100, files));
	printf("sceIoDclose bad fd: %08x\n", sceIoDclose(100));

	return 0;
}
