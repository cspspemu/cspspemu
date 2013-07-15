#ifndef MaiThread_h
#define MaiThread_h

#include "SUB/Mai_All.h"
#include "SUB/Mai_Sleep.h"

#ifdef MAI_WIN

DWORD WINAPI MaiThreadProc0(LPVOID lpParam);

class MaiThread
{
protected:
	LPSECURITY_ATTRIBUTES lpThreadAttributes;
	SIZE_T dwStackSize;
	LPTHREAD_START_ROUTINE lpStartAddress;
	LPVOID lpParameter;
	DWORD dwCreationFlags;
	LPDWORD lpThreadId;

	HANDLE threadhandle;
public:
	MaiThread();
	virtual Mai_Status Proc() = 0;
	Mai_Status open();
};

typedef struct _MaiThreadParam
{
	MaiThread *mt0;
	Mai_Bool is_creating;
} MaiThreadParam, *PMaiThreadParam;
#else

#include <pthread.h>

Mai_Void *MaiThreadProc0(Mai_Void* lpParam);

typedef Mai_Void *(*MSA)(Mai_Void*);

class MaiThread
{
protected:
	Mai_Void* lpThreadAttributes;
	Mai_U32 dwStackSize;
	MSA lpStartAddress;
	Mai_Void* lpParameter;
	Mai_U32 dwCreationFlags;
	pthread_t lpThreadId;

	pthread_t threadhandle;
public:
	MaiThread();
	virtual Mai_Status Proc() = 0;
	Mai_Status open();
};

typedef struct _MaiThreadParam
{
	MaiThread *mt0;
	Mai_Bool is_creating;
} MaiThreadParam, *PMaiThreadParam;

#endif

#endif
