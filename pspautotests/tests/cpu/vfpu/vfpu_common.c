#include "vfpu_common.h"

void printVector(const char *title, ScePspFVector4 *v) {
	printf("%s: %f,%f,%f,%f\n", title, v->x, v->y, v->z, v->w);
}

void printVectorLowP(const char *title, ScePspFVector4 *v) {
	printf("%s: %0.3f,%0.3f,%0.3f,%0.3f\n", title, v->x, v->y, v->z, v->w);
}

void printMatrix(const char *title, ScePspFMatrix4 *m) {
	printVector(title, &m->x);
	printVector(title, &m->y);
	printVector(title, &m->z);
	printVector(title, &m->w);
}

void resetAllMatrices() {
	asm volatile (
		"vmzero.q M000\n"
		"vmzero.q M100\n"
		"vmzero.q M200\n"
		"vmzero.q M300\n"
		"vmzero.q M400\n"
		"vmzero.q M500\n"
		"vmzero.q M600\n"
		"vmzero.q M700\n"
		);
}

void nonsense(ScePspFMatrix4 *m) {
	m->x.x = 1;
	m->x.y = -2;
	m->x.z = 3;
	m->x.w = -4;
	m->y.x = 5;
	m->y.y = -6;
	m->y.z = 7;
	m->y.w = -8;
	m->z.x = 9;
	m->z.y = -10;
	m->z.z = 11;
	m->z.w = -12;
	m->w.x = 13;
	m->w.y = -14;
	m->w.z = 15;
	m->w.w = -16;
}
