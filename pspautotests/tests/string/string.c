#include <common.h>

#include <pspkernel.h>
#include <pspthreadman.h>
#include <stdio.h>
#include <string.h>

float value = 13.0;

int main(int argc, char** argv) {
	char buffer[128];
	
	sprintf(buffer, "%u", (uint)0);
	printf("%s\n", buffer);
	
	sprintf(buffer, "%u", (uint)100000);
	printf("%s\n", buffer);

	sprintf(buffer, "%llu", (u64)9);
	printf("%s\n", buffer);

	sprintf(buffer, "%llu", (u64)100000);
	printf("%s\n", buffer);

	sprintf(buffer, "%.2f", value);
	printf("%s\n", buffer);

	return 0;
}
