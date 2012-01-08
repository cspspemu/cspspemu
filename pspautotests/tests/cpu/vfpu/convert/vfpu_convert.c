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

ScePspFVector4 v0, v1, v2;
ScePspFVector4 matrix[4];

void resetAllMatrices() {
	asm volatile (
		"vmzero.q  M000\n"
		"vmzero.q  M100\n"
		"vmzero.q  M200\n"
		"vmzero.q  M300\n"
		"vmzero.q  M400\n"
		"vmzero.q  M500\n"
		"vmzero.q  M600\n"
		"vmzero.q  M700\n"
	);
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
}

void test_vs2i() {
	int in_i[4];
	int out_i[4];
	resetAllMatrices();
	
	in_i[0] = 0x01234567;
	in_i[1] = 0x89ABCDEF;

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
	}
	printf("Ended\n");

	return 0;
}