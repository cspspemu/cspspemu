#ifndef MaiWaveOut_H
#define MaiWaveOut_H

#include "Mai_Base0.h"

#define MaiWaveOutI MaiWaveOutWO

class MaiWaveOutWO
{
private:
	HWAVEOUT p0;
	WAVEHDR *ppWaveHeader;
private:
	Mai_Bool is_stop;
	Mai_U32 sbuf_counter;
	Mai_U32 sbuf_num;
	Mai_U32 sbuf_size;
	Mai_I8 *sbuf;
	Heap_Alloc0 heap0;
	MaiCriticalSection mcs0;
public:
	MaiWaveOutWO(Mai_U32 num_chn, Mai_U32 sam_rate, Mai_U32 bits_per_sam);
	~MaiWaveOutWO();
	Mai_U32 getFixedBufXSize();
	Mai_Status play();
	Mai_Status pause();
	Mai_Status stop();
	Mai_Status enqueue(Mai_I8 *buf, Mai_U32 size, Mai_Bool is_syn = 1);
	Mai_U32 getRemainBufSize();
};

#endif
