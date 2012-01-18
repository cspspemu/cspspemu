/*
FPU Test. Originally from jpcsp project:
http://code.google.com/p/jpcsp/source/browse/trunk/demos/src/fputest/main.c
Modified to perform automated tests.
*/

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

void __attribute__((noinline)) vcopy(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q   C100, %1\n"
		"sv.q   C100, %0\n"

		: "+m" (*v0) : "m" (*v1)
	);
}

void __attribute__((noinline)) vrot(float angle, ScePspFVector4 *v0) {
	asm volatile (
		"mtv    %1, s501\n"

		"vrot.p	r500, s501, [c, s]\n"
		
		"sv.q   r500, %0\n"

		: "+m" (*v0) : "r" (angle)
	);
}

void __attribute__((noinline)) vdotq(ScePspFVector4 *v0, ScePspFVector4 *v1, ScePspFVector4 *v2) {
	asm volatile (
		"lv.q   C100, %1\n"
		"lv.q   C200, %2\n"
		"vdot.q S000, C100, C200\n"
		"sv.q   C000, %0\n"

		: "+m" (*v0) : "m" (*v1), "m" (*v2)
	);
}

void __attribute__((noinline)) vsclq(ScePspFVector4 *v0, ScePspFVector4 *v1, ScePspFVector4 *v2) {
	asm volatile (
		"lv.q   C100, %1\n"
		"lv.q   C200, %2\n"
		"vscl.q C300, C100, S200\n"
		"sv.q   C300, %0\n"

		: "+m" (*v0) : "m" (*v1), "m" (*v2)
	);
}

void __attribute__((noinline)) vmidt(int size, ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.q    R000, %1\n"
		"lv.q    R001, %1\n"
		"lv.q    R002, %1\n"
		"lv.q    R003, %1\n"

		: "+m" (*v0) : "m" (*v1)
	);
	
	switch (size) {
		case 2: asm volatile("vmidt.p M000\n"); break;
		case 3: asm volatile("vmidt.t M000\n"); break;
		case 4: asm volatile("vmidt.q M000\n"); break;
	}

	asm volatile (
		"sv.q    R000, 0x00+%0\n"
		"sv.q    R001, 0x10+%0\n"
		"sv.q    R002, 0x20+%0\n"
		"sv.q    R003, 0x30+%0\n"

		: "+m" (*v0) : "m" (*v1)
	);
}

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


ScePspFVector4 v0, v1, v2;
ScePspFVector4 matrix[4];

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

void checkMatrixIdentity() {
	int vsize, x, y;
	ScePspFVector4 matrix2[4];
	for (vsize = 2; vsize <= 4; vsize++) {
		v0.x = 100;
		v0.y = 101;
		v0.z = 102;
		v0.w = 103;
		vmidt(vsize, &matrix[0], &v0);
		for (y = 0; y < 4; y++) {
			matrix2[y] = v0;
			for (x = 0; x < 4; x++) {
				if (x < vsize && y < vsize) {
					((float *)&matrix2[y])[x] = (float)(x == y);
				}
			}
		}
		/*
		printf("-------------------\n");
		for (y = 0; y < 4; y++) printf("(%3.0f, %3.0f, %3.0f, %3.0f)\n", matrix[y].x, matrix[y].y, matrix[y].z, matrix[y].w);
		printf("+\n");
		for (y = 0; y < 4; y++) printf("(%3.0f, %3.0f, %3.0f, %3.0f)\n", matrix2[y].x, matrix2[y].y, matrix2[y].z, matrix2[y].w);
		printf("\n");
		*/
		printf("%d\n", memcmp((void *)&matrix, (void *)&matrix2, sizeof(matrix)));
	}
	
	//printf("Test! %f, %f, %f, %f, %f, %f, %f, %f, %f, %f\n", 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0);
	//printf("Test! %d, %d, %d, %d\n", 1, 2, 3, -3);
}

void checkConstants() {
	ScePspFVector4 v[5];
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
		: "+m" (v)
	);
	char buf[1024];
	char temp[1024];
	buf[0] = 0;
	for (n = 0; n < 5; n++) {
		sprintf(temp, "%f,%f,%f,%f\n", v[n].x, v[n].y, v[n].z, v[n].w);
		strcat(buf, temp);
	}
	//printf("%s", buf);
	printf("checkConstants(Comparison): %s\n", (strcmp(buf,
		"inf,1.414214,0.707107,1.128379\n"
		"0.636620,0.318310,0.785398,1.570796\n"
		"3.141593,2.718282,1.442695,0.434294\n"
		"0.693147,2.302585,6.283185,0.523599\n"
		"0.301030,3.321928,0.866025,0.000000\n"
	) == 0) ? "Ok" : "ERROR");
	
	puts(buf);
}

