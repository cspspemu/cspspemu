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

void vfpu_rgba8888to4444(unsigned int* in, unsigned short *out);
void vfpu_rgba8888to5551(unsigned int* in, unsigned short *out);
void vfpu_rgba8888to5650(unsigned int* in, unsigned short *out);

int main(int argc, char *argv[]) {
	unsigned int in[4];
	unsigned short out[4];
	printf("Started\n");

	resetAllMatrices();
	
	in[0] = 0xFFFF00FF; in[1] = 0x801100FF; in[2] = 0x7F5500FF; in[3] = 0x00aa00FF;

	memset(out, 0, sizeof(out)); vfpu_rgba8888to4444(in, out); printf("vfpu_rgba8888to4444: %04X, %04X, %04X, %04X\n", out[0], out[1], out[2], out[3]);
	memset(out, 0, sizeof(out)); vfpu_rgba8888to5551(in, out); printf("vfpu_rgba8888to5551: %04X, %04X, %04X, %04X\n", out[0], out[1], out[2], out[3]);
	memset(out, 0, sizeof(out)); vfpu_rgba8888to5650(in, out); printf("vfpu_rgba8888to5650: %04X, %04X, %04X, %04X\n", out[0], out[1], out[2], out[3]);
	
	printf("Ended\n");

	return 0;
}
