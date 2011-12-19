#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

int main(int argc, char **argv) {
	SceIoDirent file;
	int fd;

	fd = sceIoDopen("ms0:/PSP/GAME/virtual/folder");
	while (1) {
		if (sceIoDread(fd, &file) <= 0) break;
		printf(
			"'%s':%lld:%d:%d\n",
			file.d_name,
			file.d_stat.st_size,
			file.d_stat.st_attr,
			file.d_stat.st_mode
		);
	}
	sceIoDclose(fd);

	return 0;
}