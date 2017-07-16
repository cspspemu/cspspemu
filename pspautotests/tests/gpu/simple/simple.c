#include <common.h>

#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspdebug.h>
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include <string.h>
#include <time.h>

#include <pspgu.h>
#include <pspgum.h>
#include <pspdisplay.h>

static unsigned int __attribute__((aligned(16))) list[262144];

typedef struct {
	char u, v;
	unsigned int color;
	char x, y, z;
} VertexType1;

typedef struct {
	char u, v;
	char x, y, z;
} VertexType2;

typedef struct {
	unsigned int color;
	float x, y, z;
} VertexType3;

#define BUILD_COL(COLOR) 0xFF##COLOR

unsigned int clut16[16] = {
	BUILD_COL(000000),
	BUILD_COL(111111),
	BUILD_COL(222222),
	BUILD_COL(333333),
	BUILD_COL(444444),
	BUILD_COL(555555),
	BUILD_COL(666666),
	BUILD_COL(777777),
	BUILD_COL(888888),
	BUILD_COL(999999),
	BUILD_COL(AAAAAA),
	BUILD_COL(BBBBBB),
	BUILD_COL(CCCCCC),
	BUILD_COL(DDDDDD),
	BUILD_COL(EEEEEE),
	BUILD_COL(FFFFFF),
};

unsigned char tex4_4[4 * 4] = {
	 0,  1,  2,  3,
	 4,  5,  6,  7,
	 8,  9, 10, 11,
	12, 13, 14, 15
};

void dumpPixels(int dx, int dy, int w, int h) {
	int x, y;
	unsigned int *pixels;
	int bufferWidth;
	int pixelformat;
	sceDisplayGetFrameBuf((void **)&pixels, &bufferWidth, &pixelformat, PSP_DISPLAY_SETBUF_IMMEDIATE);
	
	for (x = 0; x < w; x++) {
		for (y = 0; y < h; y++) {
			printf("%04X,", pixels[(bufferWidth * (y + dy)) + (x + dx)]);
		}
		printf("\n");
	}
}

VertexType1 buildVertexType1(unsigned int color, char u, char v, char x, char y, char z) {
	VertexType1 vt;
	vt.color = color;
	vt.u = u; vt.v = v;
	vt.x = x; vt.y = y; vt.z = z;
	return vt;
}

VertexType2 buildVertexType2(char u, char v, char x, char y, char z) {
	VertexType2 vt;
	vt.u = u; vt.v = v;
	vt.x = x; vt.y = y; vt.z = z;
	return vt;
}

/*
VertexType3 buildVertexType3(float x, float y, float z) {
	VertexType3 vt;
	vt.x = x; vt.y = y; vt.z = z;
	return vt;
}
*/

// Every vertex has to be aligned to the maxium size of all of its component.
/**
 * Checks that engine is considering the vertex alignment.
 * Try to avoid regressions since the fix of the alignment at revision 252.
 */
void testVertexAlignment() {
	int vertexCount = 2;
	VertexType1* vertices = (VertexType1 *)sceGuGetMemory(vertexCount * sizeof(VertexType1));
	vertices[0] = buildVertexType1(0xFF0000FF, 0, 0, 0, 0, 0);
	vertices[1] = buildVertexType1(0xFF0000FF, 4, 4, 4, 4, 0);

	printf("testVertexAlignment\n");
	printf("Struct Size: %d\n", sizeof(VertexType1)); // 12

	sceGuStart(GU_DIRECT,list);
	sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	{
		sceGuEnable(GU_TEXTURE_2D);
		sceGuClutMode(GU_PSM_8888, 0, 0xFF, 0); // 32-bit palette
		sceGuClutLoad((16 / 8), clut16); // upload 32*8 entries (256)
		sceGuTexMode(GU_PSM_T8, 0, 0, 0); // 8-bit image
		sceGuTexImage(0, 4, 4, 4, tex4_4);
		sceGuTexFunc(GU_TFX_REPLACE, GU_TCC_RGB);
		sceGuTexFilter(GU_LINEAR, GU_LINEAR);
		sceGuTexScale(1.0f, 1.0f);
		sceGuTexOffset(0.0f, 0.0f);
		//sceGuAmbientColor(0xffffffff);
	
		sceGuDrawArray(GU_SPRITES, GU_TEXTURE_8BIT | GU_COLOR_8888 | GU_VERTEX_8BIT | GU_TRANSFORM_2D, vertexCount, 0, vertices);
	}
	sceGuFinish();
	sceGuSync(0, 0);
	sceGuSwapBuffers();
	
	dumpPixels(0, 0, 4, 4);
}

