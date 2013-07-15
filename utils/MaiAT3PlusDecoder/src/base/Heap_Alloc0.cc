#include "SUB/Heap_Alloc0.h"

#ifdef MAI_WIN

Heap_Alloc0::Heap_Alloc0()
{
	heap = GetProcessHeap();
	heap_alloc_opts = HEAP_ZERO_MEMORY;
}

Mai_Status Heap_Alloc0::setHeap(HANDLE heap_new)
{
	heap = heap_new;
	return 0;
}

HANDLE Heap_Alloc0::getHeap()
{
	return heap;
}

Mai_Status Heap_Alloc0::setAllocOpts(DWORD dwFlags)
{
	heap_alloc_opts = dwFlags;
	return 0;
}

DWORD Heap_Alloc0::getAllocOpts()
{
	return heap_alloc_opts;
}

Mai_Status Heap_Alloc0::newHeap()
{
	DWORD dwFlags;
	if (heap_alloc_opts & HEAP_NO_SERIALIZE) dwFlags = HEAP_NO_SERIALIZE;
	HANDLE heap_temp = HeapCreate(dwFlags, 0, 0);
	if (heap_temp)
	{
		heap = heap_temp;
		return 0;
	}
	else
	{
		return -1;
	}
}

Mai_Status Heap_Alloc0::desHeap()
{
	HeapDestroy(heap);
	heap = GetProcessHeap();

	return 0;
}

Mai_I8* Heap_Alloc0::alloc(Mai_I32 size)
{
	return (Mai_I8*)HeapAlloc(heap, heap_alloc_opts, size);
}

Mai_I16* Heap_Alloc0::allocI16(Mai_I32 num)
{
	return (Mai_I16*)HeapAlloc(heap, heap_alloc_opts, num * 2);
}

Mai_I32* Heap_Alloc0::allocI32(Mai_I32 num)
{
	return (Mai_I32*)HeapAlloc(heap, heap_alloc_opts, num * 4);
}

Mai_I64* Heap_Alloc0::allocI64(Mai_I32 num)
{
	return (Mai_I64*)HeapAlloc(heap, heap_alloc_opts, num * 8);
}

Mai_Status Heap_Alloc0::free(Mai_Void* pointer)
{
	DWORD dwFlags = 0;
	if (heap_alloc_opts & HEAP_NO_SERIALIZE) dwFlags = HEAP_NO_SERIALIZE;
	if (HeapFree(heap, dwFlags, pointer)) return 0;
	else return -1;
}

#else

#ifndef __APPLE__
#include <malloc.h>
#endif
#include <sys/mman.h>

Heap_Alloc0::Heap_Alloc0()
{
}

Mai_Status Heap_Alloc0::setHeap(HANDLE heap_new)
{
	return 0;
}

HANDLE Heap_Alloc0::getHeap()
{
	return heap;
}

Mai_Status Heap_Alloc0::setAllocOpts(DWORD dwFlags)
{
	return 0;
}

DWORD Heap_Alloc0::getAllocOpts()
{
	return heap_alloc_opts;
}

Mai_Status Heap_Alloc0::newHeap()
{
	return 0;
}

Mai_I8* Heap_Alloc0::alloc(Mai_I32 size)
{
	return (Mai_I8*)malloc(size);
}

Mai_I16* Heap_Alloc0::allocI16(Mai_I32 num)
{
	return (Mai_I16*)alloc(num * 2);
}

Mai_I32* Heap_Alloc0::allocI32(Mai_I32 num)
{
	return (Mai_I32*)alloc(num * 4);
}

Mai_I64* Heap_Alloc0::allocI64(Mai_I32 num)
{
	return (Mai_I64*)alloc(num * 8);
}

Mai_Status Heap_Alloc0::free(Mai_Void* pointer)
{
	::free(pointer);
	return 0;
}


#endif