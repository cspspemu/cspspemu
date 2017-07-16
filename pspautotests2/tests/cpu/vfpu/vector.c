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

#include "vfpu_common.h"

const ALIGN16 ScePspFVector4 vZero;
const ALIGN16 ScePspFVector4 vMinusOne = {-1, -1, -1, -1};

ALIGN16 ScePspFVector4 v0, v1, v2;
ALIGN16 ScePspFVector4 matrix[4];

void initValues() {
	// Reset output values
	v0.x = 1001;
	v0.y = 1002;
	v0.z = 1003;
	v0.w = 1004;

	v1.x = 17;
	v1.y = 13;
	v1.z = -5;
	v1.w = 11;

	v2.x = 3;
	v2.y = -7;
	v2.z = -15;
	v2.w = 19;
}

const ALIGN16 ScePspFVector4 testVectors[8] = {
	{1.0f, 2.0f, 3.0f, 4.0f},
	{5.0f, 6.0f, -7.0f, -8.0f},
	{-1.0f, -2.0f, -3.0f, -4.0f},
	{-4.0f, -5.0f, 6.0f, 7.0f},
	{-0.1f, 1000.0f, INFINITY, 0.0f},
	{0.02f, 10000000.0f, -0.0f, NAN},
	{-NAN, INFINITY, 90.0f, -NAN},
	{NAN, -NAN, 90.0f, INFINITY},
};

#define NICE

#ifdef NICE
#define NUM_TESTVECTORS 5
#else
#define NUM_TESTVECTORS 8
#endif

void NOINLINE vcopy(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C730, %1\n"
		"vmov.q C720, C730\n"
		"sv.q   C720, %0\n"
		: "+m" (*v0) : "m" (*v1)
	);
}

void NOINLINE LoadC000(const ScePspFVector4* v0) {
	__asm__ volatile (
		"lv.q   C000, 0x00+%0\n"
		: : "m" (*v0)
	);
}
void NOINLINE LoadR000(const ScePspFVector4* v0) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%0\n"
		: : "m" (*v0)
		);
}

