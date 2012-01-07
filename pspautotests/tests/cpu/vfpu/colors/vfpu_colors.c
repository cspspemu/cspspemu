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

void vfpu_rgba8888to4444(int* in, short *out);
void vfpu_rgba8888to5551(int* in, short *out);
void vfpu_rgba8888to5650(int* in, short *out);

int main(int argc, char *argv[]) {
	int in[4];
	short out[4];
	printf("Started\n");

	resetAllMatrices();
	
	in[0] = 0xFF; in[1] = 0xFF; in[2] = 0xFF; in[3] = 0xFF;

	memset(out, 0, sizeof(out)); vfpu_rgba8888to4444(in, out); printf("vfpu_rgba8888to4444: %08X, %08X, %08X, %08X\n", out[0], out[1], out[2], out[3]);
	memset(out, 0, sizeof(out)); vfpu_rgba8888to5551(in, out); printf("vfpu_rgba8888to5551: %08X, %08X, %08X, %08X\n", out[0], out[1], out[2], out[3]);
	memset(out, 0, sizeof(out)); vfpu_rgba8888to5650(in, out); printf("vfpu_rgba8888to5650: %08X, %08X, %08X, %08X\n", out[0], out[1], out[2], out[3]);
	
	printf("Ended\n");

	return 0;
}