void checkVadd() {
	printf("TODO!\n");
}

void checkVsub() {
	printf("TODO!\n");
}

void checkVdiv() {
	printf("TODO!\n");
}

void checkVmmov() {
	printf("TODO!\n");
}

void checkVmul() {
	printf("TODO!\n");
}

void checkVrcp() {
	printf("TODO!\n");
}

void checkVpfxt() {
	printf("TODO!\n");
}

void checkViim() {
	int n;
	ScePspFVector4 v[5];

	asm volatile(
		"viim.s S000, 0\n"
		"viim.s S010, 1\n"
		"viim.s S020, -3\n"
		"viim.s S030, 777\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (v)
	);

	for (n = 0; n < 1; n++) {
		printf("%f,%f,%f,%f\n", v[n].x, v[n].y, v[n].z, v[n].w);
	}
}

void checkVectorCopy() {
	initValues();
	vcopy(&v0, &v1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

void checkVfim() {
	initValues();
	vfim(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

void checkDot() {
	initValues();
	vdotq(&v0, &v1, &v2);
	printf("%f\n", v0.x);
}

void checkScale() {
	initValues();
	vsclq(&v0, &v1, &v2);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

void checkRotation() {
	initValues();
	vrot(0.7, &v0);
	printf("%f, %f\n", v0.x, v0.y);
}

void moveNormalRegister() {
	float t = 5.0;
	//int t2 = *(int *)&t;
	ScePspFVector4 v;
	asm volatile(
		"mtv %1, S410\n"
		"mtv %1, S411\n"
		"mtv %1, S412\n"
		"mtv %1, S413\n"
		"sv.q   C410, 0x00+%0\n"
		: "+m" (v) : "t" (t)
	);
	printf("%f, %f, %f, %f\n", v.x, v.y, v.z, v.w);
}

void _checkMultiply(ScePspFVector4* v0) {
	float scale1 = 2.0f;
	float scale2 = -3.0f;

	__asm__ volatile (
		"vmidt.q M000\n"
		"vmidt.q M100\n"

		"mtv    %1, S300\n"
		"mtv    %2, S301\n"

		"vscl.q R000, R000, S300\n"
		"vscl.q R001, R001, S300\n"
		"vscl.q R002, R002, S300\n"
		"vscl.q R003, R003, S300\n"

		"vscl.q R100, R100, S301\n"
		"vscl.q R101, R101, S301\n"
		"vscl.q R102, R102, S301\n"
		"vscl.q R103, R103, S301\n"

		"vmmul.q M200, M000, M100\n"

		"sv.q    R200, 0x00+%0\n"
		"sv.q    R201, 0x10+%0\n"
		"sv.q    R202, 0x20+%0\n"
		"sv.q    R203, 0x30+%0\n"
		: "+m" (*v0) : "r" (scale1), "r" (scale2)
	);
}

void checkMultiply() {
	int n;
	_checkMultiply(&matrix[0]);
	for (n = 0; n < 4; n++) {
		printf("%f, %f, %f, %f\n", matrix[n].x, matrix[n].y, matrix[n].z, matrix[n].w);
	}
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
	printf(" lvl_row:\n");
	_checkLoadUnaligned1(&v0, 13, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned1(&v0, 24, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned1(&v0, 32, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);

	printf(" lvl_column:\n");
	_checkLoadUnaligned1(&v0, 15, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned1(&v0, 23, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned1(&v0, 31, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	

	printf(" lvr_row:\n");
	_checkLoadUnaligned2(&v0, 13, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned2(&v0, 24, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned2(&v0, 32, 0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);

	printf(" lvr_column:\n");
	_checkLoadUnaligned2(&v0, 15, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned2(&v0, 23, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
	_checkLoadUnaligned2(&v0, 31, 1);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);

}

void _checkVzero(ScePspFVector4* v0) {
	__asm__ volatile (
		"vzero.q R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void checkVzero() {
	v0.w = v0.z = v0.y = v0.x = -1.0f;
	_checkVzero(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

void _checkVone(ScePspFVector4* v0) {
	__asm__ volatile (
		"vone.q R000\n"
		"sv.q   R000, 0x00+%0\n"
		: "+m" (*v0)
	);
}

void checkVone() {
	v0.w = v0.z = v0.y = v0.x = -1.0f;
	_checkVone(&v0);
	printf("%f, %f, %f, %f\n", v0.x, v0.y, v0.z, v0.w);
}

/*
int gum_current_mode = GU_PROJECTION;

int gum_matrix_update[4] = { 0 };
int gum_current_matrix_update = 0;

ScePspFMatrix4* gum_current_matrix = gum_matrix_stack[GU_PROJECTION];

ScePspFMatrix4* gum_stack_depth[4] =
{
  gum_matrix_stack[GU_PROJECTION],
  gum_matrix_stack[GU_VIEW],
  gum_matrix_stack[GU_MODEL],
  gum_matrix_stack[GU_TEXTURE]
};

ScePspFMatrix4 gum_matrix_stack[4][32];

struct pspvfpu_context *gum_vfpucontext;
*/

void printVector(ScePspFVector4 *v) {
	printf("%f,%f,%f,%f\n", v->x, v->y, v->z, v->w);
}

void printMatrix(ScePspFMatrix4 *m) {
	printVector(&m->x);
	printVector(&m->y);
	printVector(&m->z);
	printVector(&m->w);
}

void checkGum() {
	ScePspFMatrix4 m;
	int val = 45;
	float angle = 0.7f;
	float c = cosf(angle);
	float s = sinf(angle);
	ScePspFVector3 pos = { 0, 0, -2.5f };
	ScePspFVector3 rot = { val * 0.79f * (GU_PI/180.0f), val * 0.98f * (GU_PI/180.0f), val * 1.32f * (GU_PI/180.0f) };

	sceGuInit();

	sceGumMatrixMode(GU_PROJECTION);
	sceGumLoadIdentity();
	sceGumPerspective(75.0f,16.0f/9.0f,0.5f,1000.0f);
	sceGumStoreMatrix(&m);
	printf(" PROJ:\n");
	printMatrix(&m);

	sceGumMatrixMode(GU_VIEW);
	sceGumLoadIdentity();
	sceGumStoreMatrix(&m);
	printf(" VIEW:\n");
	printMatrix(&m);

	// Complete Rotation
	sceGumMatrixMode(GU_MODEL);
	sceGumLoadIdentity();
	sceGumTranslate(&pos);
	sceGumRotateXYZ(&rot);
	printf(" MODEL:\n");
	printf("Vec: (x=%f, y=%f, z=%f)\n", rot.x, rot.y, rot.z);
	printf("Cos/Sin (%f, %f)\n", c, s);
	sceGumStoreMatrix(&m);
	printMatrix(&m);

	// Simple Rotation
	sceGumMatrixMode(GU_MODEL);
	sceGumLoadIdentity();
	sceGumRotateX(30.0f);
	sceGumStoreMatrix(&m);
	printf(" MODEL:\n");
	printMatrix(&m);
	
	//sceGumUpdateMatrix();
}

void _checkGlRotate() {
	int n;
	float M[16]; 
	
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();
	glRotatef(180, 0, 0, 1.0);
	
	glGetFloatv(GL_MODELVIEW_MATRIX, M);
	for (n = 0; n < 4; n++) {
		printf("%f, %f, %f, %f\n", M[n * 4 + 0], M[n * 4 + 1], M[n * 4 + 2], M[n * 4 + 3]);
	}
}

void checkGlRotate(int argc, char *argv[]) {
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_DEPTH);
	glutInitWindowSize(480, 272);
	glutCreateWindow(__FILE__);
	_checkGlRotate();
}

void checkMultiplyFull() {
	ScePspFMatrix4 m1 = {
		{  2,  3,  5,  7 },
		{ 11, 13, 17, 19 },
		{ 23, 29, 31, 37 },
		{ 41, 43, 47, 53 },
	};

	ScePspFMatrix4 m2 = {
		{  59,  61,  67,  71 },
		{  73,  79,  83,  89 },
		{  97, 101, 103, 107 },
		{ 109, 113, 127, 131 },
	};
	
	ScePspFMatrix4 m3;
	
	ScePspFVector4 *v0 = NULL, *v1 = NULL;

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
	
	printMatrix(&m3);

	/*
	asm volatile (
		"lv.q   C100, %1\n"
		"sv.q   C100, %0\n"

		: "+m" (*v0) : "m" (*v1)
	);
	*/
}

void checkMisc() {
	float fovy = 75.0f;
	
	ScePspFVector4 v;
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
	
	printVector(v0);
}

void __attribute__((noinline)) _checkSimpleLoad(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	asm volatile (
		"lv.s   S000, 0x00+%1\n"
		"sv.s   S000, 0x00+%0\n"

		: "+m" (*v0) : "m" (*v1)
	);
}

void __attribute__((noinline)) checkSimpleLoad() {
	ScePspFVector4 vIn = {0.0f, 0.0f, 0.0f, 0.0f};
	ScePspFVector4 vOut = {0.0f, 0.0f, 0.0f, 0.0f};
	vIn.x = 0.3f;
	_checkSimpleLoad(&vOut, &vIn);
	printf("%f\n", vOut.x);
}

void __attribute__((noinline)) _checkAggregatedAdd(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	
	asm volatile (
		"lv.q   C100, %1\n"
		"vfad.q S000, C100\n"
		"sv.s   S000, 0x00+%0\n"

		: "+m" (*v0) : "m" (*v1)
	);
}

void __attribute__((noinline)) _checkAggregatedAvg(ScePspFVector4 *v0, ScePspFVector4 *v1) {
	
	asm volatile (
		"lv.q   C100, %1\n"
		"vavg.q S000, C100\n"
		"sv.s   S000, 0x00+%0\n"

		: "+m" (*v0) : "m" (*v1)
	);
}

void checkAggregated() {
	ScePspFVector4 vIn = {11.0f, 22.0f, 33.0f, 44.0f};
	ScePspFVector4 vOut = {0.0f, 0.0f, 0.0f, 0.0f};
	_checkAggregatedAdd(&vOut, &vIn);
	printf("SUM: %f\n", vOut.x);
	_checkAggregatedAvg(&vOut, &vIn);
	printf("AVG: %f\n", vOut.x);
}

void _checkMatrixScale(ScePspFMatrix4 *matrix) {
	asm volatile (
		"vfim.s	 S000, 00\n"
		"vfim.s	 S001, 01\n"
		"vfim.s	 S002, 02\n"
		"vfim.s	 S003, 03\n"
		"vfim.s	 S010, 10\n"
		"vfim.s	 S011, 11\n"
		"vfim.s	 S012, 12\n"
		"vfim.s	 S013, 13\n"
		"vfim.s	 S020, 20\n"
		"vfim.s	 S021, 21\n"
		"vfim.s	 S022, 22\n"
		"vfim.s	 S023, 23\n"
		"vfim.s	 S030, 30\n"
		"vfim.s	 S031, 31\n"
		"vfim.s	 S032, 32\n"
		"vfim.s	 S033, 33\n"
		"vfim.s	 S200, -1\n"
		
		"vmscl.q M100, E000, S200\n"
		
		"sv.q    R100, 0x00+%0\n"
		"sv.q    R101, 0x10+%0\n"
		"sv.q    R102, 0x20+%0\n"
		"sv.q    R103, 0x30+%0\n"
		: "+m" (*matrix)
	);
}

void checkMatrixScale() {
	ScePspFMatrix4 matrix;
	_checkMatrixScale(&matrix);
	printMatrix(&matrix);
}

void _checkMatrixPerVector(ScePspFMatrix4 *matrix, ScePspFVector4 *vmult, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R700, 0x00+%1\n"
		"lv.q R701, 0x10+%1\n"
		"lv.q R702, 0x20+%1\n"
		"lv.q R703, 0x30+%1\n"
		
		"lv.q R600, 0x00+%2\n"
		
		"vtfm4.q R100, M700, R600\n"
		
		"sv.q    R100, 0x00+%0\n"
		: "+m" (*vresult) : "m" (*matrix), "m" (*vmult)
	);
}

void _checkHMatrixPerVector(ScePspFMatrix4 *matrix, ScePspFVector4 *vmult, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R700, 0x00+%1\n"
		"lv.q R701, 0x10+%1\n"
		"lv.q R702, 0x20+%1\n"
		"lv.q R703, 0x30+%1\n"
		
		"lv.q R600, 0x00+%2\n"
		
		"vhtfm4.q R100, M700, R600\n"
		
		"sv.q    R100, 0x00+%0\n"
		: "+m" (*vresult) : "m" (*matrix), "m" (*vmult)
	);
}

void checkMatrixPerVector() {
	ScePspFMatrix4 matrix = {
		{ 1.0f, 5.0f,  9.0f, 13.0f },
		{ 2.0f, 6.0f, 10.0f, 14.0f },
		{ 3.0f, 7.0f, 11.0f, 15.0f },
		{ 4.0f, 8.0f, 12.0f, 16.0f }
	};
	ScePspFVector4 vmult = { -10.0f, -20.0f, 30.0f, 40.0f};
	ScePspFVector4 vout = { 0.0f, 0.0f, 0.0f, 0.0f };
	
	_checkMatrixPerVector(&matrix, &vmult, &vout);
	printVector(&vout);
	
	_checkHMatrixPerVector(&matrix, &vmult, &vout);
	printVector(&vout);
}

void _checkCrossProduct(ScePspFVector4 *vleft, ScePspFVector4 *vright, ScePspFVector4 *vresult) {
	asm volatile (
		"lv.q R500, 0x00+%1\n"
		"lv.q R600, 0x00+%2\n"
		
		"vcrsp.t R100, R500, R600\n"
		
		"sv.q    R100, 0x00+%0\n"
		: "+m" (*vresult) : "m" (*vleft), "m" (*vright)
	);
}

void checkCrossProduct() {
	ScePspFVector4 vleft = { -1.0f, -2.0f, 3.0f, 4.0f};
	ScePspFVector4 vright = { -3.0f, 5.0f, 7.0f, -11.0f };
	ScePspFVector4 vout = { 0.0f, 0.0f, 0.0f, 0.0f };
	_checkCrossProduct(&vleft, &vright, &vout);
	printVector(&vout);
}

void _checkCompare(ScePspFVector4 *vleft, ScePspFVector4 *vright, ScePspFVector4 *vresult) {
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

void checkCompare() {
	ScePspFVector4 vleft  = { 1.0f, -1.0f, -1.1f, 2.0f };
	ScePspFVector4 vright = { 1.0f,  1.0f, -1.1f, 2.1f };
	ScePspFVector4 vout = { 0.0f, 0.0f, 0.0f, 0.0f };
	_checkCompare(&vleft, &vright, &vout);
	printVector(&vout);
}

int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();
	
	printf("checkCompare:\n"); checkCompare();
	//return 0;
	printf("checkCrossProduct:\n"); checkCrossProduct();
	printf("checkMatrixPerVector:\n"); checkMatrixPerVector();
	printf("checkAggregated:\n"); checkAggregated();
	printf("checkSimpleLoad:\n"); checkSimpleLoad();
	printf("checkMisc:\n"); checkMisc();
	printf("checkMultiplyFull:\n"); checkMultiplyFull();
	printf("checkVadd:\n"); checkVadd();
	printf("checkVsub:\n"); checkVsub();
	printf("checkVdiv:\n"); checkVdiv();
	printf("checkVmul:\n"); checkVmul();
	printf("checkVrcp:\n"); checkVrcp();
	printf("checkViim:\n"); checkViim();
	printf("checkLoadUnaligned:\n"); checkLoadUnaligned();
	printf("moveNormalRegister: "); moveNormalRegister();
	printf("checkVfim: "); checkVfim();
	printf("checkConstants:\n"); checkConstants();
	printf("checkVectorCopy: "); checkVectorCopy();
	printf("checkDot: "); checkDot();
	printf("checkScale: "); checkScale();
	printf("checkRotation: "); checkRotation();
	printf("checkMatrixIdentity:\n"); checkMatrixIdentity();
	printf("checkMultiply:\n"); checkMultiply();
	printf("checkVzero:\n"); checkVzero();
	printf("checkVone:\n"); checkVone();
	printf("checkGlRotate:\n"); checkGlRotate(argc, argv);
	printf("checkGum:\n"); checkGum();
	
	printf("checkMatrixScale:\n"); checkMatrixScale();
	
	//return 0;

	printf("Ended\n");
	
	/*
	while (1) {
		sceDisplayWaitVblankStart();
	}
	*/

	return 0;
}