void testColorAdd() {
	int vertexCount = 2;
	VertexType2* vertices = (VertexType2 *)sceGuGetMemory(vertexCount * sizeof(VertexType2));
	vertices[0] = buildVertexType2(0, 0, 0, 0, 0);
	vertices[1] = buildVertexType2(4, 4, 4, 4, 0);

	printf("testColorAdd\n");
	
	sceGuStart(GU_DIRECT, list);
	sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	{
		sceGuDisable(GU_TEXTURE_2D);
		sceGuColor(0xff0000ff);
		sceGuDrawArray(GU_SPRITES, GU_TEXTURE_8BIT | GU_VERTEX_8BIT | GU_TRANSFORM_2D, vertexCount, 0, vertices);
	}
	sceGuFinish();
	sceGuSync(0, 0);

	sceGuStart(GU_DIRECT, list);
	{
		//sceGuColor(0x00000000);

		sceGuEnable(GU_TEXTURE_2D);
		sceGuClutMode(GU_PSM_8888, 0, 0xFF, 0); // 32-bit palette
		sceGuClutLoad((16 / 8), clut16); // upload 32*8 entries (256)
		sceGuTexMode(GU_PSM_T8, 0, 0, 0); // 8-bit image
		sceGuTexImage(0, 4, 4, 4, tex4_4);
		sceGuBlendFunc(GU_ADD, GU_SRC_ALPHA, GU_ONE_MINUS_SRC_ALPHA, 0, 0);
		sceGuTexFunc(GU_TFX_MODULATE, GU_TCC_RGB);
		sceGuTexFilter(GU_LINEAR, GU_LINEAR);
		sceGuTexScale(1.0f, 1.0f);
		sceGuTexOffset(0.0f, 0.0f);
		//sceGuAmbientColor(0xffffffff);
		//sceGuColor(0xffffffff);

		sceGuDrawArray(GU_SPRITES, GU_TEXTURE_8BIT | GU_VERTEX_8BIT | GU_TRANSFORM_2D, vertexCount, 0, vertices);
	}
	sceGuFinish();
	sceGuSync(0, 0);
	sceGuSwapBuffers();
	
	dumpPixels(0, 0, 4, 4);
}

u16 bezier_indices[16] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
VertexType3 __attribute__((aligned(16))) bezier_vertices[16] = {
	{ 0xFFFFFFFF, -0.500000,  0.500000, 0.000000 },
	{ 0xFFEEEEEE, -0.166667,  0.500000, 0.000000 },
	{ 0xFFDDDDDD,  0.166667,  0.500000, 0.000000 },
	{ 0xFFCCCCCC,  0.500000,  0.500000, 0.000000 },
	{ 0xFFBBBBBB, -0.500000,  0.166667, 0.000000 },
	{ 0xFFAAAAAA, -0.166667,  0.166667, 0.000000 },
	{ 0xFF999999,  0.166667,  0.166667, 0.000000 },
	{ 0xFF888888,  0.500000,  0.166667, 0.000000 },
	{ 0xFF777777, -0.500000, -0.166667, 0.000000 },
	{ 0xFF666666, -0.166667, -0.166667, 0.000000 },
	{ 0xFF555555,  0.166667, -0.166667, 0.000000 },
	{ 0xFF444444,  0.500000, -0.166667, 0.000000 },
	{ 0xFF333333, -0.500000, -0.500000, 0.000000 },
	{ 0xFF222222, -0.166667, -0.500000, 0.000000 },
	{ 0xFF111111,  0.166667, -0.500000, 0.000000 },
	{ 0xFF000000,  0.500000, -0.500000, 0.000000 },
};