#define GEN_P(genFun, name) \
	genFun(name ## _p, #name ".p", "C");

#define GEN_T(genFun, name) \
	genFun(name ## _t, #name ".t", "C");

#define GEN_Q(genFun, name) \
	genFun(name ## _q, #name ".q", "C");

#define GEN_PQ(genFun, name) \
	genFun(name ## _p, #name ".p", "C"); \
	genFun(name ## _q, #name ".q", "C");

#define GEN_PTQ(genFun, name) \
	genFun(name ## _p, #name ".p", "C"); \
	genFun(name ## _t, #name ".t", "C"); \
	genFun(name ## _q, #name ".q", "C");

#define GEN_SP(genFun, name) \
	genFun(name ## _s, #name ".s", "S"); \
	genFun(name ## _p, #name ".p", "C");

#define GEN_SPTQ(genFun, name) \
	genFun(name ## _s, #name ".s", "S"); \
	genFun(name ## _p, #name ".p", "C"); \
	genFun(name ## _t, #name ".t", "C"); \
	genFun(name ## _q, #name ".q", "C");

#define TEST_P(testFun, name) \
	testFun(#name "_p", name ## _p);

#define TEST_T(testFun, name) \
	testFun(#name "_t", name ## _t);

#define TEST_Q(testFun, name) \
	testFun(#name "_q", name ## _q);

#define TEST_PQ(testFun, name) \
	testFun(#name "_p", name ## _p); \
	testFun(#name "_q", name ## _q);

#define TEST_PTQ(testFun, name) \
	testFun(#name "_p", name ## _p); \
	testFun(#name "_t", name ## _t); \
	testFun(#name "_q", name ## _q);

#define TEST_SP(testFun, name) \
	testFun(#name "_s", name ## _s); \
	testFun(#name "_p", name ## _p);

#define TEST_SPTQ(testFun, name) \
	testFun(#name "_s", name ## _s); \
	testFun(#name "_p", name ## _p); \
	testFun(#name "_t", name ## _t); \
	testFun(#name "_q", name ## _q);

//////////////////////////////////////////////////////////////////////////
// "V" functions
//////////////////////////////////////////////////////////////////////////

#define GEN_V(FuncName, Op, PFX) \
	void NOINLINE FuncName(ScePspFVector4* v0) { \
	__asm__ volatile ( \
	Op " "PFX"000\n" \
	"sv.q   C000, 0x00+%0\n" \
	: "+m" (*v0) \
	); \
}

void testV(const char *desc, void (*vxxx)(ScePspFVector4 *v0)) {
	LoadC000(&vMinusOne);
	(*vxxx)(&v0);
	printVector(desc, &v0);
}


//////////////////////////////////////////////////////////////////////////
// "VV" functions
//////////////////////////////////////////////////////////////////////////

#define GEN_VV(FuncName, Op, PFX) \
	void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	Op " " PFX "000, "PFX"100\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1) \
	); \
}

void testVV(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1)) {
	int i;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		LoadC000(&vMinusOne);
		(*vvvxxx)(&v0, &testVectors[i]);
		printVector(desc, &v0);
	}
}
void testVVLowP(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1)) {
	int i;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		LoadC000(&vZero);
		(*vvvxxx)(&v0, &testVectors[i]);
		printVectorLowP(desc, &v0);
	}
}

void testVVtrig(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1)) {
	int i;
	for (i = 0; i < 256; i++) {
		v1.x = (i - 128) / 64.0f;
		v1.y = (i - 128) / 64.0f;
		v1.z = (i - 128) / 64.0f;
		v1.w = (i - 128) / 64.0f;
		LoadC000(&vMinusOne);
		(*vvvxxx)(&v0, &v1);
		printVector(desc, &v0);
	}
}


//////////////////////////////////////////////////////////////////////////
// "SV" functions
//////////////////////////////////////////////////////////////////////////

#define GEN_SV(FuncName, Op, PFX) \
	void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	Op " S000, "PFX"100\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1) \
	); \
}

void testSV(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1)) {
	int i;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		LoadC000(&vMinusOne);
		(*vvvxxx)(&v0, &testVectors[i]);
		printVector(desc, &v0);
	}
}


//////////////////////////////////////////////////////////////////////////
// "DV" functions - destination is double size
//////////////////////////////////////////////////////////////////////////

#define GEN_DV(FuncName, Op, PFX) \
	void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	Op " C000, "PFX"100\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1) \
	); \
}

void testDV(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1)) {
	int i;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		LoadC000(&vMinusOne);
		(*vvvxxx)(&v0, &testVectors[i]);
		printVector(desc, &v0);
	}
}

//////////////////////////////////////////////////////////////////////////
// "VVV" functions
//////////////////////////////////////////////////////////////////////////

#define GEN_VVV(FuncName, Op, PFX) \
void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	"lv.q   C200, %2\n" \
	Op " " PFX "000, "PFX"100, "PFX"200\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1), "m" (*v2) \
	); \
}

void testVVV(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2)) {
	int i,j;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		for (j = 0; j < NUM_TESTVECTORS; j++) {
			LoadC000(&vZero);
			(*vvvxxx)(&v0, &testVectors[i], &testVectors[j]);
			printVector(desc, &v0);
		}
	}
}

//////////////////////////////////////////////////////////////////////////
// "SVV" functions (like dot products)
//////////////////////////////////////////////////////////////////////////

#define GEN_SVV(FuncName, Op, PFX) \
void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	"lv.q   C200, %2\n" \
	Op " S000, "PFX"100, "PFX"200\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1), "m" (*v2) \
	); \
}

void testSVV(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2)) {
	int i,j;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		for (j = 0; j < NUM_TESTVECTORS; j++) {
			LoadC000(&vZero);
			(*vvvxxx)(&v0, &testVectors[i], &testVectors[j]);
			printVector(desc, &v0);
		}
	}
}

//////////////////////////////////////////////////////////////////////////
// "VVS" functions (vscl)
//////////////////////////////////////////////////////////////////////////

#define GEN_VVS(FuncName, Op, PFX) \
void NOINLINE FuncName(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2) { \
	asm volatile ( \
	"lv.q   C100, %1\n" \
	"lv.q   C200, %2\n" \
	Op " " PFX "000, "PFX"100, S200\n" \
	"sv.q   C000, %0\n" \
	: "+m" (*v0) : "m" (*v1), "m" (*v2) \
	); \
}

void testVVS(const char *desc, void (*vvvxxx)(ScePspFVector4 *v0, const ScePspFVector4 *v1, const ScePspFVector4 *v2)) {
	int i,j;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		for (j = 0; j < NUM_TESTVECTORS; j++) {
			LoadC000(&vZero);
			(*vvvxxx)(&v0, &testVectors[i], &testVectors[j]);
			printVector(desc, &v0);
		}
	}
}


//////////////////////////////////////////////////////////////////////////
// OK! Let's run some hard autogen tests!
//////////////////////////////////////////////////////////////////////////

GEN_SPTQ(GEN_V, vzero);
GEN_SPTQ(GEN_V, vone);
void checkV() {
	TEST_SPTQ(testV, vzero);
	TEST_SPTQ(testV, vone);
}

GEN_SPTQ(GEN_VV, vmov);
GEN_SPTQ(GEN_VV, vabs);
GEN_SPTQ(GEN_VV, vneg);
GEN_SPTQ(GEN_VV, vsat0);
GEN_SPTQ(GEN_VV, vsat1);
GEN_SPTQ(GEN_VV, vrcp);
GEN_SPTQ(GEN_VV, vrsq);
GEN_SPTQ(GEN_VV, vsin);
GEN_SPTQ(GEN_VV, vcos);
GEN_SPTQ(GEN_VV, vexp2);
GEN_SPTQ(GEN_VV, vlog2);
GEN_SPTQ(GEN_VV, vsqrt);
GEN_SPTQ(GEN_VV, vasin);
GEN_SPTQ(GEN_VV, vnrcp);
GEN_SPTQ(GEN_VV, vnsin);
GEN_SPTQ(GEN_VV, vrexp2);

GEN_SPTQ(GEN_VV, vsgn);
GEN_SPTQ(GEN_VV, vocp);
GEN_PQ(GEN_VV, vbfy1);
GEN_Q(GEN_VV, vbfy2);
GEN_Q(GEN_VV, vsrt1);
GEN_Q(GEN_VV, vsrt2);
GEN_Q(GEN_VV, vsrt3);
GEN_Q(GEN_VV, vsrt4);
void checkVV() {
	TEST_SPTQ(testVV, vmov);
	TEST_SPTQ(testVV, vabs);
	TEST_SPTQ(testVV, vneg);
	TEST_SPTQ(testVV, vsat0);
	TEST_SPTQ(testVV, vsat1);
	TEST_SPTQ(testVV, vrcp);
	TEST_SPTQ(testVV, vrsq);
	TEST_SPTQ(testVVLowP, vsin);
	TEST_SPTQ(testVVLowP, vcos);
	TEST_SPTQ(testVV, vexp2);
	TEST_SPTQ(testVVLowP, vlog2);
	TEST_SPTQ(testVVLowP, vsqrt);
	TEST_SPTQ(testVVtrig, vasin);
	TEST_SPTQ(testVV, vnrcp);
	TEST_SPTQ(testVVLowP, vnsin);
	TEST_SPTQ(testVV, vrexp2);

	TEST_SPTQ(testVV, vsgn);
	TEST_SPTQ(testVV, vocp);
	TEST_PQ(testVV, vbfy1);
	TEST_Q(testVV, vbfy2);
	TEST_Q(testVV, vsrt1);
	TEST_Q(testVV, vsrt2);
	TEST_Q(testVV, vsrt3);
	TEST_Q(testVV, vsrt4);
}

GEN_SP(GEN_DV, vsocp);
// GEN_PQ(GEN_VV, vsocp); can't get as to recognize
void checkDV() {
	TEST_SP(testDV, vsocp);
}

GEN_PTQ(GEN_SV, vavg);
GEN_PTQ(GEN_SV, vfad);
// GEN_PQ(GEN_VV, vsocp); can't get as to recognize
void checkSV() {
	TEST_PTQ(testSV, vavg);
	TEST_PTQ(testSV, vfad);
}

GEN_SPTQ(GEN_VVV, vadd);
GEN_SPTQ(GEN_VVV, vsub);
GEN_SPTQ(GEN_VVV, vmul);
GEN_SPTQ(GEN_VVV, vdiv);
GEN_SPTQ(GEN_VVV, vsge);
GEN_SPTQ(GEN_VVV, vslt);
GEN_SPTQ(GEN_VVV, vmin);
GEN_SPTQ(GEN_VVV, vmax);
GEN_T(GEN_VVV, vcrs);
GEN_T(GEN_VVV, vcrsp);
GEN_Q(GEN_VVV, vqmul);
GEN_SPTQ(GEN_VVV, vscmp);

void checkVVV() {
	TEST_SPTQ(testVVV, vadd);
	TEST_SPTQ(testVVV, vsub);
	TEST_SPTQ(testVVV, vmul);
	TEST_SPTQ(testVVV, vdiv);
	TEST_SPTQ(testVVV, vsge);
	TEST_SPTQ(testVVV, vslt);
	TEST_SPTQ(testVVV, vmin);
	TEST_SPTQ(testVVV, vmax);
	TEST_T(testVVV, vcrs);
	TEST_T(testVVV, vcrsp);
	TEST_Q(testVVV, vqmul);
	TEST_SPTQ(testVVV, vscmp);
}

GEN_PTQ(GEN_SVV, vdot);
GEN_PTQ(GEN_SVV, vhdp);
GEN_P(GEN_SVV, vdet);
void checkSVV() {
	TEST_PTQ(testSVV, vdot);
	TEST_PTQ(testSVV, vhdp);
	TEST_P(testSVV, vdet);
}

GEN_PTQ(GEN_VVS, vscl);
void checkVVS() {
	TEST_PTQ(testVVS, vscl);
}

void NOINLINE vfim(ScePspFVector4 *v0) {
	asm volatile (
		"vfim.s	 s500, 0.011111111111111112\n"
		"vfim.s	 s501, -0.011111111111111112\n"
		"vfim.s	 s502, inf\n"
		"vfim.s	 s503, nan\n"
		"sv.q    C500, %0\n"

		: "+m" (*v0)
	);
}


void checkConstants() {
	static ALIGN16 ScePspFVector4 v[5];
	int n;
	asm volatile(
		"vcst.s S000, VFPU_HUGE\n"
		"vcst.s S010, VFPU_SQRT2\n"
		"vcst.s S020, VFPU_SQRT1_2\n"
		"vcst.s S030, VFPU_2_SQRTPI\n"
		"vcst.s S001, VFPU_2_PI\n"
		"vcst.s S011, VFPU_1_PI\n"
		"vcst.s S021, VFPU_PI_4\n"
		"vcst.s S031, VFPU_PI_2\n"
		"vcst.s S002, VFPU_PI\n"
		"vcst.s S012, VFPU_E\n"
		"vcst.s S022, VFPU_LOG2E\n"
		"vcst.s S032, VFPU_LOG10E\n"
		"vcst.s S003, VFPU_LN2\n"
		"vcst.s S013, VFPU_LN10\n"
		"vcst.s S023, VFPU_2PI\n"
		"vcst.s S033, VFPU_PI_6\n"
		"vcst.s S100, VFPU_LOG10TWO\n"
		"vcst.s S110, VFPU_LOG2TEN\n"
		"vcst.s S120, VFPU_SQRT3_2\n"
		"viim.s S130, 0\n"
		"sv.q   R000, 0x00+%0\n"
		"sv.q   R001, 0x10+%0\n"
		"sv.q   R002, 0x20+%0\n"
		"sv.q   R003, 0x30+%0\n"
		"sv.q   R100, 0x40+%0\n"
		: "+m" (v[0])
	);
	for (n = 0; n < 5; n++) {
		printVector("vcst.s", &v[n]);
	}
}

void NOINLINE checkViim() {
	int n;
	static ALIGN16 ScePspFVector4 v[4];

	asm volatile(
		"viim.s S000, 0\n"
		"viim.s S010, 1\n"
		"viim.s S020, -3\n"
		"viim.s S030, 777\n"
		"viim.s S001, 32767\n"
		"viim.s S002, -32768\n"
		"viim.s S003, -0\n"
		"viim.s S011, -8\n"
		"viim.s S021, -3\n"
		"viim.s S031, -1\n"
		"sv.q   R000, 0x00+%0\n"
		"sv.q   R001, 0x10+%0\n"
		"sv.q   R002, 0x20+%0\n"
		"sv.q   R003, 0x30+%0\n"
		: "+m" (v[0])
	);

	for (n = 0; n < 4; n++) {
		printVector("viim", &v[n]);
	}
}

void checkVectorCopy() {
	initValues();
	vcopy(&v0, &v1);
	printVector("vmov", &v0);
}

void checkVfim() {
	initValues();
	vfim(&v0);
	printVector("vfim", &v0);
}

void NOINLINE vrot(float angle, ScePspFVector4 *v0) {
	asm volatile (
		"mtv    %1, s601\n"
		"vrot.q	R000, s601, [c, s, s, s]\n"
		"vrot.q	R001, s601, [s, c, 0, 0]\n"
		"vrot.q	R002, s601, [s, 0, c, 0]\n"
		"vrot.q	R003, s601, [s, 0, 0, c]\n"
		"vrot.q	R100, s601, [c, s, 0, 0]\n"
		"vrot.q	R101, s601, [s, c, s, s]\n"
		"vrot.q	R102, s601, [0, s, c, 0]\n"
		"vrot.q	R103, s601, [0, s, 0, c]\n"
		"vrot.q	R200, s601, [c, 0, s, 0]\n"
		"vrot.q	R201, s601, [0, c, s, 0]\n"
		"vrot.q	R202, s601, [s, s, c, s]\n"
		"vrot.q	R203, s601, [0, 0, s, c]\n"
		"vrot.q	R300, s601, [c, 0, 0, s]\n"
		"vrot.q	R301, s601, [0, c, 0, s]\n"
		"vrot.q	R302, s601, [0, 0, c, s]\n"
		"vrot.q	R303, s601, [-s, -s, -s, c]\n"
		"vrot.q	R400, s601, [c, -s, -s, -s]\n"
		"vrot.q	R401, s601, [-s, c, 0, 0]\n"
		"vrot.q	R402, s601, [-s, 0, c, 0]\n"
		"vrot.q	R403, s601, [-s, 0, 0, c]\n"
		"vrot.q	R500, s601, [c, -s, 0, 0]\n"
		"vrot.q	R501, s601, [-s, c, -s, -s]\n"
		"vrot.q	R502, s601, [0, -s, c, 0]\n"
		"vrot.q	R503, s601, [0, -s, 0, c]\n"
		"mtv    %1, s501\n"
		"vrot.q	R600, s501, [c, 0, -s, 0]\n"
		"vrot.q	R601, s501, [0, c, -s, 0]\n"
		"vrot.q	R602, s501, [-s, -s, c, -s]\n"
		"vrot.q	R603, s501, [0, 0, -s, c]\n"
		"vrot.q	R700, s501, [c, 0, 0, -s]\n"
		"vrot.q	R701, s501, [0, c, 0, -s]\n"
		"vrot.q	R702, s501, [0, 0, c, -s]\n"
		"vrot.q	R703, s501, [-s, -s, -s, c]\n"
		"sv.q   R000, 0x00+%0\n"
		"sv.q   R001, 0x10+%0\n"
		"sv.q   R002, 0x20+%0\n"
		"sv.q   R003, 0x30+%0\n"
		"sv.q   R100, 0x40+%0\n"
		"sv.q   R101, 0x50+%0\n"
		"sv.q   R102, 0x60+%0\n"
		"sv.q   R103, 0x70+%0\n"
		"sv.q   R200, 0x80+%0\n"
		"sv.q   R201, 0x90+%0\n"
		"sv.q   R202, 0xA0+%0\n"
		"sv.q   R203, 0xB0+%0\n"
		"sv.q   R300, 0xC0+%0\n"
		"sv.q   R301, 0xD0+%0\n"
		"sv.q   R302, 0xE0+%0\n"
		"sv.q   R303, 0xF0+%0\n"
		"sv.q   R400, 0x100+%0\n"
		"sv.q   R401, 0x110+%0\n"
		"sv.q   R402, 0x120+%0\n"
		"sv.q   R403, 0x130+%0\n"
		"sv.q   R500, 0x140+%0\n"
		"sv.q   R501, 0x150+%0\n"
		"sv.q   R502, 0x160+%0\n"
		"sv.q   R503, 0x170+%0\n"
		"sv.q   R600, 0x180+%0\n"
		"sv.q   R601, 0x190+%0\n"
		"sv.q   R602, 0x1A0+%0\n"
		"sv.q   R603, 0x1B0+%0\n"
		"sv.q   R700, 0x1C0+%0\n"
		"sv.q   R701, 0x1D0+%0\n"
		"sv.q   R702, 0x1E0+%0\n"
		"sv.q   R703, 0x1F0+%0\n"
		: "+m" (*v0) : "r" (angle)
		);
}

void checkRotation() {
	static ALIGN16 ScePspFVector4 v[32];
	int i;
	vrot(0.7, &v[0]);
	for (i = 0; i < 32; i++)
		printVectorLowP("vrot", &v[i]);
	vrot(-1.1, &v[0]);
	for (i = 0; i < 32; i++)
		printVectorLowP("vrot", &v[i]);
}

void moveNormalRegister() {
	float t = 5.0;
	//int t2 = *(int *)&t;
	static ScePspFVector4 v[1];
	asm volatile(
		"mtv %1, S410\n"
		"mtv %1, S411\n"
		"mtv %1, S412\n"
		"mtv %1, S413\n"
		"sv.q   C410, 0x00+%0\n"
		: "+m" (v[0]) : "t" (t)
	);
	printVector("moveNormalRegister", &v0);
}

void _checkLoadUnaligned1(ScePspFVector4* v0, int index, int column) {
	float list[64] = {0.0f};
	float *vec = &list[index];
	int n;
	for (n = 0; n < 64; n++) {
		list[n] = -(float)n;
	}

	if (column) {
		__asm__ volatile (
			"vmov.q C000, R000[0, 0, 0, 0]\n"
			"lvl.q C000, %1\n"
			"sv.q   C000, 0x00+%0\n"
			: "+m" (*v0) : "m" (*vec)
		);
	} else {
		__asm__ volatile (
			"vmov.q R000, R000[0, 0, 0, 0]\n"
			"lvl.q  R000, %1\n"
			"sv.q   R000, 0x00+%0\n"
			: "+m" (*v0) : "m" (*vec)
		);
	}
}

void _checkLoadUnaligned2(ScePspFVector4* v0, int index, int column) {
	float list[64] = {0.0f};
	float *vec = &list[index];
	int n;
	for (n = 0; n < 64; n++) {
		list[n] = -(float)n;
	}

	if (column) {
		__asm__ volatile (
			"vmov.q C000, R000[0, 0, 0, 0]\n"
			"lvr.q C000, %1\n"
			"sv.q   C000, 0x00+%0\n"
			: "+m" (*v0) : "m" (*vec)
		);
	} else {
		__asm__ volatile (
			"vmov.q R000, R000[0, 0, 0, 0]\n"
			"lvr.q  R000, %1\n"
			"sv.q   R000, 0x00+%0\n"
			: "+m" (*v0) : "m" (*vec)
		);
	}
}

void checkLoadUnaligned() {
	_checkLoadUnaligned1(&v0, 13, 0);
	printVector("lvl_row", &v0);
	_checkLoadUnaligned1(&v0, 24, 0);
	printVector("lvl_row", &v0);
	_checkLoadUnaligned1(&v0, 32, 0);
	printVector("lvl_row", &v0);

	_checkLoadUnaligned1(&v0, 15, 1);
	printVector("lvl_column", &v0);
	_checkLoadUnaligned1(&v0, 23, 1);
	printVector("lvl_column", &v0);
	_checkLoadUnaligned1(&v0, 31, 1);
	printVector("lvl_column", &v0);
	
	_checkLoadUnaligned2(&v0, 13, 0);
	printVector("lvr_row", &v0);
	_checkLoadUnaligned2(&v0, 24, 0);
	printVector("lvr_row", &v0);
	_checkLoadUnaligned2(&v0, 32, 0);
	printVector("lvr_row", &v0);

	_checkLoadUnaligned2(&v0, 15, 1);
	printVector("lvr_column", &v0);
	_checkLoadUnaligned2(&v0, 23, 1);
	printVector("lvr_column", &v0);
	_checkLoadUnaligned2(&v0, 31, 1);
	printVector("lvr_column", &v0);

}

void checkMisc() {
	float fovy = 75.0f;
	
	ALIGN16 ScePspFVector4 v;
	ScePspFVector4 *v0 = &v;
	
	resetAllMatrices();
	
	__asm__ volatile (
    "vmzero.q M100\n"                   // set M100 to all zeros
    "mtv     %1, S000\n"                // S000 = fovy
    "viim.s  S001, 90\n"                // S002 = 90.0f
    "vrcp.s  S001, S001\n"              // S002 = 1/90
    "vmul.s  S000, S000, S000[1/2]\n"   // S000 = fovy * 0.5 = fovy/2
    "vmul.s  S000, S000, S001\n"        // S000 = (fovy/2)/90
		"sv.q   C000, %0\n"
		: "+m" (*v0) : "r"(fovy)
	);
	
	printVector("misc", v0);
}

void NOINLINE _checkSimpleLoad(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.s   S000, 0x00+%1\n"
		"sv.s   S000, 0x00+%0\n"
		: "+m" (*v0) : "m" (*v1)
	);
}

void NOINLINE checkSimpleLoad() {
	ALIGN16 ScePspFVector4 vIn = {0.0f, 0.0f, 0.0f, 0.0f};
	ALIGN16 ScePspFVector4 vOut = {0.0f, 0.0f, 0.0f, 0.0f};
	vIn.x = 0.3f;
	_checkSimpleLoad(&vOut, &vIn);
	printVector("simpleLoad", &vOut);
}

void NOINLINE _checkCompare(ScePspFVector4 *vleft, ScePspFVector4 *vright, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R500, 0x00+%1\n"
		"lv.q R600, 0x00+%2\n"
		
		"vcmp.t	EQ,  R500, R600\n"
		
		"vfim.s	  S100, -0\n"
		"vfim.s	  S110, -0\n"
		"vfim.s	  S120, -0\n"
		"vfim.s	  S130, -0\n"
		
		"vcmovt.s S100, S000[1], 0\n"
		"vcmovt.s S110, S000[1], 1\n"
		"vcmovt.s S120, S000[1], 2\n"
		"vcmovt.s S130, S000[1], 3\n"
		
		"sv.q    R100, 0x00+%0\n"
		: "+m" (*vresult) : "m" (*vleft), "m" (*vright)
	);
}

void NOINLINE _checkVwbn1(ScePspFVector4 *v0, ScePspFVector4 *vec) {
    __asm__ volatile (
        "lv.q C000, %1\n"
        "vwbn.s S000, S000, 0\n"
        "vwbn.s S001, S000, 8\n"
        "vwbn.s S002, S000, 16\n"
        "vwbn.s S003, S000, 32\n"
        "vwbn.s S010, S000, 64\n"
        "vwbn.s S011, S000, 128\n"
        "vwbn.s S012, S000, 144\n"
        "vwbn.s S013, S000, 192\n"
        "vwbn.s S020, S000, 125\n"
        "vwbn.s S021, S000, 126\n"
        "vwbn.s S022, S000, 127\n"
        "vwbn.s S023, S000, 128\n"
        "vwbn.s S030, S000, 130\n"
        "vwbn.s S031, S000, 144\n"
        "vwbn.s S032, S000, 192\n"
        "vwbn.s S033, S000, 255\n"
        "sv.q C000, 0x00+%0\n"
        "sv.q C010, 0x10+%0\n"
        "sv.q C020, 0x20+%0\n"
        "sv.q C030, 0x30+%0\n"
        : "+m" (*v0) : "m" (*vec)
    );
}

void checkVwbn() {
    static ALIGN16 ScePspFVector4 vin = { 1.0f, 0.0f, 0.0f, 0.0f };
    static ALIGN16 ScePspFVector4 vout[4] = { {0.0f, 0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f, 0.0f} };
    int i;
    const float testValues[16] = {
        1E-20f, 1, 100, 100000000,
        2, 3, 4, 5,
        100, 102, 104, 109,
        -100, -10000000, -0.001, -1
    };

    for (i = 0; i < 16; i++) {
        vin.x = testValues[i];
        _checkVwbn1(&vout, &vin);
        printVector("checkVwbn 0", &vout[0]);
        printVector("checkVwbn 1", &vout[1]);
        printVector("checkVwbn 2", &vout[2]);
        printVector("checkVwbn 3", &vout[3]);
    }
}

void checkCompare() {
	static ALIGN16 ScePspFVector4 vleft  = { 1.0f, -1.0f, -1.1f, 2.0f };
	static ALIGN16 ScePspFVector4 vright = { 1.0f,  1.0f, -1.1f, 2.1f };
	static ALIGN16 ScePspFVector4 vout = { 0.0f, 0.0f, 0.0f, 0.0f };
	int i, j;
	for (i = 0; i < NUM_TESTVECTORS; i++) {
		for (j = 0; j < NUM_TESTVECTORS; j++) {
			_checkCompare(&vleft, &vright, &vout);
			printVector("checkCompare", &vout);
		}
	}
}

const char *cmpNames[16] = {
  "FL",
  "EQ",
  "LT",
  "LE",
  "TR",
  "NE",
  "GE",
  "GT",
  "EZ",
  "EN",
  "EI",
  "ES",
  "NZ",
  "NN",
  "NI",
  "NS",
};

void NOINLINE _checkCompare2(float a, float b) {
 	static ALIGN16 ScePspFVector4 vleft;
	static ALIGN16 ScePspFVector4 vright;
 	ScePspFVector4 *vLeft = &vleft;
 	ScePspFVector4 *vRight = &vright;
  int i;
  for (i = 0; i < 4; i++) {
    memcpy(((float*)&vleft) + i, &a, 4);
    memcpy(((float*)&vright) + i, &b, 4);
  }
  int temp;
  unsigned int res[16];
  printf("checkCompare2: === Comparing %f, %f ===\n", a, b);
  memset(res, 0, sizeof(res));
  asm volatile(
		"lv.q R500, 0x00+%1\n"
		"lv.q R600, 0x00+%2\n"
    "li %3, 0\n"
    "mtvc %3, $131\n"

		// $131 = VFPU_CC
    // The vmul is necessary to resolve the hazard of reading VFPU_CC immediately after writing it.
    // Apparently, Sony didn't bother to implement proper interlocking.
    "vcmp.q FL, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 0+%0\n"
    "vcmp.q EQ, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 4+%0\n"
    "vcmp.q LT, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 8+%0\n"
    "vcmp.q LE, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 12+%0\n"

    "vcmp.q TR, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 16+%0\n"
    "vcmp.q NE, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 20+%0\n"
    "vcmp.q GE, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 24+%0\n"
    "vcmp.q GT, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 28+%0\n"

    "vcmp.q EZ, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 32+%0\n"
    "vcmp.q EN, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 36+%0\n"
    "vcmp.q EI, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 40+%0\n"
    "vcmp.q ES, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 44+%0\n"
    
    "vcmp.q NZ, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 48+%0\n"
    "vcmp.q NN, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 52+%0\n"
    "vcmp.q NI, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 56+%0\n"
    "vcmp.q NS, R500, R600\n" "vmul.q R700, R701, R702\n" "mfvc %3, $131\n" "sw %3, 60+%0\n"
    : "=m"(res[0]) : "m"(*vLeft), "m"(*vRight), "r"(temp)
  );
  for (i = 0; i < 16; i++) {
    if (0) {
      //simple mode, only condition flag
      printf("checkCompare2: %f %f %s: %s\n", a, b, cmpNames[i], ((res[i]>>23)&1) ? "T" : "F");
    } else {
      printf("checkCompare2: %f %f %s: %08x\n", a, b, cmpNames[i], res[i]);
    }
  }
}

void checkCompare2() {
	const float numbers[12] = {
		0.0f, 1.0f, 1.00000001f, -1.0f,
		0.5f, 2.0f, -2.0f, 10000.0f,
		INFINITY, NAN, -INFINITY, -NAN
	};
	int i,j;
	for (i = 0; i < 12; i++) {
		for (j = 0; j < 12; j++) {
			_checkCompare2(numbers[i], numbers[j]);
		}
	}
}


int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();

	checkV();
	checkVV();
	checkSV();
	checkVVV();
	checkSVV();
	checkVVS();

	checkCompare();
	checkCompare2();
	checkSimpleLoad();
	checkMisc();
	checkViim();
	checkLoadUnaligned();
	moveNormalRegister();
	checkVfim();
	checkConstants();
	checkVectorCopy();
	checkRotation();
    checkVwbn();

	printf("Ended\n");
	return 0;
}
