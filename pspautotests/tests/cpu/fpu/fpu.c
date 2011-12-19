/*
FPU Test. Originally from jpcsp project:
http://code.google.com/p/jpcsp/source/browse/trunk/demos/src/fputest/main.c
Modified to perform automated tests.
*/

#include <common.h>

#include <pspkernel.h>

float __attribute__((noinline)) adds(float x, float y) {
	float result;
	asm volatile("add.s %0, %1, %2" : "=f"(result) : "f"(x), "f"(y));
	return result;
}

float __attribute__((noinline)) subs(float x, float y) {
	float result;
	asm volatile("sub.s %0, %1, %2" : "=f"(result) : "f"(x), "f"(y));
	return result;
}

float __attribute__((noinline)) muls(float x, float y) {
	float result;
	asm volatile("mul.s %0, %1, %2" : "=f"(result) : "f"(x), "f"(y));
	return result;
}

float __attribute__((noinline)) divs(float x, float y) {
	float result;
	asm volatile("div.s %0, %1, %2" : "=f"(result) : "f"(x), "f"(y));
	return result;
}

float __attribute__((noinline)) sqrts(float x) {
	float result;
	asm volatile("sqrt.s %0, %1" : "=f"(result) : "f"(x));
	return result;
}

float __attribute__((noinline)) abss(float x) {
	float result;
	asm volatile("abs.s %0, %1" : "=f"(result) : "f"(x));
	return result;
}

float __attribute__((noinline)) negs(float x) {
	float result;
	asm volatile("neg.s %0, %1" : "=f"(result) : "f"(x));
	return result;
}

int __attribute__((noinline)) cvtws(float x, int rm) {
	float resultFloat;
	asm volatile("ctc1 %0, $31" : : "r"(rm));
	asm volatile("cvt.w.s %0, %1" : "=f"(resultFloat) : "f"(x));
	int result = *((int *) &resultFloat);
	return result;
}

#define CHECK_OP(op, expected) { float f = 0.0; f = op; printf("%s\n%f\n", #op " == " #expected, f);}

#define RINT_0  0
#define CAST_1  1
#define CEIL_2  2
#define FLOOR_3 3

int main(int argc, char *argv[]) {
	CHECK_OP(adds(1.0, 1.0), 2.0);
	CHECK_OP(subs(3.0, 1.0), 2.0);
	CHECK_OP(muls(2.0, 1.0), 2.0);
	CHECK_OP(divs(4.0, 2.0), 2.0);
	CHECK_OP(abss(+2.0), 2.0);
	CHECK_OP(abss(-2.0), 2.0);
	CHECK_OP(negs(negs(+2.0)), 2.0);
	CHECK_OP(sqrts(4.0), 2.0);

	CHECK_OP(cvtws(1.1, RINT_0), 1);
	CHECK_OP(cvtws(1.1, CAST_1), 1);
	CHECK_OP(cvtws(1.1, CEIL_2), 2);
	CHECK_OP(cvtws(1.1, FLOOR_3), 1);

	CHECK_OP(cvtws(-1.1, RINT_0), -1);
	CHECK_OP(cvtws(-1.1, CAST_1), -1);
	CHECK_OP(cvtws(-1.1, CEIL_2), -1);
	CHECK_OP(cvtws(-1.1, FLOOR_3), -2);

	CHECK_OP(cvtws(1.9, RINT_0), 2);
	CHECK_OP(cvtws(1.9, CAST_1), 1);
	CHECK_OP(cvtws(1.9, CEIL_2), 2);
	CHECK_OP(cvtws(1.9, FLOOR_3), 1);

	CHECK_OP(cvtws(1.5, RINT_0), 2);
	CHECK_OP(cvtws(1.5, CAST_1), 1);
	CHECK_OP(cvtws(1.5, CEIL_2), 2);
	CHECK_OP(cvtws(1.5, FLOOR_3), 1);

	return 0;
}
