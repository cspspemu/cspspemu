#include "SUB/MaiBitReader.h"
#include <stdio.h>

MaiBitReader::MaiBitReader(Mai_I32 byte_buf_size, Mai_I32 type)
{
	quene_in = new MaiQueue0(byte_buf_size);
	buffer = 0;
	bits_num = 0;
	this->type = type;
}

MaiBitReader::~MaiBitReader()
{
	mcs0.enter();

	delete quene_in;

	mcs0.leave();
}

Mai_Status MaiBitReader::moreByte()
{
	if (quene_in->GetLength())
	{
		Mai_I8 temp;
		quene_in->Out(&temp, 1);
		if (type == MaiBitReaderTypeHigh)
			buffer = (buffer << 8) | (temp & 0xFF);
		else if (type == MaiBitReaderTypeLow)
			buffer |= (temp & 0xFF) << bits_num;
		bits_num += 8;
		return 0;
	}
	else return -1;
}

Mai_Status MaiBitReader::addData(Mai_I8* src, Mai_I32 len_s)
{
	mcs0.enter();
	
	printf("addData(%d) : ", len_s);
	for (int n = 0; n < len_s; n++) printf("%02X-", (unsigned char)src[n]);
	printf("\n");

	quene_in->In(src, len_s);

	mcs0.leave();

	return 0;
}

Mai_I32 MaiBitReader::getRemainingBitsNum()
{
	mcs0.enter();

	Mai_I32 bits_remain = (quene_in->GetLength() << 3) + bits_num;

	mcs0.leave();

	return bits_remain;
}

Mai_I32 MaiBitReader::getWithI32Buffer(Mai_I32 bnum, Mai_Bool get_then_del_in_buf)
{
	mcs0.enter();

	while (bnum > bits_num) if (moreByte()) break;
	if (bnum <= bits_num)
	{
		Mai_I32 to_out = 0;

		if (type == MaiBitReaderTypeHigh)
			to_out = (buffer >> (bits_num - bnum)) & ((1 << bnum) - 1);
		else if (type == MaiBitReaderTypeLow)
			to_out = buffer & ((1 << bnum) - 1);

		if (get_then_del_in_buf)
		{
			bits_num -= bnum;

			if (type == MaiBitReaderTypeHigh)
				buffer = buffer & ((1 << bits_num) - 1);
			else if (type == MaiBitReaderTypeLow)
				buffer = (buffer >> bnum) & ((1 << bits_num) - 1);
		}

		mcs0.leave();
		printf("getWithI32Buffer(%d) : %d\n", bnum, to_out);
		return to_out;
	}
	else
	{
		mcs0.leave();
		return 0;
	}
}
