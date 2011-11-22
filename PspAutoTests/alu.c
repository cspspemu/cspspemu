#include "@common.h"
#include <stdio.h>
#include <stdlib.h>

// SET PSPSDK=c:\pspsdk
// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" alu.c -lc -lg -lpsplibc -lpspsdk -lpspuser -o alu.elf && psp-fixup-imports alu.elf

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

int GlobalShiftValue = 8;

void checkLeftShift() {
	int Value = 0x00001122;
	emitUInt(Value << 16);
	emitUInt(Value << GlobalShiftValue);
}

void checkRightShift() {
	int Value1 = 0x11223344;
	int Value2 = 0xFF223344;
	emitUInt(Value1 >> 16);
	emitUInt(Value1 >> GlobalShiftValue);
	emitUInt(Value2 >> 16);
	emitUInt(Value2 >> GlobalShiftValue);
	emitUInt(((unsigned int)Value2) >> 16);
	emitUInt(((unsigned int)Value2) >> GlobalShiftValue);
}

int main(int argc, char **argv) {
	char buffer[1024];
	int decpt, sign;
	char *rve;
	
	checkSetLessThan();
	checkLeftShift();
	checkRightShift();
	
/*
dtoa(double d, int mode, int ndigits, int *decpt, int *sign, char **rve);
*/
	
	emitString((char *)_itoa(12345678, buffer, 10));
	//emitString(dtoa(1234.567, 0, 10, &decpt, &sign, &rve));

	return 0;
}