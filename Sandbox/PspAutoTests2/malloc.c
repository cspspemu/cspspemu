#include "@common.h"

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" malloc.c -lc -lpspsdk -lpspuser -o malloc.elf && psp-fixup-imports malloc.elf

int main(int argc, char **argv) {
	emitInt(malloc(100) != 0);
	return 0;
}