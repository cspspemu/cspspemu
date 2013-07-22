#ifndef Heap_Alloc0_h
#define Heap_Alloc0_h

#include "SUB/Mai_All.h"

/* Heap_Alloc0 */
#ifdef MAI_WIN
class Heap_Alloc0
{
public:
	Heap_Alloc0();
	Mai_Status setHeap(HANDLE heap_new);
	HANDLE getHeap();
	Mai_Status setAllocOpts(DWORD dwFlags);
	DWORD getAllocOpts();
	Mai_Status newHeap();
	Mai_Status desHeap();
	Mai_I8* alloc(Mai_I32 size);
	Mai_I16* allocI16(Mai_I32 num);
	Mai_I32* allocI32(Mai_I32 num);
	Mai_I64* allocI64(Mai_I32 num);
	Mai_Status free(Mai_Void* pointer);
private:
	HANDLE heap;
	DWORD heap_alloc_opts;
};
#else
class Heap_Alloc0
{
public:
	Heap_Alloc0();
	Mai_Status setHeap(HANDLE heap_new);
	HANDLE getHeap();
	Mai_Status setAllocOpts(DWORD dwFlags);
	DWORD getAllocOpts();
	Mai_Status newHeap();
	static Mai_I8* alloc(Mai_I32 size);
	static Mai_I16* allocI16(Mai_I32 num);
	static Mai_I32* allocI32(Mai_I32 num);
	static Mai_I64* allocI64(Mai_I32 num);
	static Mai_Status free(Mai_Void* pointer);
private:
	HANDLE heap;
	DWORD heap_alloc_opts;
};
#endif

#endif
