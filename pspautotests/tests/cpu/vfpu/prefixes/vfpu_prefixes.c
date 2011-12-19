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

void _checkPrefixes0(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[1, 1, 1, 1]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes1(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[0, 1, 2, 1/2]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes2(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[3, 1/3, 1/4, 1/6]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes3(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[0, 1, 2, 3]\n"
		"vmov.q R000, R000[y, -y, z, -z]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void checkPrefixes() {
	v0.x = v0.y = v0.z = v0.w = NAN;
	_checkPrefixes0(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes1(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes2(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes3(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();

	printf("checkPrefixes:\n"); checkPrefixes();
	return 0;
}