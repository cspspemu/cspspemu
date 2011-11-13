#include "@common.h"

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" @imports.S args.c -lc -lpspsdk -lpspuser -o args.elf && psp-fixup-imports args.elf

int main(int argc, char **argv) {
	emitInt(argc > 0);
	emitInt(strlen(argv[0]) > 0);
	return 0;
}