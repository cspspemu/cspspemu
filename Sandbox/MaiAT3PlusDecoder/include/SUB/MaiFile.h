
#ifndef MaiFile_h
#define MaiFile_h

#include "SUB/Mai_All.h"
#include "SUB/Heap_Alloc0.h"
#include "SUB/MaiString.h"
#include "SUB/Unicode.h"
#include "SUB/MaiBufIO.h"

#define MaiFileORdOnly 1
#define MaiFileOWrOnly 2
#define MaiFileORdWr (MaiFileORdOnly | MaiFileOWrOnly)
#define MaiFileOCreate 4

/* MaiFile */
class MaiFile : public MaiIO
{
public:
	MaiFile(Mai_I8* src);
	MaiFile(Mai_WChar* src);
	~MaiFile();
	Mai_Status open(Mai_I32 oflag);
	Mai_Status close();
	Mai_Bool is_exist();
	Mai_Status del();
	Mai_Status moveTo(Mai_I8* dst);
	Mai_Status moveTo(Mai_WChar* dst);
	Mai_Status seek(Mai_I64 pos);
	Mai_I32 skipBytes(Mai_I32 n);
	Mai_I64 getFilePointer();
	Mai_I64 getFileLen();
	Mai_Status setFileLen(Mai_I64 newLength);
	Mai_I32 read(Mai_Void* b, Mai_I32 off, Mai_I32 len);
	Mai_I32 write(Mai_Void* b, Mai_I32 off, Mai_I32 len);

private:
#ifdef MAI_WIN
	Mai_Status init(Mai_WChar* src);
#else
	Mai_Status init(Mai_I8* src);
#endif

private:
#ifdef MAI_WIN
	Mai_WChar* src;
	HANDLE file0;
	Heap_Alloc0 heap0;
#else
	Mai_I8* src;
	Mai_I32 file0;
	Heap_Alloc0 heap0;
#endif
};

#endif
