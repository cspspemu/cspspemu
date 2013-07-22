#ifndef MaiString_h
#define MaiString_h

#include "SUB/Mai_All.h"
#include "SUB/Heap_Alloc0.h"

/* MaiString */
Mai_I32 Ustrlen(Mai_WChar* s);
Mai_Status Ustrcpy(Mai_WChar* dst, Mai_WChar* src);

Mai_I32 Astrlen(Mai_I8* s);
Mai_Status Astrcpy(Mai_I8* dst, Mai_I8* src);

Mai_I32 Mai_strlen(Mai_WChar* s);
Mai_I32 Mai_strlen(Mai_I8* s);
Mai_Status Mai_strcpy(Mai_WChar* dst, Mai_WChar* src);
Mai_Status Mai_strcpy(Mai_I8* dst, Mai_I8* src);
Mai_Status Mai_strcat(Mai_WChar* dst, Mai_WChar* src);
Mai_Status Mai_strcat(Mai_I8* dst, Mai_I8* src);
Mai_Status Mai_strcmp(Mai_WChar* p1, Mai_WChar* p2);
Mai_Status Mai_strcmp(Mai_I8* p1, Mai_I8* p2);
Mai_Status LowToCap(Mai_I8* src);
Mai_Status LowToCap(Mai_WChar* src);
Mai_Status removeLRLetter(Mai_I8* src, Mai_I8 letter);
Mai_Status removeLRLetter(Mai_WChar* src, Mai_WChar letter);
Mai_Status splitString(Mai_I8* dst, Mai_I8* remain, Mai_I8* src, Mai_I8 splitter);
Mai_Status splitString(Mai_WChar* dst, Mai_WChar* remain, Mai_WChar* src, Mai_WChar splitter);

Mai_Status DecToUStr(WCHAR* str, Mai_I32 off, Mai_I64 a);
Mai_Status UStrToDec(Mai_I64 *a, Mai_WChar* str);
Mai_Status AStrToDec(Mai_I64 *a, Mai_I8* str);

Mai_Status AStrToHex(Mai_I64 *a, Mai_I8* str);

Mai_Status MaiStrCloner(Heap_Alloc0 *heap0, Mai_WChar **pdst, Mai_WChar *src);

Mai_Status Mai_strcmpAllC(Mai_I8* p1, Mai_I8* p2);
Mai_Status Mai_strcmpAllC(Mai_WChar* p1, Mai_WChar* p2);

#endif
