#include "@common.h"
#include <pspge.h>
#include <pspgu.h>
#include <pspgum.h>

// psp-gcc -I. -I"%PSPSDK%/psp/sdk/include" -L. -L"%PSPSDK%/psp/sdk/lib" gum.c -lc -lpspgum -lpspgu -lpspge -lpspdisplay -lpspuser -lm -o gum.elf && psp-fixup-imports gum.elf

static unsigned int __attribute__((aligned(16))) list[262144];

void dumpMatrix(ScePspFMatrix4 m) {
	emitString("- ROW 0 -----");
	
	emitFloat(m.x.x);
	emitFloat(m.x.y);
	emitFloat(m.x.z);
	emitFloat(m.x.w);
	
	emitString("- ROW 1 -----");

	emitFloat(m.y.x);
	emitFloat(m.y.y);
	emitFloat(m.y.z);
	emitFloat(m.y.w);
	
	emitString("- ROW 2 -----");
	
	emitFloat(m.z.x);
	emitFloat(m.z.y);
	emitFloat(m.z.z);
	emitFloat(m.z.w);
	
	emitString("- ROW 3 -----");
	
	emitFloat(m.w.x);
	emitFloat(m.w.y);
	emitFloat(m.w.z);
	emitFloat(m.w.w);
}

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)

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

int main(int argc, char **argv) {
	ScePspFMatrix4 m;
	int n;
	m.x.x = -1;

	//void* fbp0 = getStaticVramBuffer(BUF_WIDTH,SCR_HEIGHT,GU_PSM_8888);
	//void* fbp1 = getStaticVramBuffer(BUF_WIDTH,SCR_HEIGHT,GU_PSM_8888);
	//void* zbp = getStaticVramBuffer(BUF_WIDTH,SCR_HEIGHT,GU_PSM_4444);

	void* fbp0 = (void *)0x04000000;
	void* fbp1 = (void *)0x04088000;
	void* zbp = (void *)0x04110000;
	/*
	void* fbp0 = (void *)0x40000000;
	void* fbp1 = (void *)0x40000000;
	void* zbp = (void *)0x40000000;
	*/

	sceGuInit();
 
	sceGuStart(GU_DIRECT,list);
	{
		sceGuDrawBuffer(GU_PSM_8888,fbp0,BUF_WIDTH);
		sceGuDispBuffer(SCR_WIDTH,SCR_HEIGHT,fbp1,BUF_WIDTH);
		sceGuDepthBuffer(zbp,BUF_WIDTH);
		sceGuOffset(2048 - (SCR_WIDTH/2),2048 - (SCR_HEIGHT/2));
		sceGuViewport(2048,2048,SCR_WIDTH,SCR_HEIGHT);
		sceGuDepthRange(65535,0);
		sceGuScissor(0,0,SCR_WIDTH,SCR_HEIGHT);
		sceGuEnable(GU_SCISSOR_TEST);
		sceGuFrontFace(GU_CW);
		sceGuShadeModel(GU_SMOOTH);
		sceGuDisable(GU_TEXTURE_2D);
		sceGuFinish();
	}
	sceGuSync(0,0);

	/*
	sceGumMatrixMode(GU_PROJECTION);

	sceGumLoadIdentity();
	sceGumStoreMatrix(&m);
	dumpMatrix(m);

	sceGumLoadIdentity();
	sceGumOrtho(0, 480, 272, 0, -1, +1);
	sceGumStoreMatrix(&m);
	dumpMatrix(m);
	*/

	ScePspFVector3 pos = {240.0f, 136.0f, 0.0f};
	int val = 0;

	for (n = 0; n < 10; n++)
	{
		sceGuStart(GU_DIRECT,list);
		{
			sceGumMatrixMode(GU_PROJECTION);
			sceGumLoadIdentity();
			sceGumOrtho(0, 480, 272, 0, -1, 1);

			sceGumMatrixMode(GU_VIEW);
			sceGumLoadIdentity();
	 
			sceGumMatrixMode(GU_MODEL);
			sceGumLoadIdentity();

			sceGumMatrixMode(GU_WORLD);
			sceGumLoadIdentity();

			sceGumTranslate(&pos);
			sceGumRotateZ(val*0.03f);
			
			sceGumDrawArray(GU_TRIANGLES,GU_COLOR_8888|GU_VERTEX_32BITF|GU_TRANSFORM_3D,1*3,0,vertices);
		}
		sceGuFinish();
		sceGuSync(0,0);
	}
	
	sceGuTerm();
	
	return 0;
}