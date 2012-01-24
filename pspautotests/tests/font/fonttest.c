#include <common.h>

#include <pspsdk.h>
#include <pspgu.h>
#include <pspkernel.h>
#include <stdio.h>
#include <stdlib.h>
#include "libfont.h"

static void *Font_Alloc(void *data, u32 size) {
	printf("Font_Alloc(%08X, %u)\n", (uint)data, (uint)size);
	return malloc(size);
}

static void Font_Free(void *data, void *p){
	printf("Font_Free(%08X, %08X)\n", (uint)data, (uint)p);
	free(p);
}

int main(int argc, char *argv[]) {
	FontLibraryHandle libHandle;
	FontHandle        fontHandle;
	FontInfo          fontInfo;
	FontStyle         fontStyles[32];
	GlyphImage        glyphImage;
	int count;
	int result;
	int n;
	int x, y;
	uint errorCode;
	FontNewLibParams params = { NULL, 4, NULL, Font_Alloc, Font_Free, NULL, NULL, NULL, NULL, NULL, NULL };
	
	pspSdkLoadStartModule("flash0:/kd/libfont.prx", PSP_MEMORY_PARTITION_KERNEL);

	//pspDebugScreenInit();
	
	printf("Starting fonttest...\n");
	
	libHandle = sceFontNewLib(&params, &errorCode);
	printf("sceFontNewLib: %d, %08X\n", libHandle != 0, errorCode);
	{
		result = sceFontGetNumFontList(libHandle, &errorCode);
		printf("sceFontGetNumFontList: %d, 0x%08X\n", result, errorCode);
		count = 32;
		result = sceFontGetFontList(libHandle, fontStyles, count);
		printf("sceFontGetFontList: %d\n", result);
		if (result < count) count = result;
		for (n = 0; n < count; n++) {
			//printf("  %d:'%s':'%s'\n", n, fontStyles[n].fontName, fontStyles[n].fontFileName);
		}
		
		fontHandle = sceFontOpen(libHandle, 0, 0777, &errorCode);
		printf("sceFontOpen: %d, %08X\n", fontHandle != 0, errorCode);
		{
			result = sceFontGetFontInfo(fontHandle, &fontInfo);
			printf("sceFontGetFontInfo: %d\n", result);
			printf("Font.maxGlyphWidthF: %f\n", fontInfo.maxGlyphWidthF);
			printf("Font.maxGlyphHeightF: %f\n", fontInfo.maxGlyphHeightF);
			printf("Font.maxGlyphAscenderF: %f\n", fontInfo.maxGlyphAscenderF);
			printf("Font.maxGlyphDescenderF: %f\n", fontInfo.maxGlyphDescenderF);
			printf("Font.maxGlyphLeftXYF: %f, %f\n", fontInfo.maxGlyphLeftXF, fontInfo.maxGlyphBaseYF);
			printf("Font.minGlyphCenterXF: %f\n", fontInfo.minGlyphCenterXF);
			printf("Font.maxGlyphTopYF: %f\n", fontInfo.maxGlyphTopYF);
			printf("Font.maxGlyphAdvanceXYF: %f, %f\n", fontInfo.maxGlyphAdvanceXF, fontInfo.maxGlyphAdvanceXF);

			glyphImage.pixelFormat = GU_PSM_8888;
			glyphImage.positionX_F26_6 = 0 << 6;
			glyphImage.positionY_F26_6 = 0 << 6;
			glyphImage.bufferWidth = 32;
			glyphImage.bufferHeight = 32;
			glyphImage.bytesPerLine = 32 * 4;
			glyphImage.buffer = malloc(32 * 32 * sizeof(uint));

			printf("sceFontGetCharGlyphImage: %d\n", sceFontGetCharGlyphImage(fontHandle, 'H', &glyphImage));
			for (y = 0, n= 0; y < 32; y++) {
				printf("%08X: ", n);
				for (x = 0; x < 32; x++, n++) {
					uint v = ((uint*)glyphImage.buffer)[n];
					printf("%01X", v & 0x7);
				}
				printf("\n");
			}
		}
		sceFontClose(fontHandle);
	}
	sceFontDoneLib(libHandle);

	return 0;
}