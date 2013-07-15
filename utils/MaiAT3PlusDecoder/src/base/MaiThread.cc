#include "SUB/MaiThread.h"

#ifdef MAI_WIN

MaiThreadParam mtp0 = {0};

DWORD WINAPI MaiThreadProc0(LPVOID lpParam)
{
	PMaiThreadParam pmtp0 = (PMaiThreadParam)lpParam;
	MaiThread *mt1 = pmtp0->mt0;
	pmtp0->is_creating = 0;
	mt1->Proc();
	return 0;
}

MaiThread::MaiThread()
{
	lpThreadAttributes = NULL;
	dwStackSize = 0;
	lpStartAddress = MaiThreadProc0;
	lpParameter = (LPVOID)&mtp0;
	dwCreationFlags = 0;
	lpThreadId = NULL;
}

Mai_Status MaiThread::open()
{
	while (mtp0.is_creating) Sleep(10);
	mtp0.mt0 = this;
	mtp0.is_creating = 1;//printf("s%d\n", mtp0.mt0);
	threadhandle = CreateThread(lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
	if (threadhandle) return 0;
	else
	{
		MessageBoxW(NULL, L"CreateThread Error!", L"Error", 0);
		return -1;
	}
}

#else



MaiThreadParam mtp0 = {0};

Mai_Void *MaiThreadProc0(Mai_Void* lpParam)
{
	pthread_detach(pthread_self());
	
	PMaiThreadParam pmtp0 = (PMaiThreadParam)lpParam;
	MaiThread *mt1 = pmtp0->mt0;
	pmtp0->is_creating = 0;
	mt1->Proc();

	pthread_exit(0);	
	return 0;
}

MaiThread::MaiThread()
{
	lpThreadAttributes = NULL;
	dwStackSize = 0;
	lpStartAddress = (MSA)MaiThreadProc0;
	lpParameter = (Mai_Void*)&mtp0;
	dwCreationFlags = 0;
	lpThreadId = 0;
}

Mai_Status MaiThread::open()
{
	while (mtp0.is_creating) Mai_Sleep(10);
	mtp0.mt0 = this;
	mtp0.is_creating = 1;//printf("s%d\n", mtp0.mt0);
	//threadhandle = CreateThread(lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
	if ( !pthread_create(&threadhandle, NULL, lpStartAddress, lpParameter) ) return 0;
	else
	{
		mtp0.is_creating = 0;
		//MessageBoxW(NULL, L"CreateThread Error!", L"Error", 0);
		return -1;
	}
}

#endif
