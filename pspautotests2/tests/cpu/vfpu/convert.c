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

__attribute__ ((aligned (16))) ScePspFVector4 v0, v1, v2;
__attribute__ ((aligned (16))) ScePspFVector4 matrix[4];

void __attribute__((noinline)) vfim(ScePspFVector4 *v0) {
	asm volatile (
		"vfim.s	 s500, 0.011111111111111112\n"
		"vfim.s	 s501, -0.011111111111111112\n"
		"vfim.s	 s502, inf\n"
		"vfim.s	 s503, nan\n"
		"sv.q    C500, %0\n"

		: "+m" (*v0)
		);
}

void __attribute__((noinline)) vf2h(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vf2h.q C200, C100\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vh2f(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vh2f.p C200, C100\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vf2id(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vf2id.q C200, C100, 0\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vf2in(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vf2in.q C200, C100, 0\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vf2iu(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vf2iu.q C200, C100, 0\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vf2iz(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vf2iz.q C200, C100, 0\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}

void __attribute__((noinline)) vi2f(ScePspFVector4 *v0, int *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"vi2f.q C200, C100, 0\n"
		"sv.q   C200, %0\n"

		: "+m" (*v0) : "m" (*v1)
		);
}


void checkHalf() {
	static __attribute__ ((aligned (16))) ScePspFVector4 vIn1 =
	{0.9f, 1.3f, 2.7f, 1000.5f};
	static __attribute__ ((aligned (16))) ScePspFVector4 vIn2 =
	{-0.9f, -1.3f, -2.7f, -1000.5f};
	static __attribute__ ((aligned (16))) ScePspFVector4 vIn3 =
	{1.5f, 2.5f, -3.5f, -4.5f};
	static __attribute__ ((aligned (16))) ScePspFVector4 vIn4 =
	{3.5f, INFINITY, -INFINITY, NAN};

	static __attribute__ ((aligned (16))) ScePspFVector4 vOutF =
	{0.0f, 0.0f, 0.0f, 0.0f};

	struct {unsigned int x,y,z,w;} vOut;

	vf2h(&vOutF, &vIn1); memcpy(&vOut, &vOutF, 16);
	printf("vf2h: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vf2h(&vOutF, &vIn2); memcpy(&vOut, &vOutF, 16);
	printf("vf2h: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vf2h(&vOutF, &vIn3); memcpy(&vOut, &vOutF, 16);
	printf("vf2h: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vf2h(&vOutF, &vIn4); memcpy(&vOut, &vOutF, 16);
	printf("vf2h: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);

	vh2f(&vOutF, &vIn1); memcpy(&vOut, &vOutF, 16);
	printf("vh2f: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vh2f(&vOutF, &vIn2); memcpy(&vOut, &vOutF, 16);
	printf("vh2f: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vh2f(&vOutF, &vIn3); memcpy(&vOut, &vOutF, 16);
	printf("vh2f: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vh2f(&vOutF, &vIn4); memcpy(&vOut, &vOutF, 16);
	printf("vh2f: %08x,%08x,%08x,%08x\n", vOut.x, vOut.y, vOut.z, vOut.w);

	vf2h(&vOutF, &vIn1); vh2f(&vOutF, &vOutF);
	printf("vf2h vh2f: %f,%f,%f,%f\n", vOutF.x, vOutF.y, vOutF.z, vOutF.w);
	vf2h(&vOutF, &vIn2); vh2f(&vOutF, &vOutF);
	printf("vf2h vh2f: %f,%f,%f,%f\n", vOutF.x, vOutF.y, vOutF.z, vOutF.w);
	vf2h(&vOutF, &vIn3); vh2f(&vOutF, &vOutF);
	printf("vf2h vh2f: %f,%f,%f,%f\n", vOutF.x, vOutF.y, vOutF.z, vOutF.w);
	vf2h(&vOutF, &vIn4); vh2f(&vOutF, &vOutF);
	printf("vf2h vh2f: %f,%f,%f,%f\n", vOutF.x, vOutF.y, vOutF.z, vOutF.w);
}


void checkVF2I() {
	static __attribute__ ((aligned (16))) ScePspFVector4 vIn[8] =
	{
		{0.0f, 0.1f, 0.5f, 0.9f}, 
		{1.0f, 1.1f, 1.5f, 1.9f},
		{2.0f, 2.5f, 3.5f, 1000.0f},
		{INFINITY, NAN, 0.0f, 0.0f},
		{-0.0f, -0.1f, -0.5f, -0.9f},
		{-1.0f, -1.1f, -1.5f, -1.9f},
		{-2.0f, -2.5f, -3.5f, -1000.0f},
		{-INFINITY, -NAN},
	};
	
	static __attribute__ ((aligned (16))) ScePspFVector4 vOutF =
	{0.0f, 0.0f, 0.0f, 0.0f};

	struct {int x,y,z,w;} vOut;
	int i;
	for (i = 0; i < 8; i++) {
		vf2id(&vOutF, &vIn[i]); memcpy(&vOut, &vOutF, 16);
		printf("vf2id: %i,%i,%i,%i\n", vOut.x, vOut.y, vOut.z, vOut.w);
		vf2in(&vOutF, &vIn[i]); memcpy(&vOut, &vOutF, 16);
		printf("vf2in: %i,%i,%i,%i\n", vOut.x, vOut.y, vOut.z, vOut.w);
		vf2iz(&vOutF, &vIn[i]); memcpy(&vOut, &vOutF, 16);
		printf("vf2iz: %i,%i,%i,%i\n", vOut.x, vOut.y, vOut.z, vOut.w);
		vf2iu(&vOutF, &vIn[i]); memcpy(&vOut, &vOutF, 16);
		printf("vf2iu: %i,%i,%i,%i\n", vOut.x, vOut.y, vOut.z, vOut.w);
	}
}

void checkVI2F() {
	static __attribute__ ((aligned (16))) int vIn1[4] =
	{0, 0xFFFFFFFF, 3, 0x80000000};
	static __attribute__ ((aligned (16))) int vIn2[4] =
	{-1, -2, -3, 0x10000};

	static __attribute__ ((aligned (16))) ScePspFVector4 vOut =
	{0.0f, 0.0f, 0.0f, 0.0f};

	vi2f(&vOut, vIn1);
	printf("vi2f: %f,%f,%f,%f\n", vOut.x, vOut.y, vOut.z, vOut.w);
	vi2f(&vOut, vIn2);
	printf("vi2f: %f,%f,%f,%f\n", vOut.x, vOut.y, vOut.z, vOut.w);
}

void vfpu_vc2i_s(int* in, int *out);
void vfpu_vuc2i_s(int* in, int *out);
void vfpu_vs2i_s(int* in, int *out);
void vfpu_vs2i_p(int* in, int *out);
void vfpu_vi2f_q(int* in, float *out);

void test_vc2i() {
	int in_i[4];
	int out_i[4];
	resetAllMatrices();
	
	in_i[0] = 0x01234567;
	in_i[1] = 0x89ABCDEF;
	in_i[2] = 0x01234567;
	in_i[3] = 0x89ABCDEF;

	memset(out_i, 0, sizeof(out_i)); vfpu_vc2i_s(in_i, out_i); printf("vfpu_vc2i_s: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);
}

void test_vuc2i() {
	int in_i[4];
	int out_i[4];
	resetAllMatrices();
	
	in_i[0] = 0x01234567;
	in_i[1] = 0x89ABCDEF;
	in_i[2] = 0x01234567;
	in_i[3] = 0x89ABCDEF;

	memset(out_i, 0, sizeof(out_i)); vfpu_vuc2i_s(in_i, out_i); printf("vfpu_vuc2i_s: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);

	in_i[0] = 0xFEDCBA89;
	in_i[1] = 0x00000000;
	in_i[2] = 0x00000000;
	in_i[3] = 0x00000000;

	memset(out_i, 0, sizeof(out_i)); vfpu_vuc2i_s(in_i, out_i); printf("vfpu_vuc2i_s: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);
}

void test_vs2i() {
	int in_i[4];
	int out_i[4];
	resetAllMatrices();
	
	in_i[0] = 0x01234567;
	in_i[1] = 0x89ABCDEF;

	memset(out_i, 0, sizeof(out_i)); vfpu_vs2i_s(in_i, out_i); printf("vfpu_vs2i_s: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);
	memset(out_i, 0, sizeof(out_i)); vfpu_vs2i_p(in_i, out_i); printf("vfpu_vs2i_p: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);

  in_i[0] = 0xFCCF00FF;
	in_i[1] = 0x1100FF11;

	memset(out_i, 0, sizeof(out_i)); vfpu_vs2i_s(in_i, out_i); printf("vfpu_vs2i_s: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);
	memset(out_i, 0, sizeof(out_i)); vfpu_vs2i_p(in_i, out_i); printf("vfpu_vs2i_p: %08X, %08X, %08X, %08X\n", out_i[0], out_i[1], out_i[2], out_i[3]);
}

void test_vi2f() {
	int in_i[4];
	float out_f[4];
	resetAllMatrices();
	
	in_i[0] = 1;
	in_i[1] = 10;
	in_i[2] = 9999;
	in_i[3] = 1693234;

	out_f[0] = out_f[1] = out_f[2] = out_f[3] = 0.0f; vfpu_vi2f_q(in_i, out_f); printf("vfpu_vi2f_q: %f, %f, %f, %f\n", out_f[0], out_f[1], out_f[2], out_f[3]);
}

int main(int argc, char *argv[]) {
	printf("Started\n");
	{
		test_vc2i();
		test_vuc2i();
		test_vs2i();
		test_vi2f();

		printf("checkVF2I:\n"); checkVF2I();
		printf("checkVI2F:\n"); checkVI2F();
		printf("checkHalf:\n"); checkHalf();
	}
	printf("Ended\n");

	return 0;
}
