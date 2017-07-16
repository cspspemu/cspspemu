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

#include <pspgu.h>
#include <pspgum.h>

#include "vfpu_common.h"

ALIGN16 ScePspFVector4 v0, v1, v2;
ALIGN16 ScePspFVector4 matrix[4];

void checkGum(float angle) {
	static ALIGN16 ScePspFMatrix4 m;
	int val = 45;
	float c = cosf(angle);
	float s = sinf(angle);
	static ALIGN16 ScePspFVector3 pos = { 0, 0, -2.5f };
	static ALIGN16 ScePspFVector3 rot;
	rot.x = val * 0.79f * (GU_PI/180.0f);
	rot.y = val * 0.98f * (GU_PI/180.0f);
	rot.z = val * 1.32f * (GU_PI/180.0f);

	sceGuInit();

	sceGumMatrixMode(GU_PROJECTION);
	sceGumLoadIdentity();
	sceGumPerspective(75.0f,16.0f/9.0f,0.5f,1000.0f);
	sceGumStoreMatrix(&m);
	printMatrix("proj", &m);

	sceGumMatrixMode(GU_VIEW);
	sceGumLoadIdentity();
	sceGumStoreMatrix(&m);
	printMatrix("view", &m);

	// Complete Rotation
	sceGumMatrixMode(GU_MODEL);
	sceGumLoadIdentity();
	sceGumTranslate(&pos);
	sceGumRotateXYZ(&rot);
	printf("Vec: (x=%f, y=%f, z=%f)\n", rot.x, rot.y, rot.z);
	printf("Cos/Sin (%f, %f)\n", c, s);
	sceGumStoreMatrix(&m);
	printMatrix("model1", &m);

	// Simple Rotation
	sceGumMatrixMode(GU_MODEL);
	sceGumLoadIdentity();
	sceGumRotateX(30.0f);
	sceGumStoreMatrix(&m);
	printMatrix("model2", &m);
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

int main(int argc, char *argv[]) {
	printf("Started\n");

	resetAllMatrices();
	
	printf("checkGlRotate:\n"); checkGlRotate(argc, argv);
	printf("checkGum 0.7:\n"); checkGum(0.7f);
	printf("checkGum 1.1:\n"); checkGum(1.1f);

	printf("Ended\n");
	return 0;
}
