#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

int gpValue = 1;

int main(int argc, char **argv) {
	printf("%d", gpValue);
	gpValue = 2;
	printf("%d", gpValue);

	return 0;
}