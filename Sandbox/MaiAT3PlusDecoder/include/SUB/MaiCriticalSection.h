#ifndef MaiCriticalSection_h
#define MaiCriticalSection_h

#include "SUB/Mai_All.h"

#ifdef MAI_WIN
#else
#include <pthread.h>
#endif

class MaiCriticalSection
{
public:
	MaiCriticalSection();
	~MaiCriticalSection();
	Mai_Status enter();
	Mai_Status leave();
private:
#ifdef MAI_WIN
	CRITICAL_SECTION cs0;
#else
	pthread_mutex_t pmt0;
#endif
};

#endif