/*

struct Vertex
{
   unsigned int color;
   float x, y, z;
};

struct Vertex __attribute__((aligned(16))) vertices[1*3] =
{
       {0xFF0000FF, 0.0f, -50.0f, 0.0f}, // Top, red
       {0xFF00FF00, 50.0f, 50.0f, 0.0f}, // Right, green
       {0xFFFF0000, -50.0f, 50.0f, 0.0f}, // Left, blue
};

void testBezier() {
	int n;
	int vtype = GU_INDEX_16BIT | GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_2D;
	//int vtype = GU_INDEX_16BIT | GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_3D;

	sceGuDepthRange(65535,0);
	sceGuStart(GU_DIRECT, list);
	sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	{
		sceGuDisable(GU_TEXTURE_2D);
		sceGuColor(0xff0000ff);
		
		sceGumMatrixMode(GU_PROJECTION);
		sceGumLoadIdentity();
		sceGumOrtho(0, 480, 272, 0, -1, 1);

		sceGumMatrixMode(GU_VIEW);
		sceGumLoadIdentity();
 
		sceGumMatrixMode(GU_MODEL);
		sceGumLoadIdentity();

		if (!(vtype & GU_TRANSFORM_2D)) {
			ScePspFVector3 scale = {100.0f, 100.0f, 100.0f};
			ScePspFVector3 translate = {100.0f, 100.0f, 0.0f};

			sceGumTranslate(&translate);
			sceGumScale(&scale);
		} else {
			//float size = 100.0f;
			float translate = 100.0f;
			//float translate = 0.0f;
			for (n = 0; n < 16; n++) {
				bezier_vertices[n].x = bezier_vertices[n].x * size + translate;
				bezier_vertices[n].y = bezier_vertices[n].y * size + translate;
				bezier_vertices[n].z = 0.0f;
			}
		}

		sceGuPatchDivide(2, 2);
		sceGuDrawBezier(vtype, 4, 4, bezier_indices, bezier_vertices);
		//sceGuDrawArray(GU_LINE_STRIP, vtype, 4 * 4, bezier_indices, bezier_vertices);
	}
	sceGuFinish();
	sceGuSync(0, 0);
	sceGuSwapBuffers();
}
*/

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4) /* change this if you change to another screenmode */
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH * SCR_HEIGHT * 2) /* zbuffer seems to be 16-bit? */

void init() {
	sceKernelDcacheWritebackAll();

	sceGuInit();
	sceGuStart(GU_DIRECT, list);

	sceGuDrawBuffer (GU_PSM_8888, (void*)0, BUF_WIDTH);
	sceGuDispBuffer (SCR_WIDTH, SCR_HEIGHT, (void*)FRAME_SIZE,BUF_WIDTH);
	sceGuDepthBuffer((void *)(FRAME_SIZE * 2), BUF_WIDTH);
	sceGuOffset     (2048 - (SCR_WIDTH / 2),2048 - (SCR_HEIGHT / 2));
	sceGuViewport   (2048, 2048, SCR_WIDTH, SCR_HEIGHT);
	sceGuDepthRange (0xc350, 0x2710);
	sceGuScissor    (0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuEnable     (GU_SCISSOR_TEST);
	sceGuFrontFace  (GU_CW);
	sceGuClear      (GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	sceGuFinish     ();
	sceGuSync       (0, 0);

	sceDisplayWaitVblankStart();
	sceGuDisplay(GU_TRUE);
}

int main(int argc, char *argv[]) {
	init();
	testVertexAlignment();
	testColorAdd();
	//testBezier();
	return 0;
}