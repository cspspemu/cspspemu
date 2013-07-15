#include "SUB/MaiCriticalSection.h"

MaiCriticalSection::MaiCriticalSection()
{
#ifdef MAI_WIN
	InitializeCriticalSection(&cs0);
#else
	pthread_mutex_init(&pmt0, 0);
#endif
}

MaiCriticalSection::~MaiCriticalSection()
{
#ifdef MAI_WIN
	DeleteCriticalSection(&cs0);
#else
	pthread_mutex_destroy(&pmt0);
#endif
}

Mai_Status MaiCriticalSection::enter()
{
#ifdef MAI_WIN
	EnterCriticalSection(&cs0);
#else
	pthread_mutex_lock(&pmt0);
#endif

	return 0;
}

Mai_Status MaiCriticalSection::leave()
{
#ifdef MAI_WIN
	LeaveCriticalSection(&cs0);
#else
	pthread_mutex_unlock(&pmt0);
#endif

	return 0;
}
