/*
FPU Test. Originally from jpcsp project:
http://code.google.com/p/jpcsp/source/browse/trunk/demos/src/fputest/main.c
Modified to perform automated tests.
*/


// For some reason, vectors on the stack all are unaligned. So I've pulled them out
// into globals, which is horribly ugly. But it'll do for now.


#include <common.h>

#include <pspkernel.h>
#include <stdio.h>
#include <string.h>
#include <math.h>

#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>
//#include "../common/emits.h"

#include <pspgu.h>
#include <pspgum.h>

#include "vfpu_common.h"

__attribute__ ((aligned (16))) ScePspFVector4 v0, v1, v2, v3;
__attribute__ ((aligned (16))) ScePspFMatrix4 matrix;
__attribute__ ((aligned (16))) ScePspFMatrix4 matrix2;

void __attribute__((noinline)) vmidt(int size, ScePspFMatrix4 *v0, ScePspFMatrix4 *v1) {
	asm volatile (
		"lv.q    R000, %1\n"
		"lv.q    R001, %1\n"
		"lv.q    R002, %1\n"
		"lv.q    R003, %1\n"

		: "+m" (*v0) : "m" (*v1)
		);

	switch (size) {
	case 2: asm volatile("vmidt.p M000\nvmidt.p M022\n"); break;
	case 3: asm volatile("vmidt.t M000\n"); break;
	case 4: asm volatile("vmidt.q M000\n"); break;
	case 5: asm volatile("vmone.q M000\n"); break;
	case 6: asm volatile("vmzero.q M000\n"); break;

	}

	asm volatile (
		"sv.q    R000, 0x00+%0\n"
		"sv.q    R001, 0x10+%0\n"
		"sv.q    R002, 0x20+%0\n"
		"sv.q    R003, 0x30+%0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void checkMatrixIdentity() {
	int vsize;
	for (vsize = 2; vsize <= 6; vsize++) {
		nonsense(&matrix);
		vmidt(vsize, &matrix2, &matrix);
		printMatrix("vmidt", &matrix2);
	}
}

void _checkMultiply2(ScePspFMatrix4* m0, ScePspFMatrix4 *m1, ScePspFMatrix4 *m2) {
	__asm__ volatile (

		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		"lv.q    R100, 0x00+%2\n"
		"lv.q    R101, 0x10+%2\n"
		"lv.q    R102, 0x20+%2\n"
		"lv.q    R103, 0x30+%2\n"

		"vmmul.p M200, M000, M100\n"
		"vmmul.p M202, M002, E102\n"
		"vmmul.p E220, M020, M120\n"
		"vmmul.p M222, E022, M122\n"

		"sv.q    R200, 0x00+%0\n"
		"sv.q    R201, 0x10+%0\n"
		"sv.q    R202, 0x20+%0\n"
		"sv.q    R203, 0x30+%0\n"
		: "+m" (*m0) : "m" (*m1), "m" (*m2)
		);
}

void _checkMultiply4(ScePspFMatrix4* m0, ScePspFMatrix4 *m1, ScePspFMatrix4 *m2) {
	__asm__ volatile (

		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		"lv.q    R100, 0x00+%2\n"
		"lv.q    R101, 0x10+%2\n"
		"lv.q    R102, 0x20+%2\n"
		"lv.q    R103, 0x30+%2\n"

		"vmmul.q M200, M000, M100\n"

		"sv.q    R200, 0x00+%0\n"
		"sv.q    R201, 0x10+%0\n"
		"sv.q    R202, 0x20+%0\n"
		"sv.q    R203, 0x30+%0\n"
		: "+m" (*m0) : "m" (*m1), "m" (*m2)
		);
}

void _checkSquare4(ScePspFMatrix4* m0, ScePspFMatrix4 *m1) {
	__asm__ volatile (

		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		"vmmul.q M200, M000, M000\n"

		"sv.q    R200, 0x00+%0\n"
		"sv.q    R201, 0x10+%0\n"
		"sv.q    R202, 0x20+%0\n"
		"sv.q    R203, 0x30+%0\n"
		: "+m" (*m0) : "m" (*m1)
		);
}

void checkMultiply() {
	static ScePspFMatrix4 matrix1 = {
		{ 1.0f, 5.0f,  9.0f, 13.0f },
		{ 2.0f, 6.0f, 10.0f, 14.0f },
		{ 3.0f, 7.0f, 11.0f, 15.0f },
		{ 4.0f, 8.0f, 12.0f, 16.0f }
	};
	static ScePspFMatrix4 matrix2 = {
		{ -1.0f, 20.0f, 30.0f, -4.0f },
		{ 10.0f, 2.0f, 30000.0f, 400000.0f },
		{ 100.0f, 200.0f, -3.0f, 40.0f },
		{ -1000.0f, 2.0f, -0.3f, -400.0f }
	};

	_checkMultiply2(&matrix, &matrix1, &matrix2);
	printMatrix("vmmul.p", &matrix);
	_checkMultiply4(&matrix, &matrix1, &matrix2);
	printMatrix("vmmul.q", &matrix);
	_checkSquare4(&matrix, &matrix1);
	printMatrix("vmmul.q sq", &matrix);
}

static void doTranspose(ScePspFMatrix4* m0, ScePspFMatrix4 *m1) {
	__asm__ volatile (
		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		"sv.q    C000, 0x00+%0\n"
		"sv.q    C010, 0x10+%0\n"
		"sv.q    C020, 0x20+%0\n"
		"sv.q    C030, 0x30+%0\n"
		: "+m" (*m0) : "m" (*m1)
		);
}

void checkTranspose() {
	nonsense(&matrix2);
	printMatrix("non transpose", &matrix2);
	doTranspose(&matrix, &matrix2);
	printMatrix("transpose", &matrix);
}

void doMatrixScale(ScePspFMatrix4* m0, ScePspFMatrix4 *m1) {
	asm volatile (
		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		"vmscl.q M100, E000, S200\n"

		"sv.q    R100, 0x00+%0\n"
		"sv.q    R101, 0x10+%0\n"
		"sv.q    R102, 0x20+%0\n"
		"sv.q    R103, 0x30+%0\n"
		: "+m" (*m0) : "m" (*m1)
		);
}

void checkMatrixScale() {
	nonsense(&matrix);
	doMatrixScale(&matrix2, &matrix);
	printMatrix("vmscl.q", &matrix2);
}

void _checkMatrixPerVector(ScePspFMatrix4 *m, ScePspFVector4 *vmult, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R700, 0x00+%1\n"
		"lv.q R701, 0x10+%1\n"
		"lv.q R702, 0x20+%1\n"
		"lv.q R703, 0x30+%1\n"

		"lv.q R600, 0x00+%2\n"

		"vtfm3.t R100, M700, R600\n"
		"vtfm4.q R200, M700, R600\n"

		"sv.q    R100, 0x00+%0\n"
		"sv.q    R200, 0x10+%0\n"
		: "+m" (*vresult) : "m" (*m), "m" (*vmult)
		);
}

