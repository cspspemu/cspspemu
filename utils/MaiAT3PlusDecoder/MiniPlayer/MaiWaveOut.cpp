
#include "MaiWaveOut.h"
#include "stdio.h"

MaiWaveOutWO::MaiWaveOutWO(Mai_U32 num_chn, Mai_U32 sam_rate, Mai_U32 bits_per_sam)
{
	is_stop = 0;
	
	//create sbuf
	sbuf_num = 0x80;
	sbuf_size = 0x2000;
	sbuf = heap0.alloc(sbuf_num * sbuf_size);
	sbuf_counter = 0;
	ppWaveHeader = (WAVEHDR *)heap0.alloc(sbuf_num * sizeof(WAVEHDR));

	WAVEFORMATEX wfx;
	Mai_memset(&wfx, 0, sizeof(wfx));

	wfx.wFormatTag = WAVE_FORMAT_PCM;
	wfx.nChannels = num_chn;
	wfx.nSamplesPerSec = sam_rate;
	wfx.wBitsPerSample = bits_per_sam;
	wfx.cbSize = 0;
	wfx.nBlockAlign = wfx.wBitsPerSample * wfx.nChannels / 8;  
	wfx.nAvgBytesPerSec = wfx.nChannels * wfx.nSamplesPerSec * wfx.wBitsPerSample / 8;

	::waveOutOpen(&p0, WAVE_MAPPER, &wfx, 0, 0, CALLBACK_NULL);

	for (Mai_U32 a0 = 0; a0 < sbuf_num; a0++)
	{
		memset(&ppWaveHeader[a0], 0, sizeof(WAVEHDR));
		ppWaveHeader[a0].dwLoops = 1;
		ppWaveHeader[a0].dwBufferLength = sbuf_size;
		ppWaveHeader[a0].lpData = (LPSTR)(sbuf + sbuf_size * a0);
		ppWaveHeader[a0].dwFlags = WHDR_DONE;
	}
}

MaiWaveOutWO::~MaiWaveOutWO()
{
	this->stop();
	mcs0.enter();

	waveOutClose(p0);

	heap0.free(ppWaveHeader);
	heap0.free(sbuf);

	mcs0.leave();
}

Mai_U32 MaiWaveOutWO::getFixedBufXSize()
{
	return sbuf_size;
}

Mai_Status MaiWaveOutWO::play()
{
	is_stop = 0;
	waveOutRestart(p0);
	return 0;
}

Mai_Status MaiWaveOutWO::stop()
{
	is_stop = 1;
	waveOutReset(p0);
	return 0;
}

Mai_Status MaiWaveOutWO::pause()
{
	waveOutPause(p0);
	return 0;
}

Mai_Status MaiWaveOutWO::enqueue(Mai_I8 *buf, Mai_U32 size, Mai_Bool is_syn)
{
	mcs0.enter();
	
	/*Mai_U32 buf_required = (size + sbuf_size - 1) / sbuf_size;
	SLBufferQueueState state;
	(*bufferQueueItf)->GetState(bufferQueueItf, &state);
	*/
	if ( (!is_syn) )//&& ( (sbuf_num - state.count) < buf_required ) )
	{
		mcs0.leave();
		return -1;
	}
	
	Mai_U32 buf_loc = 0;
	while ( (!is_stop) && (size) )//(buf_required)
	{
		WAVEHDR *h_now = &ppWaveHeader[sbuf_counter / sbuf_size];
		if (h_now->dwFlags & WHDR_DONE)
		{
			::waveOutUnprepareHeader(p0, h_now, sizeof(WAVEHDR));
			Mai_U32 to_copy = size;
			if (to_copy > sbuf_size) to_copy = sbuf_size;
			Mai_memcpy(sbuf + sbuf_counter, buf + buf_loc, to_copy);
			h_now->dwBufferLength = to_copy;
			h_now->dwFlags = 0;
			::waveOutPrepareHeader(p0, h_now, sizeof(WAVEHDR));
			::waveOutWrite(p0, h_now, sizeof(WAVEHDR));
			sbuf_counter += sbuf_size;
			sbuf_counter %= sbuf_num * sbuf_size;
			buf_loc += to_copy;
			size -= to_copy;
		}
		else
		{
			Mai_Sleep(1);
		}
	}
	
	mcs0.leave();
	return 0;
}

Mai_U32 MaiWaveOutWO::getRemainBufSize()
{
	Mai_U32 buf_count = 0;
	for (Mai_U32 a0 = 0; a0 < sbuf_num; a0++)
	{
		WAVEHDR *h_now = &ppWaveHeader[a0];
		if (!(h_now->dwFlags & WHDR_DONE))
		{
			buf_count++;
		}
	}

	return buf_count * sbuf_size;
}



