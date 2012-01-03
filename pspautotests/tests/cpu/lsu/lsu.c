#include <common.h>

#include <pspkernel.h>

///////////////////////////////////////////////////////////////////////////////
// LWL/LWR                                                                   //
///////////////////////////////////////////////////////////////////////////////

#define LWRL_DEF(TYPE, POS) \
	unsigned int TYPE##_##POS(unsigned int address) { \
		int ret; \
		asm volatile ( \
			"li $t0, 0x12345678\n" \
			#TYPE " $t0, " #POS "(%1)\n" \
			"move %0, $t0\n" \
			\
			: "=r"(ret) : "r" (address) \
		); \
		return ret; \
	}

#define LWR_DEF(POS) LWRL_DEF(lwr, POS)
#define LWL_DEF(POS) LWRL_DEF(lwl, POS)

#define LWRL_CALL(TYPE, POS) \
	printf(#TYPE "_" #POS " 0x%08X\n", TYPE##_##POS((unsigned int)&data[0]));

#define LWR_CALL(POS) LWRL_CALL(lwr, POS)
#define LWL_CALL(POS) LWRL_CALL(lwl, POS)

///////////////////////////////////////////////////////////////////////////////
// SWL/SWR                                                                   //
///////////////////////////////////////////////////////////////////////////////

#define SWRL_DEF(TYPE, POS) \
	unsigned int TYPE##_##POS(unsigned int address) { \
		int ret; \
		asm volatile ( \
			"li $t0, 0x12345678\n" \
			#TYPE " $t0, " #POS "(%1)\n" \
			"move %0, $t0\n" \
			\
			: "=r"(ret) : "r" (address) \
		); \
		return ret; \
	}

#define SWR_DEF(POS) SWRL_DEF(swr, POS)
#define SWL_DEF(POS) SWRL_DEF(swl, POS)

#define SWRL_CALL(TYPE, POS) \
	memcpy(data_copy, data, sizeof(data)); \
	TYPE##_##POS((unsigned int)&data_copy[0]); \
	printf(#TYPE "_" #POS " 0x%08X\n", *(unsigned int *)&data_copy[0]);

#define SWR_CALL(POS) SWRL_CALL(swr, POS)
#define SWL_CALL(POS) SWRL_CALL(swl, POS)


unsigned char data[16] = { 0xF0, 0xE1, 0xD2, 0xC3, 0xB4, 0xA5, 0x96, 0x87, 0x78, 0x69, 0x5A, 0x4B, 0x3C, 0x2D, 0x1E, 0x0F };
unsigned char data_copy[16];

LWR_DEF(0)
LWR_DEF(1)
LWR_DEF(2)
LWR_DEF(3)

LWL_DEF(0)
LWL_DEF(1)
LWL_DEF(2)
LWL_DEF(3)

SWR_DEF(0)
SWR_DEF(1)
SWR_DEF(2)
SWR_DEF(3)

SWL_DEF(0)
SWL_DEF(1)
SWL_DEF(2)
SWL_DEF(3)

void testAlignedLwlrSwlr() {
	LWR_CALL(0);
	LWR_CALL(1);
	LWR_CALL(2);
	LWR_CALL(3);
	printf("\n");

	LWL_CALL(0);
	LWL_CALL(1);
	LWL_CALL(2);
	LWL_CALL(3);
	printf("\n");

	SWR_CALL(0);
	SWR_CALL(1);
	SWR_CALL(2);
	SWR_CALL(3);
	printf("\n");

	SWL_CALL(0);
	SWL_CALL(1);
	SWL_CALL(2);
	SWL_CALL(3);
	printf("\n");
}

void testUnalignedLwlrSwlr() {
	printf("@TODO: testUnalignedLwlrSwlr\n");
}

int main(int argc, char *argv[]) {
	testAlignedLwlrSwlr();
	testUnalignedLwlrSwlr();

	return 0;
}