#include <common.h>

#include <stdio.h>
#include <pspkernel.h>
#include <pspiofilemgr.h>
#include <pspiofilemgr_kernel.h>
#include <oslib/oslib.h>

int test_IoInit(PspIoDrvArg* arg) {
	arg->arg = (void *)777;
	printf("test_IoInit(%d)(%d)\n", (arg->drv->funcs->IoInit == test_IoInit), (int)arg->arg);
	return -1;
}

int test_IoExit(PspIoDrvArg* arg) {
	printf("test_IoExit(%d)\n", (int)arg->arg);
	return -1;
}

int test_IoOpen(PspIoDrvFileArg *arg, char *file, int flags, SceMode mode) {
	printf("test_IoOpen('%s',%d,%04o)\n", file, flags, mode);
	arg->arg = (void *)333;
	return -1;
}

int test_IoClose(PspIoDrvFileArg *arg) {
	printf("test_IoClose\n");
	return -1;
}

int test_IoRead(PspIoDrvFileArg *arg, char *data, int len) {
	printf("test_IoRead\n");
	return -1;
}

int test_IoWrite(PspIoDrvFileArg *arg, const char *data, int len) {
	printf("test_IoWrite\n");
	return -1;
}

SceOff test_IoLseek(PspIoDrvFileArg *arg, SceOff ofs, int whence) {
	printf("test_IoLseek\n");
	return -1;
}

int test_IoIoctl(PspIoDrvFileArg *arg, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen) {
	printf("test_IoIoctl\n");
	return -1;
}

int test_IoRemove(PspIoDrvFileArg *arg, const char *name) {
	printf("test_IoRemove\n");
	return -1;
}

int test_IoMkdir(PspIoDrvFileArg *arg, const char *name, SceMode mode) {
	printf("test_IoMkdir\n");
	return -1;
}

int test_IoRmdir(PspIoDrvFileArg *arg, const char *name) {
	printf("test_IoRmdir\n");
	return -1;
}

int test_IoDopen(PspIoDrvFileArg *arg, const char *dirname) {
	printf("test_IoDopen\n");
	return -1;
}

int test_IoDclose(PspIoDrvFileArg *arg) {
	printf("test_IoDclose\n");
	return -1;
}

int test_IoDread(PspIoDrvFileArg *arg, SceIoDirent *dir) {
	printf("test_IoDread\n");
	return -1;
}

int test_IoGetstat(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat) {
	printf("test_IoGetstat\n");
	return -1;
}

int test_IoChstat(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat, int bits) {
	printf("test_IoChstat\n");
	return -1;
}

int test_IoRename(PspIoDrvFileArg *arg, const char *oldname, const char *newname) {
	printf("test_IoRename\n");
	return -1;
}

int test_IoChdir(PspIoDrvFileArg *arg, const char *dir) {
	printf("test_IoChdir\n");
	return -1;
}

int test_IoMount(PspIoDrvFileArg *arg) {
	printf("test_IoMount\n");
	return -1;
}

int test_IoUmount(PspIoDrvFileArg *arg) {
	printf("test_IoUmount\n");
	return -1;
}

int test_IoDevctl(PspIoDrvFileArg *arg, const char *devname, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen) {
	printf("test_IoDevctl\n");
	return -1;
}

int test_IoUnk21(PspIoDrvFileArg *arg) {
	printf("test_IoUnk21\n");
	return -1;
}

int main(int argc, char *argv[]) {
	PspIoDrvFuncs test_funcs = {
		test_IoInit,
		test_IoExit,
		test_IoOpen,
		test_IoClose,
		test_IoRead,
		test_IoWrite,
		test_IoLseek,
		test_IoIoctl,
		test_IoRemove,
		test_IoMkdir,
		test_IoRmdir,
		test_IoDopen,
		test_IoDclose,
		test_IoDread,
		test_IoGetstat,
		test_IoChstat,
		test_IoRename,
		test_IoChdir,
		test_IoMount,
		test_IoUmount,
		test_IoDevctl,
		test_IoUnk21,
	};
	PspIoDrv test_driver = { "test", 0x10, 0x800, "TEST", &test_funcs };

	//sceIodeldrv("umd");
	sceIoAddDrv(&test_driver);
	sceIoOpen("test:/path/to/file", PSP_O_WRONLY | PSP_O_CREAT, 0777);
	//printf("%d\n", );
	sceIoDelDrv("test");
	
	return 0;
}