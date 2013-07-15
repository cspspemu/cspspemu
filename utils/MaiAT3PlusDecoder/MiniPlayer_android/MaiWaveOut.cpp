#include "MaiWaveOut.h"




void MaiBufferQueueCallback(
	SLBufferQueueItf queueItf,
	SLuint32 eventFlags,
	const void *pBuffer,
	SLuint32 bufferSize,
	SLuint32 dataUsed,
	void *pContext)
{
}

MaiWaveOutSLES::MaiWaveOutSLES(Mai_U32 num_chn, Mai_U32 sam_rate, Mai_U32 bits_per_sam)
{
	is_stop = 0;
	
	//create sbuf
	sbuf_num = 0x80;
	sbuf_size = 0x6000;
	sbuf = heap0.alloc(sbuf_num * sbuf_size);
	sbuf_counter = 0;
	
	//create engine
	SLresult res0;
	
	SLEngineOption EngineOption[] = {
	(SLuint32)SL_ENGINEOPTION_THREADSAFE,
	(SLuint32)SL_BOOLEAN_TRUE,
	//(SLuint32)SL_ENGINEOPTION_MAJORVERSION, (SLuint32)1,
	//(SLuint32)SL_ENGINEOPTION_MINORVERSION, (SLuint32)1
	};
	
	res0 = slCreateEngine(&sl, 1, EngineOption, 0, 0, 0);
	
	res0 = (*sl)->Realize(sl, SL_BOOLEAN_FALSE);
	
	//des
	//(*sl)->Destroy(sl);
	
	SLEngineItf engine_itf;
	
	res0 = (*sl)->GetInterface(sl, SL_IID_ENGINE, (void*)&engine_itf);
	
	SLboolean required[3];
	SLInterfaceID iidArray[3];
	
	for (Mai_I32 a0 = 0; a0 < 3; a0++)
	{
		required[a0] = SL_BOOLEAN_FALSE;
		iidArray[a0] = SL_IID_NULL;
	}
	
	//required[0] = SL_BOOLEAN_TRUE;
	//iidArray[0] = SL_IID_VOLUME;
	
	res0 = (*engine_itf)->CreateOutputMix(engine_itf, &OutputMix, 0, iidArray, required);
	
	res0 = (*OutputMix)->Realize(OutputMix, SL_BOOLEAN_FALSE);
	
	//SLVolumeItf volumeItf;
	//res0 = (*OutputMix)->GetInterface(OutputMix, SL_IID_VOLUME, (void*)&volumeItf);
	
	SLDataLocator_BufferQueue bufferQueue;
	bufferQueue.locatorType = SL_DATALOCATOR_BUFFERQUEUE;
	bufferQueue.numBuffers = sbuf_num;
	
	SLDataFormat_PCM pcm;
	pcm.formatType = SL_DATAFORMAT_PCM;
	pcm.numChannels = num_chn;
	pcm.samplesPerSec = sam_rate * 1000;
	pcm.bitsPerSample = bits_per_sam;
	pcm.containerSize = bits_per_sam;
	//if (num_chn == 2)
		//pcm.channelMask = SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT;
		pcm.channelMask = 0;
	//if (num_chn == 6)
		//pcm.channelMask = SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT | SL_SPEAKER_FRONT_CENTER | SL_SPEAKER_BACK_LEFT | SL_SPEAKER_BACK_RIGHT | SL_SPEAKER_LOW_FREQUENCY;
	pcm.endianness = SL_BYTEORDER_LITTLEENDIAN;
	
	SLDataSource audioSource;
	audioSource.pFormat = (void *)&pcm;
	audioSource.pLocator = (void *)&bufferQueue;
	
	SLDataLocator_OutputMix locator_outputmix;
	SLDataSink audioSink;
	locator_outputmix.locatorType = SL_DATALOCATOR_OUTPUTMIX;
	locator_outputmix.outputMix = OutputMix;
	audioSink.pLocator = (void *)&locator_outputmix;
	audioSink.pFormat = 0;
	
	required[0] = SL_BOOLEAN_TRUE;
	iidArray[0] = SL_IID_BUFFERQUEUE;
	
	
	res0 = (*engine_itf)->CreateAudioPlayer(engine_itf, &player, &audioSource, &audioSink, 1, iidArray, required);
	
	res0 = (*player)->Realize(player, SL_BOOLEAN_FALSE);
	
	res0 = (*player)->GetInterface(player, SL_IID_PLAY, (void*)&playItf);
	
	res0 = (*player)->GetInterface(player, SL_IID_BUFFERQUEUE, (void*)&bufferQueueItf);

}

MaiWaveOutSLES::~MaiWaveOutSLES()
{
	this->stop();
	mcs0.enter();
	
	(*player)->Destroy(player);
	(*OutputMix)->Destroy(OutputMix);
	(*sl)->Destroy(sl);
	
	heap0.free(sbuf);
	
	mcs0.leave();
}

Mai_U32 MaiWaveOutSLES::getFixedBufXSize()
{
	return sbuf_size;
}

Mai_Status MaiWaveOutSLES::play()
{
	is_stop = 0;
	(*playItf)->SetPlayState( playItf, SL_PLAYSTATE_PLAYING );
	return 0;
}

Mai_Status MaiWaveOutSLES::stop()
{
	is_stop = 1;
	(*playItf)->SetPlayState( playItf, SL_PLAYSTATE_STOPPED );
	return 0;
}

Mai_Status MaiWaveOutSLES::pause()
{
	(*playItf)->SetPlayState( playItf, SL_PLAYSTATE_PAUSED );
	return 0;
}

Mai_Status MaiWaveOutSLES::enqueue(Mai_I8 *buf, Mai_U32 size, Mai_Bool is_syn)
{
	mcs0.enter();
	
	Mai_U32 buf_required = (size + sbuf_size - 1) / sbuf_size;
	SLBufferQueueState state;
	(*bufferQueueItf)->GetState(bufferQueueItf, &state);
	
	if ( (!is_syn) && ( (sbuf_num - state.count) < buf_required ) )
	{
		mcs0.leave();
		return -1;
	}
	
	Mai_U32 buf_loc = 0;
	while ( (!is_stop) && (size) )//(buf_required)
	{
		(*bufferQueueItf)->GetState(bufferQueueItf, &state);
		if (sbuf_num - state.count)
		{
			Mai_U32 to_copy = size;
			if (to_copy > sbuf_size) to_copy = sbuf_size;
			Mai_memcpy(sbuf + sbuf_counter, buf + buf_loc, to_copy);
			(*bufferQueueItf)->Enqueue(bufferQueueItf, (void*)(sbuf + sbuf_counter), to_copy);
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

Mai_U32 MaiWaveOutSLES::getRemainBufSize()
{
	SLBufferQueueState state;
	(*bufferQueueItf)->GetState(bufferQueueItf, &state);
	
	return state.count * sbuf_size;
}

