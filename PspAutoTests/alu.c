#include "@common.h"

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" alu.c -lc -lpspsdk -lpspuser -o alu.elf && psp-fixup-imports alu.elf

int intValues[] = { -67123, +67123, -100, +100, 0 };

int _checkSetLessThanSigned(int a, int b) {
	return (a < b);
}

int _checkSetLessThanUnsigned(unsigned int a, unsigned int b) {
	return (a < b);
}

void checkSetLessThan() {
	int i, j;
	for (i = 0; i < lengthof(intValues); i++) {
		for (j = 0; j < lengthof(intValues); j++) {
			emitInt(_checkSetLessThanSigned(i, j));
			emitInt(_checkSetLessThanUnsigned(i, j));
		}
	}
}

int main(int argc, char **argv) {
	checkSetLessThan();
	return 0;
}