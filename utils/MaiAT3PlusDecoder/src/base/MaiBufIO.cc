
#include "SUB/MaiBufIO.h"

MaiIO::~MaiIO()
{
}



MaiBufWriter::MaiBufWriter(MaiIO *io_itf, Mai_U32 block_size, Mai_U32 buf_size)
{
	this->io_itf = io_itf;
	this->block_size = block_size;
	this->buf_size = buf_size;

	queue0 = new MaiQueue0(buf_size);
	block_buf = heap0.alloc(block_size);

	is_ok = 1;

	open();

	mcs0.enter();
	while (is_ok != 2)
	{
		mcs0.leave();
		Mai_Sleep(1);
		mcs0.enter();
	}
	mcs0.leave();
}

MaiBufWriter::~MaiBufWriter()
{
	mcs0.enter();
	is_ok = 0;
	mcs0.leave();

	mcs1.enter();
	heap0.free(block_buf);
	delete queue0;
	mcs1.leave();

}

Mai_Status MaiBufWriter::Proc()
{
	mcs1.enter();
	mcs0.enter();
	is_ok = 2;
	mcs0.leave();
	mcs0.enter();
	while (is_ok)
	{
		mcs0.leave();

		if (queue0->GetLength() >= (Mai_I32)block_size)
		{
			queue0->Out(block_buf, block_size);
			if (block_size != io_itf->write(block_buf, 0, block_size))
			{
				//
			}
		}
		else
		{
			Mai_Sleep(1);
		}

		mcs0.enter();
	}
	mcs0.leave();

	Mai_I32 remain = queue0->GetLength();

	while (remain)
	{
		if (remain > (Mai_I32)block_size) remain = block_size;
		queue0->Out(block_buf, remain);
		if (remain != io_itf->write(block_buf, 0, remain))
		{
		}
		remain = queue0->GetLength();
	}

	mcs1.leave();
	return 0;
}

Mai_I32 MaiBufWriter::write(Mai_Void* b, Mai_I32 off, Mai_I32 len)
{
	Mai_I32 a0 = 0;

	mcs0.enter();
	while ((is_ok) && (a0 < len))
	{
		mcs0.leave();

		//Mai_I32 to_trans = ((Mai_I32)buf_size > len) ? len : buf_size;
		Mai_I32 transed = queue0->In((Mai_I8*)b + a0 + off, len - a0);
		a0 += transed;
		if (!transed) Mai_Sleep(1);

		mcs0.enter();
	}
	mcs0.leave();

	return a0;
}
