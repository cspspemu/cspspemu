#include <common.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>

#define NUM_BUFFERS 1024
#define BUFFER_SIZE (5 * 1024)

// 24MB to use
int main(int argc, char** argv) {
	unsigned int sum;
	int n, m;
	unsigned int *buffers[NUM_BUFFERS];
	unsigned int *buffer;
	char *temp;
	printf("Allocating %d MB\n", (NUM_BUFFERS * BUFFER_SIZE * sizeof(unsigned int)) / (1024 * 1024));
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
		temp = malloc(20 * 1024 * 1024);
		printf("Allocate 20 MB: %s\n", (temp == NULL) ? "NULL" : "NOT NULL");
		for (n = 0; n < NUM_BUFFERS; n++) {
			free(buffers[n]);
		}
		temp = malloc(20 * 1024 * 1024);
		printf("Allocate 20 MB after free: %s\n", (temp == NULL) ? "NULL" : "NOT NULL");
	}
	printf("ENDED\n");

	return 0;
}
