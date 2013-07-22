#ifndef MaiBufIO_h
#define MaiBufIO_h

#include "SUB/Mai_All.h"
#include "SUB/Mai_mem.h"
#include "SUB/MaiThread.h"
#include "SUB/MaiQueue0.h"
#include "SUB/MaiCriticalSection.h"

class MaiIO
{
public:
	virtual ~MaiIO();
	virtual Mai_I32 read(Mai_Void* b, Mai_I32 off, Mai_I32 len) = 0;
	virtual Mai_I32 write(Mai_Void* b, Mai_I32 off, Mai_I32 len) = 0;
};


class MaiBufWriter : public MaiThread
{
private:
	MaiIO *io_itf;
	Mai_U32 block_size;
	MaiQueue0 *queue0;
	Mai_I8 *block_buf;
	Mai_U32 buf_size;
	Heap_Alloc0 heap0;
	Mai_Bool is_ok;
	MaiCriticalSection mcs0;
	MaiCriticalSection mcs1;
public:
	MaiBufWriter(MaiIO *io_itf, Mai_U32 block_size = 0x800, Mai_U32 buf_size = 0x100000);
	~MaiBufWriter();
	Mai_Status Proc();
	Mai_I32 write(Mai_Void* b, Mai_I32 off, Mai_I32 len);
};

#endif
