#include "SUB/Mai_Sleep.h"

#ifdef MAI_WIN
Mai_Status Mai_Sleep(Mai_U32 millisec)
{
	Sleep( millisec );
	return 0;
}
#else
Mai_Status Mai_Sleep(Mai_U32 millisec)
{
	usleep( ((Mai_U64)millisec) * 1000 );
	return 0;
}
#endif
