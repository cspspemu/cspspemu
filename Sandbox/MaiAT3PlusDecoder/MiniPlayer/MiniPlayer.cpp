#include "Mai_Base0.h"

#include "stdio.h"

#include "MaiWaveOut.h"

#include "MaiAT3PlusFrameDecoder.h"

Mai_Status main(Mai_I32 n, Mai_I8 **args)
{
	if (n < 2)
	{
		printf("usage:  MiniPlayer <at3 or oma file>\n");
		return 0;
	}
	MaiFile *raf0 = new MaiFile(args[1]);

	raf0->open(MaiFileORdOnly);


	//parse header infos here
	Mai_I8 strt[0x10];
	raf0->seek(0);
	raf0->read(strt, 0, 3);
	strt[3] = 0;
	raf0->seek(0);
	Mai_U16 bztmp;
	if (!Mai_strcmp(strt, "ea3"))
	{
		//we get ea3 header
		printf("ea3 header\n");
		raf0->seek(0x6);
		Mai_I8 tmp[4];
		raf0->read(tmp, 0, 4);
		Mai_I32 skipbytes = 0;
		for (Mai_I32 a0 = 0; a0 < 4; a0++)
		{
			skipbytes <<= 7;
			skipbytes += tmp[a0] & 0x7F;
		}
		printf("skipBytes: %d\n", skipbytes);
		raf0->skipBytes(skipbytes);
	}
	
	if (!Mai_strcmp(strt, "RIF")) //RIFF
	{
		//we get RIFF header
		printf("RIFF header\n");
		raf0->seek(0x10);
		Mai_I32 fmt_size = 0;
		raf0->read(&fmt_size, 0, 4);
		Mai_U16 fmt = 0;
		raf0->read(&fmt, 0, 2);
		if (fmt != 0xFFFE)
		{
			printf("RIFF File fmt error\n");
			return -1;
		}
		raf0->skipBytes(0x28);
		raf0->read(&bztmp, 0, 2);
		bztmp = (bztmp >> 8) | (bztmp << 8);
		raf0->skipBytes(fmt_size - 0x2c);

		//search the data chunk
		for (Mai_I32 a0 = 0; a0 < 0x100; a0++)
		{
			Mai_I32 tmpr;
			raf0->read(&tmpr, 0, 4);
			if (tmpr == (Mai_I32)'atad') break;
		}
		Mai_I32 tmpr;
		raf0->read(&tmpr, 0, 4);
	}
	else
	{
		//EA3 block that contains at3+ stream
		printf("EA3 header\n");
		raf0->skipBytes(0x22);
	
		raf0->read(&bztmp, 0, 2);
		bztmp = (bztmp >> 8) | (bztmp << 8);
		raf0->skipBytes(0x3c);
	}
	Mai_I32 blocksz = bztmp & 0x3FF;
	
	Mai_I8 buf0[0x3000];
	
	//calculate the frame block size here
	Mai_I32 block_size = blocksz * 8 + 8;

	printf("frame_block_size 0x%x\n", block_size);
	
	
	//so we make a new at3+ frame decoder
	MaiAT3PlusFrameDecoder *d2 = new MaiAT3PlusFrameDecoder();

	raf0->read(buf0, 0, block_size);
	Mai_I32 chns = 0;
	Mai_I16 *p_buf;
	//decode the first frame and get channel num
	if (d2->decodeFrame(buf0, block_size, &chns, &p_buf)) printf("decode error\n");
	printf("channels: %d\n", chns);
	if (chns > 2) printf("warning: waveout doesn't support %d chns\n", chns);

	//just waveout
	MaiWaveOutI *mwo0 = new MaiWaveOutI(chns, 44100, 16);

	mwo0->play();
	while (1)
	{
		{
			raf0->read(buf0, 0, block_size);

			//decode frame and get sample data
			if (d2->decodeFrame(buf0, block_size, 0, &p_buf)) printf("decode error\n");
		}
		//play it
		mwo0->enqueue((Mai_I8*)p_buf, 0x800 * chns * 2);

		if (raf0->getFilePointer() >= raf0->getFileLen()) break;
	}

	raf0->close();
	delete raf0;

	while (mwo0->getRemainBufSize()) Mai_Sleep(1);
	
	delete d2;

	return 0;
}
