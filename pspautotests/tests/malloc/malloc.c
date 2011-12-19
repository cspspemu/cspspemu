#include <pspkernel.h>
#include <pspthreadman.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

PSP_MODULE_INFO("MALLOC TEST", 0, 1, 1);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER | THREAD_ATTR_VFPU);

#define NUM_BUFFERS 1024
#define BUFFER_SIZE (5 * 1024)

// 24MB to use
int main(int argc, char** argv) {
	unsigned int sum;
	int n, m;
	unsigned int *buffers[NUM_BUFFERS];
	unsigned int *buffer;
	char *temp;
	Kprintf("STARTED\n");
	{
		for (n = 0; n < NUM_BUFFERS; n++) {
			buffers[n] = calloc(BUFFER_SIZE, sizeof(unsigned int));
			buffer = buffers[n];
			Kprintf("%s\n", (buffer == NULL) ? "NULL" : "NOT NULL");
			for (m = 0; m < BUFFER_SIZE; m++) buffer[m] = n;
		}
		for (n = 0; n < NUM_BUFFERS; n++) {
			sum = 0;
			buffer = buffers[n];
			for (m = 0; m < BUFFER_SIZE; m++) sum += buffer[m];
			Kprintf("%d: %d\n", n, sum);
		}
		temp = malloc(10 * 1024 * 1024);
		Kprintf("%s\n", (temp == NULL) ? "NULL" : "NOT NULL");
		for (n = 0; n < NUM_BUFFERS; n++) {
			free(buffers[n]);
		}
		temp = malloc(10 * 1024 * 1024);
		Kprintf("%s\n", (temp == NULL) ? "NULL" : "NOT NULL");
	}
	Kprintf("ENDED\n");

	sceKernelExitGame();
	return 0;
}
