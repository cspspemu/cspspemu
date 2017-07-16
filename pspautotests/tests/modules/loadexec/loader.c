#include <common.h>

#include <pspkernel.h>
#include <psploadexec.h>
#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/unistd.h>

int main(int argc, char *argv[]) {
	char buf[MAXPATHLEN];
	struct SceKernelLoadExecParam params;
	params.size = sizeof(struct SceKernelLoadExecParam);
	params.args = argc;
	params.argp = argv;
	params.key  = NULL;

	getcwd(buf, MAXPATHLEN);
	strcat(buf, "/simple.prx");
	
	printf("[1]\n");
	sceKernelLoadExec(buf, &params);
	printf("[2]\n");

	return 0;
}