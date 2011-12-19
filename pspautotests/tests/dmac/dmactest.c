#include <common.h>

#include <pspkernel.h>
#include <oslib/oslib.h>

char dataSource[] = "Hello World. This is a test to check if sceDmacMemcpy works.";
char dataDest[sizeof(dataSource)] = {0};

int main(int argc, char *argv[]) {
	printf("%s\n", dataSource);
	printf("%s\n", dataDest);

	oslUncacheData(dataSource, sizeof(dataSource));
	oslUncacheData(dataDest, sizeof(dataDest));
	sceDmacMemcpy(dataDest, dataSource, sizeof(dataSource));

	printf("%s\n", dataDest);

	void *ptr;
	ptr = memalign(128 , 2048); printf("%d\n", ((int)ptr) % 128);
	ptr = memalign(1024, 2048); printf("%d\n", ((int)ptr) % 1024);
	//ptr = memalign(100 , 2048); printf("%d\n", ((int)ptr) % 100);

	//printf("%i bytes available\n", oslGetRamStatus().maxAvailable);

	return 0;
}