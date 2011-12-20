#include <pspkernel.h>
#include <pspthreadman.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <common.h>


#define NUM_BUFFERS 64
#define BUFFER_SIZE (5 * 1024 * 1024 / NUM_BUFFERS)

// 24MB to use
int main(int argc, char** argv) {
	unsigned int sum;
	int n, m;
	unsigned int *buffers[NUM_BUFFERS];
	unsigned int *buffer;
	char *temp;
	printf("STARTED\n");
	{
		for (n = 0; n < NUM_BUFFERS; n++) {
			buffers[n] = calloc(BUFFER_SIZE, sizeof(unsigned int));
			buffer = buffers[n];
			printf("%s\n", (buffer == NULL) ? "NULL" : "NOT NULL");
			for (m = 0; m < BUFFER_SIZE; m++) buffer[m] = n;
		}
		for (n = 0; n < NUM_BUFFERS; n++) {
			sum = 0;
			buffer = buffers[n];
			for (m = 0; m < BUFFER_SIZE; m++) sum += buffer[m];
			printf("%d: %d\n", n, sum);
		}
		temp = malloc(24 * 1024 * 1024);
		printf("%s\n", (temp == NULL) ? "NULL" : "NOT NULL");
		if (temp != NULL) free(temp);
		for (n = 0; n < NUM_BUFFERS; n++) {
			free(buffers[n]);
		}
		temp = malloc(24 * 1024 * 1024);
		printf("%s\n", (temp == NULL) ? "NULL" : "NOT NULL");
		if (temp != NULL) free(temp);
	}
	printf("ENDED\n");

	return 0;
}
