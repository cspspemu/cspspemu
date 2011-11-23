#include "@common.h"

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" fpu.c -lpspumd -lpsppower -lpspdebug -lpspgu -lpspgum -lpspge -lpspdisplay -lpspsdk -lc -lpspnet -lpspnet_inet -lpspuser -lpsprtc -lpspctrl -o fpu.elf && psp-fixup-imports fpu.elf
// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" fpu.c -lc -lpspuser -o fpu.elf && psp-fixup-imports fpu.elf

float floatValues[] = { -67123.0f, +67123.0f, -100.0f, +100.0f, 0.0f, 1.0f, -1.0f };

/*
int _checkSetLessThanSigned(int a, int b) {
	return (a < b);
}

int _checkSetLessThanUnsigned(unsigned int a, unsigned int b) {
	return (a < b);
}

void checkSetLessThan() {
	int i, j;
	for (i = 0; i < lengthof(floatValues); i++) {
		for (j = 0; j < lengthof(floatValues); j++) {
			emitFloat(_checkSetLessThanSigned(i, j));
		}
	}
}
*/

int main(int argc, char **argv) {
	char temp[1024];
	emitFloat(floatValues[0]);
	emitFloat(floatValues[1]);
	emitFloat(floatValues[2]);
	emitFloat(floatValues[3]);
	emitFloat(floatValues[4]);
	emitFloat(floatValues[5]);
	emitFloat(floatValues[0] * 300 / 1000);
	emitFloat(11111.22222);
	sprintf(temp, "%.6f", (float)(floatValues[0] * 300 / 1000));
	emitFloat(98765.43210);
	emitString(temp);
	emitFloat(123.4567890);
	return 0;
}