#include <common.h>

#include <pspkernel.h>

///////////////////////////////////////////////////////////////////////////////
// LWL/LWR                                                                   //
///////////////////////////////////////////////////////////////////////////////

#define LWRL_DEF(TYPE, POS, SUFIX) \
	unsigned int TYPE##_##SUFIX(unsigned int address) { \
		int ret; \
		asm volatile ( \
			/*"li $t0, 0x12345678\n"*/ \
			"li $t0, 0x0F0E0D0C\n" \
			#TYPE " $t0, " #POS "(%1)\n" \
			"move %0, $t0\n" \
			\
			: "=r"(ret) : "r" (address) \
		); \
		return ret; \
	}

#define LWR_DEF(POS) LWRL_DEF(lwr, POS, POS)
#define LWL_DEF(POS) LWRL_DEF(lwl, POS, POS)
#define LWR_NEG_OFFSET_DEF(POS) LWRL_DEF(lwr, -POS, NEG##POS)
#define LWL_NEG_OFFSET_DEF(POS) LWRL_DEF(lwl, -POS, NEG##POS)

#define LWRL_CALL(TYPE, POS, OFFSET, SUFIX) \
	printf(#TYPE "_" #SUFIX "_" #OFFSET " 0x%08X\n", TYPE##_##SUFIX((unsigned int)&data[OFFSET]));

#define LWR_CALL(POS, OFFSET) LWRL_CALL(lwr, POS, OFFSET, POS)
#define LWL_CALL(POS, OFFSET) LWRL_CALL(lwl, POS, OFFSET, POS)
#define LWR_NEG_OFFSET_CALL(POS, OFFSET) LWRL_CALL(lwr, -POS, OFFSET, NEG##POS)
#define LWL_NEG_OFFSET_CALL(POS, OFFSET) LWRL_CALL(lwl, -POS, OFFSET, NEG##POS)

///////////////////////////////////////////////////////////////////////////////
// SWL/SWR                                                                   //
///////////////////////////////////////////////////////////////////////////////

#define SWRL_DEF(TYPE, POS, SUFIX) \
	unsigned int TYPE##_##SUFIX(unsigned int address) { \
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

#define SWR_DEF(POS) SWRL_DEF(swr, POS, POS)
#define SWL_DEF(POS) SWRL_DEF(swl, POS, POS)
#define SWR_NEG_OFFSET_DEF(POS) SWRL_DEF(swr, -POS, NEG##POS)
#define SWL_NEG_OFFSET_DEF(POS) SWRL_DEF(swl, -POS, NEG##POS)

#define SWRL_CALL(TYPE, POS, OFFSET, SUFIX) \
	memcpy(data_copy, data, sizeof(data)); \
	TYPE##_##SUFIX((unsigned int)&data_copy[OFFSET]); \
	printf(#TYPE "_" #SUFIX "_" #OFFSET " 0x%08X, 0x%08X, 0x%08X, 0x%08X\n", *(unsigned int *)&data_copy[0], *(unsigned int *)&data_copy[4], *(unsigned int *)&data_copy[8], *(unsigned int *)&data_copy[12]);

#define SWR_CALL(POS, OFFSET) SWRL_CALL(swr, POS, OFFSET, POS)
#define SWL_CALL(POS, OFFSET) SWRL_CALL(swl, POS, OFFSET, POS)
#define SWR_NEG_OFFSET_CALL(POS, OFFSET) SWRL_CALL(lwr, -POS, OFFSET, NEG##POS)
#define SWL_NEG_OFFSET_CALL(POS, OFFSET) SWRL_CALL(lwl, -POS, OFFSET, NEG##POS)


//unsigned char data[16] = { 0xF0, 0xE1, 0xD2, 0xC3, 0xB4, 0xA5, 0x96, 0x87, 0x78, 0x69, 0x5A, 0x4B, 0x3C, 0x2D, 0x1E, 0x0F };
unsigned char data[16] = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 };
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

LWR_NEG_OFFSET_DEF(0)
LWR_NEG_OFFSET_DEF(1)
LWR_NEG_OFFSET_DEF(2)
LWR_NEG_OFFSET_DEF(3)

LWL_NEG_OFFSET_DEF(0)
LWL_NEG_OFFSET_DEF(1)
LWL_NEG_OFFSET_DEF(2)
LWL_NEG_OFFSET_DEF(3)

SWR_NEG_OFFSET_DEF(0)
SWR_NEG_OFFSET_DEF(1)
SWR_NEG_OFFSET_DEF(2)
SWR_NEG_OFFSET_DEF(3)

SWL_NEG_OFFSET_DEF(0)
SWL_NEG_OFFSET_DEF(1)
SWL_NEG_OFFSET_DEF(2)
SWL_NEG_OFFSET_DEF(3)

void testAlignedLwlrSwlr() {
	LWR_CALL(0, 0);
	LWR_CALL(1, 0);
	LWR_CALL(2, 0);
	LWR_CALL(3, 0);
	printf("\n");

	LWL_CALL(0, 0);
	LWL_CALL(1, 0);
	LWL_CALL(2, 0);
	LWL_CALL(3, 0);
	printf("\n");

	SWR_CALL(0, 0);
	SWR_CALL(1, 0);
	SWR_CALL(2, 0);
	SWR_CALL(3, 0);
	printf("\n");

	SWL_CALL(0, 0);
	SWL_CALL(1, 0);
	SWL_CALL(2, 0);
	SWL_CALL(3, 0);
	printf("\n");
}

void testUnalignedLwlrSwlr() {
	LWR_CALL(0, 7);
	LWR_CALL(1, 7);
	LWR_CALL(2, 7);
	LWR_CALL(3, 7);
	printf("\n");

	LWL_CALL(0, 7);
	LWL_CALL(1, 7);
	LWL_CALL(2, 7);
	LWL_CALL(3, 7);
	printf("\n");

	SWR_CALL(0, 7);
	SWR_CALL(1, 7);
	SWR_CALL(2, 7);
	SWR_CALL(3, 7);
	printf("\n");

	SWL_CALL(0, 7);
	SWL_CALL(1, 7);
	SWL_CALL(2, 7);
	SWL_CALL(3, 7);
	printf("\n");
}

void testUnalignedNegLwlrSwlr() {
	LWR_NEG_OFFSET_CALL(0, 5);
	LWR_NEG_OFFSET_CALL(1, 5);
	LWR_NEG_OFFSET_CALL(2, 5);
	LWR_NEG_OFFSET_CALL(3, 5);
	printf("\n");

	LWL_NEG_OFFSET_CALL(0, 5);
	LWL_NEG_OFFSET_CALL(1, 5);
	LWL_NEG_OFFSET_CALL(2, 5);
	LWL_NEG_OFFSET_CALL(3, 5);
	printf("\n");

	SWR_NEG_OFFSET_CALL(0, 5);
	SWR_NEG_OFFSET_CALL(1, 5);
	SWR_NEG_OFFSET_CALL(2, 5);
	SWR_NEG_OFFSET_CALL(3, 5);
	printf("\n");

	SWL_NEG_OFFSET_CALL(0, 5);
	SWL_NEG_OFFSET_CALL(1, 5);
	SWL_NEG_OFFSET_CALL(2, 5);
	SWL_NEG_OFFSET_CALL(3, 5);
	printf("\n");
}

int main(int argc, char *argv[]) {
	testAlignedLwlrSwlr();
	testUnalignedLwlrSwlr();
	testUnalignedNegLwlrSwlr();

	return 0;
}