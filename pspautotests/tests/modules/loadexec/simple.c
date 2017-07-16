#include <pspkernel.h>
#include <stdio.h>

PSP_MODULE_INFO("SIMPLE", 0x1000, 1, 1);

#define eprintf Kprintf

int main(int argc, char *argv[]) {
	Kprintf("Hello world!\n");
	return 0;
}