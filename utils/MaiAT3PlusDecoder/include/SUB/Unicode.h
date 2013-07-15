#ifndef Unicode_h
#define Unicode_h

#include "SUB/Mai_All.h"
#include "SUB/MaiString.h"

/* Mai_CodePage */
Mai_Status CP932ToUC(WCHAR* dst, Mai_I32 off_d, Mai_I8* src, Mai_I32 off_s);
Mai_Status UCToCP932(Mai_I8* dst, Mai_I32 off_d, WCHAR* src, Mai_I32 off_s);

Mai_Status CP936ToUC(WCHAR* dst, Mai_I32 off_d, Mai_I8* src, Mai_I32 off_s);
Mai_Status UCToCP936(Mai_I8* dst, Mai_I32 off_d, WCHAR* src, Mai_I32 off_s);

Mai_Status UCToUTF8(Mai_I8* dst, Mai_WChar *src);
Mai_Status UTF8ToUC(Mai_WChar *dst, Mai_I8* src);

#endif