void _checkHMatrixPerVector(ScePspFMatrix4 *matrix, ScePspFVector4 *vmult, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R700, 0x00+%1\n"
		"lv.q R701, 0x10+%1\n"
		"lv.q R702, 0x20+%1\n"
		"lv.q R703, 0x30+%1\n"

		"lv.q R600, 0x00+%2\n"

		"vhtfm3.t R100, M700, R600\n"
		"vhtfm4.q R200, M700, R600\n"

		"sv.q    R100, 0x00+%0\n"
		"sv.q    R200, 0x10+%0\n"
		: "+m" (*vresult) : "m" (*matrix), "m" (*vmult)
		);
}


void checkMatrixPerVector() {
	static ScePspFVector4 vmult = { -13.0f, -2111.0f, 33.0f, 40.0f};
	static ScePspFVector4 vout[2];

	nonsense(&matrix);

	_checkMatrixPerVector(&matrix, &vmult, vout);
	printVector("vtfm3.t", &vout[0]);
	printVector("vtfm4.q", &vout[1]);

	_checkHMatrixPerVector(&matrix, &vmult, vout);
	printVector("vhtfm3.t", &vout[0]);
	printVector("vhtfm4.q", &vout[1]);
}

void checkMultiplyFull() {
	static __attribute__ ((aligned (16))) ScePspFMatrix4 m1 = {
		{  2,  3,  5,  7 },
		{ 11, 13, 17, 19 },
		{ 23, 29, 31, 37 },
		{ 41, 43, 47, 53 },
	};

	static __attribute__ ((aligned (16))) ScePspFMatrix4 m2 = {
		{  59,  61,  67,  71 },
		{  73,  79,  83,  89 },
		{  97, 101, 103, 107 },
		{ 109, 113, 127, 131 },
	};
	
	static __attribute__ ((aligned (16))) ScePspFMatrix4 m3;
	
	__attribute__ ((aligned (16))) ScePspFVector4 *v0 = NULL, *v1 = NULL;

	v1 = &m1.x;

	asm volatile (
		"lv.q    R000, 0x00+%1\n"
		"lv.q    R001, 0x10+%1\n"
		"lv.q    R002, 0x20+%1\n"
		"lv.q    R003, 0x30+%1\n"

		: "+m" (*v0) : "m" (*v1)
	);


	v1 = &m2.x;

	asm volatile (
		"lv.q    R100, 0x00+%1\n"
		"lv.q    R101, 0x10+%1\n"
		"lv.q    R102, 0x20+%1\n"
		"lv.q    R103, 0x30+%1\n"

		: "+m" (*v0) : "m" (*v1)
	);
	
	v0 = &m3.x;

	asm volatile (
		"vmmul.q   M200, M000, M100\n"
		"sv.q R200, 0x00+%0\n"
		"sv.q R201, 0x10+%0\n"
		"sv.q R202, 0x20+%0\n"
		"sv.q R203, 0x30+%0\n"
		
		: "+m" (*v0)
	);
	
	printMatrix("vmmul.q 1", &m3);
	nonsense(&m3);

	asm volatile (
		"vmmul.t   M201, M000, M100\n"
		"sv.q R200, 0x00+%0\n"
		"sv.q R201, 0x10+%0\n"
		"sv.q R202, 0x20+%0\n"
		"sv.q R203, 0x30+%0\n"

		: "+m" (*v0)
		);

	printMatrix("vmmul.q 2", &m3);
	nonsense(&m3);

	asm volatile (
		"vmmul.p   M200, M000, M100\n"
		"vmmul.p   M202, M002, M102\n"
		"vmmul.p   M220, M020, M120\n"
		"vmmul.p   M222, M022, M122\n"
		"sv.q R200, 0x00+%0\n"
		"sv.q R201, 0x10+%0\n"
		"sv.q R202, 0x20+%0\n"
		"sv.q R203, 0x30+%0\n"

		: "+m" (*v0)
		);

	printMatrix("vmmul.q 3", &m3);
}

int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();
	
	checkMatrixPerVector();
	checkTranspose();
	checkMultiplyFull();
	checkMatrixIdentity();
	checkMatrixScale();
	
	printf("Ended\n");
	return 0;
}
