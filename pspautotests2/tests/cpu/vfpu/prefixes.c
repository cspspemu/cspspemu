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

void _checkPrefixes4(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000[0:1, 0:1, 0:1, 0:1], R000[0, 1, 2, 3]\n"
		"vmov.q R000[m, m, 0:1, m], R000[3, 2, 1, 0]\n"
		"vmov.q R000, R000[y, -1, z, -y]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes5(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[0, 1, 2, 3]\n"
		"vmov.q C000, C000[0, 1/2, 1/3, 1/4]\n"
		"vpfxs  -w, -z, -y, -x\n"
		"vmov.p R000, R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes6(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[0, 1, 2, 3]\n"
		"vmov.q C000, C000[0, 1/2, 1/3, 1/4]\n"
		"vpfxs  -z, -y, -y, -y\n"
		"vmov.p R000, R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixes7(ScePspFVector4* v0) {
	__asm__ volatile (
		"vmov.q R000, R000[-3, 1, 2, 3]\n"
		"vmov.q C000, C000[-3, 1/2, 1/3, 1/4]\n"
		"vpfxs  |z|, -y, -y, -y\n"
		"vmov.p R000, R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void _checkPrefixS1(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[x, x, x, x]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS2(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[y, y, y, y]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS3(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[z, z, z, z]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS4(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[w, w, w, w]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS5(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[-x, -y, -z, -w]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS6(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[|x|, |y|, |z|, |w|]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS7(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[-|x|, -|y|, -|z|, -|w|]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS8(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[0, 1, 2, 1/2]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixS9(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000, R000[3, 1/3, 1/4, 1/6]\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixD1(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000[0:1, 0:1, 0:1, 0:1], R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixD2(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000[-1:1, -1:1, -1:1, -1:1], R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void _checkPrefixD3(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R000[m, m, m, m], R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void checkPrefixes() {
	v0.x = v0.y = v0.z = v0.w = NAN;
	printf("general:\n");
	_checkPrefixes0(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes1(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes2(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes3(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkPrefixes4(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	// These are unlikely to happen and a pain, but we may want to fix them at some point.
	//_checkPrefixes5(&v0);
	//printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	//_checkPrefixes6(&v0);
	//printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	//_checkPrefixes7(&v0);
	//printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);

	v1.x = -NAN; v1.y = -INFINITY; v1.z = -0.0f; v1.w = 3.0f;
	printf("source:\n");
	_checkPrefixS1(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS2(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS3(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS4(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS5(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS6(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS7(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS8(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixS9(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);

	printf("dest:\n");
	_checkPrefixD1(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixD2(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	_checkPrefixD3(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
}

void checkVScl(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q C100, C100[-1/3, -2, -0, -3]\n"
		"vmov.q R100, R100[-1/3, -1/2, -1/4, -1/6]\n"
		"vpfxs  w, y, z, x\n"
		// Seems to do some funky things...
		"vpfxt  3, -w, -w, -w\n"
		"vscl.q R000, R000, S100\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void checkVMov(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.q R100, R100[-1/3, -1/2, -1/4, -1/6]\n"
		"vpfxt  1/3, |y|, |z|, |x|\n"
		// Does vmov consume t prefix?
		"vmov.q C200, C200\n"
		"vadd.q R000, R000, R100\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void checkLVq(ScePspFVector4* v0, ScePspFVector4* v1) {
	__asm__ volatile (
		"lv.q   R000, 0x00+%1\n"
		"vmov.s S100, S100[-1/3]\n"
		"vpfxs  3, 3, 3, 3\n"
		"vpfxt  1/3, |y|, |z|, |x|\n"
		// Does lv.q consume or apply any prefix?
		"lv.q   R000, 0x00+%1\n"
		"vadd.q R000, R000, R100\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0), "+m" (*v1)
	);
}

void checkOps() {
	v1.x = -NAN; v1.y = -INFINITY; v1.z = -0.0f; v1.w = 3.0f;
	checkVScl(&v0, &v1);
	printf("%f (%s), %f, %f, %f (%s)\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w, signbit(v0.w) ? "-" : "+");
	checkVMov(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
	checkLVq(&v0, &v1);
	printf("%f (%s), %f, %f, %f\n", v0.x, signbit(v0.x) ? "-" : "+", v0.y, v0.z, v0.w);
}

int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();

	printf("checkPrefixes:\n"); checkPrefixes();
	printf("checkOps:\n"); checkOps();
	return 0;
}
