#ifndef MaiBitReader_h
#define MaiBitReader_h

#include "SUB/Mai_All.h"
#include "SUB/MaiCriticalSection.h"
#include "SUB/MaiQueue0.h"

#define MaiBitReaderTypeHigh 0
#define MaiBitReaderTypeLow 1

class MaiBitReader
{
private:
	MaiQueue0* quene_in;
	Mai_I32 buffer;
	Mai_I32 bits_num;
	Mai_I32 type;

	MaiCriticalSection mcs0;
private:
	Mai_Status moreByte();
public:
	MaiBitReader(Mai_I32 byte_buf_size, Mai_I32 type = MaiBitReaderTypeHigh);
	~MaiBitReader();
	Mai_Status addData(Mai_I8* src, Mai_I32 len_s);
	Mai_I32 getRemainingBitsNum();
	Mai_I32 getWithI32Buffer(Mai_I32 bnum, Mai_Bool get_then_del_in_buf = 1);
};

#endif
