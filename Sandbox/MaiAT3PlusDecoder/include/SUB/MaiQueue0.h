#ifndef MaiQueue0_h
#define MaiQueue0_h

#include "SUB/Mai_All.h"
#include "SUB/Heap_Alloc0.h"
#include "SUB/Mai_Sleep.h"
#include "SUB/Mai_mem.h"
#include "SUB/MaiCriticalSection.h"

class MaiQueue0
{
public:
	MaiQueue0(Mai_I32 quene_max_size);
	~MaiQueue0();
	Mai_I32 In(Mai_I8 *head, Mai_I32 length);
	Mai_I32 Out(Mai_I8 *head, Mai_I32 length);
	Mai_I32 OutPre(Mai_I8 *head, Mai_I32 length);
	Mai_I32 GetLength();
	Mai_I32 GetMaxLength();
	Mai_Status Flush();
	Mai_Status Dis();
	Mai_Bool isOK();
private:
	Mai_I8 *base;
	Mai_I32 rear,front;
	Mai_I32 max_size;
	Mai_Status status;
	Heap_Alloc0 heap0;
	MaiCriticalSection mcs0;

	//Mai_I32 is_ining;
	//Mai_I32 is_outing;
};

#endif
