#ifndef Mai_All_h
#define Mai_All_h

#ifndef UNICODE
#define UNICODE
#endif

#ifndef _UNICODE
#define _UNICODE
#endif

#ifndef WINVER
#define WINVER 0x0501
#endif

#ifdef _WIN32
#define MAI_WIN
#endif

#ifndef MAI_LEVEL
#define MAI_LEVEL 99999
#endif

/* Type Definition */
typedef int Mai_Status;

typedef void Mai_Void;

typedef int Mai_Bool;

typedef char Mai_I8;
typedef unsigned char Mai_U8;

typedef short Mai_I16;
typedef unsigned short Mai_U16;

typedef int Mai_I32;
typedef unsigned int Mai_U32;

typedef long long Mai_I64;
typedef unsigned long long Mai_U64;

typedef Mai_I64 Mai_I64_Fixed1;

typedef wchar_t Mai_WChar;

#ifndef MAI_WIN
typedef Mai_WChar WCHAR;

typedef Mai_U8 BYTE;
typedef Mai_U16 WORD;
typedef Mai_U32 HANDLE;
typedef Mai_U32 DWORD;
#endif



/* Include */
#ifndef MAI_USER_APP
#ifdef MAI_WIN
#include <winsock2.h>
#include <windows.h>
#include <mmsystem.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#else
#include <unistd.h>
#include <wchar.h>
#include <memory.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#endif
#endif

#ifdef _M_X64
#define Mai_X64
#endif

#endif
