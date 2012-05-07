/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <stdio.h>
#include <pspaudiolib.h>
#include <pspaudio.h>
#include "xmem.h"
#include "xmath.h"

#include "xsound.h"

typedef struct sound_instance {
	u16 playstate;
	u32 playptr;
	u32 playptr_fraction;
	u32 rateratio;
	u16 loop;
	u16 panmode;
	float pan;
	float volume;
	xSoundBuffer* buffer;
	xSound3dSource* src;
} sound_instance;

int num_sounds = 0;
sound_instance* sounds = NULL;

static void sound_callback(void *buf, unsigned int reqn, void *pdata)
{
	if (sounds == NULL) return;
	s16* buffer = buf;
	int n;
	for (n = 0; n < reqn; n++)
	{
		int out_right = 0;
		int out_left = 0;
		int i;
		for (i = 0; i < num_sounds; i++)
		{
			sound_instance* inst = &sounds[i];

			if (inst->playstate != X_SOUND_PLAY)
				continue;

			u32 fraction = inst->playptr_fraction + inst->rateratio;
			inst->playptr += fraction >> 16;
			inst->playptr_fraction = fraction & 0xffff;
			if (inst->volume <= 0.0f)
				continue;

			if (inst->playptr >= inst->buffer->samplecount)
			{
				if (inst->loop == X_SOUND_LOOP)
				{
					inst->playptr = 0;
					inst->playptr_fraction = 0;
				}
				else
				{
					xSoundSetState(i, X_SOUND_STOP);
					continue;
				}
			}

			int index = inst->buffer->channels * inst->playptr;
			if (inst->buffer->samplesize == 1)
			{
				s8* data8 = (s8*)inst->buffer->data;
				if (inst->buffer->channels == 1)
				{
					out_left  += (int)(data8[index]*256 * inst->volume * (1.0f - inst->pan));
					out_right += (int)(data8[index]*256 * inst->volume * inst->pan);
				}
				else
				{
					if (inst->panmode == X_SOUND_COMBINED)
					{
						out_left  += (int)((data8[index+0] + data8[index+1])*256*0.5f * inst->volume * (1.0f - inst->pan));
						out_right += (int)((data8[index+0] + data8[index+1])*256*0.5f * inst->volume * inst->pan);
					}
					else
					{
						out_left  += (int)(data8[index+0]*256 * inst->volume * (1.0f - inst->pan));
						out_right += (int)(data8[index+1]*256 * inst->volume * inst->pan);
					}
				}
			}
			else
			{
				s16* data16 = (s16*)inst->buffer->data;
				if (inst->buffer->channels == 1)
				{
					out_left  += (int)(data16[index] * inst->volume * (1.0f - inst->pan));
					out_right += (int)(data16[index] * inst->volume * inst->pan);
				}
				else
				{
					if (inst->panmode == X_SOUND_COMBINED)
					{
						out_left  += (int)((data16[index+0] + data16[index+1])*0.5f * inst->volume * (1.0f - inst->pan));
						out_right += (int)((data16[index+0] + data16[index+1])*0.5f * inst->volume * inst->pan);
					}
					else
					{
						out_left  += (int)(data16[index+0] * inst->volume * (1.0f - inst->pan));
						out_right += (int)(data16[index+1] * inst->volume * inst->pan);
					}
				}
			}
		}
		if (out_left < -32768) out_left = -32768;
		else if (out_left > 32767) out_left = 32767;
		if (out_right < -32768) out_right = -32768;
		else if (out_right > 32767) out_right = 32767;
		*(buffer++) = (s16)out_left;
		*(buffer++) = (s16)out_right;
	}
}

int xSoundInit(int max_sounds)
{
	X_LOG("Attempting to initialize sounds...");
	if (sounds != NULL) return 1;
	sounds = (sound_instance*)x_malloc(max_sounds*sizeof(sound_instance));
	if (sounds == NULL) return 1;
	num_sounds = max_sounds;
	xSoundSetStateAll(X_SOUND_STOP, 0);
	pspAudioInit();
	pspAudioSetChannelCallback(X_SOUND_CHANNEL, sound_callback, 0);
	pspAudioSetVolume(X_SOUND_CHANNEL, PSP_VOLUME_MAX, PSP_VOLUME_MAX);
	X_LOG("Successfully initialized sounds.");
	return 0;
}

void xSoundEnd()
{
	X_LOG("Attempting to end sounds...");
	if (sounds == NULL) return;
	pspAudioEnd();
	num_sounds = 0;
	x_free(sounds);
	sounds = NULL;
	X_LOG("Successfully ended sounds.");
}

void xSoundGlobalVolume(float volume)
{
	if (volume < 0.0f) volume = 0.0f;
	if (volume > 1.0f) volume = 1.0f;
	int vol = (int)(volume*PSP_VOLUME_MAX);
	pspAudioSetVolume(X_SOUND_CHANNEL, vol, vol);
}

typedef struct {
	u32 ChunkID;
	u32 ChunkSize;
	u32 Format;
} riff_chunk;

typedef struct {
	u32 Subchunk1ID;
	u32 Subchunk1Size;
	u16 AudioFormat;
	u16 NumChannels;
	u32 SampleRate;
	u32 ByteRate;
	u16 BlockAlign;
	u16 BitsPerSample;
} wave_chunk;

typedef struct {
	u32 Subchunk2ID;
	u32 Subchunk2Size;
} data_chunk;

#define WAV_HEAD_RIFF   ('R'<<0|'I'<<8|'F'<<16|'F'<<24) /* "RIFF" (0x46464952) */
#define WAV_HEAD_FORMAT ('W'<<0|'A'<<8|'V'<<16|'E'<<24) /* "WAVE" (0x45564157) */
#define WAV_HEAD_SUB1ID ('f'<<0|'m'<<8|'t'<<16|' '<<24) /* "fmt " (0x20746d66) */
#define WAV_HEAD_SUB2ID ('d'<<0|'a'<<8|'t'<<16|'a'<<24) /* "data" (0x61746164) */

#define WAV_PCM (1)

xSoundBuffer* xSoundLoadBufferWav(char* filename)
{
	if (filename == NULL) return NULL;
	X_LOG("Attempting to load WAV buffer \"%s\"...", filename);
	xSoundBuffer* buf = (xSoundBuffer*)x_malloc(sizeof(xSoundBuffer));
	if (buf == NULL) return NULL;
	buf->data = NULL;

	FILE* file = fopen(filename, "rb");
	if (file == NULL)
	{
		xSoundFreeBuffer(buf);
		return NULL;
	}

	riff_chunk riff_c;
	fread(&riff_c, sizeof(riff_chunk), 1, file);

	wave_chunk wave_c;
	fread(&wave_c, sizeof(wave_chunk), 1, file);

	data_chunk data_c;
	fseek(file, wave_c.Subchunk1Size-16, SEEK_CUR);
	fread(&data_c, sizeof(data_chunk), 1, file);

	X_LOG("WAV: ChunkID: 0x%08x, ChunkSize: %u, Format: 0x%08x, Subchunk1ID: 0x%08x, Subchunk1Size: %u \
		  AudioFormat: %u, NumChannels: %u, SampleRate: %u, ByteRate: %u, BlockAlign: %u, BitsPerSample: %u, Subchunk2ID: 0x%08x, Subchunk2Size: %u",
		  filename, riff_c.ChunkID, riff_c.ChunkSize, riff_c.Format, wave_c.Subchunk1ID, wave_c.Subchunk1Size,
		  wave_c.AudioFormat, wave_c.NumChannels, wave_c.SampleRate, wave_c.ByteRate, wave_c.BlockAlign, wave_c.BitsPerSample, data_c.Subchunk2ID, data_c.Subchunk2Size);

	if (riff_c.ChunkID != WAV_HEAD_RIFF || riff_c.Format != WAV_HEAD_FORMAT ||
		wave_c.AudioFormat != WAV_PCM || wave_c.Subchunk1ID != WAV_HEAD_SUB1ID ||
		data_c.Subchunk2ID != WAV_HEAD_SUB2ID ||
		(wave_c.NumChannels != 1 && wave_c.NumChannels != 2) ||
		(wave_c.BitsPerSample != 8 && wave_c.BitsPerSample != 16))
	{
		fclose(file);
		xSoundFreeBuffer(buf);
		return NULL;
	}

	u32 datalength = data_c.Subchunk2Size;

	buf->channels = wave_c.NumChannels;
	buf->samplesize = wave_c.BitsPerSample/8;
	buf->samplerate = wave_c.SampleRate;
	buf->samplecount = datalength/(buf->channels*buf->samplesize);

	buf->data = x_malloc(datalength);
	if (buf->data == NULL)
	{
		fclose(file);
		xSoundFreeBuffer(buf);
		return NULL;
	}
	fread(buf->data, datalength, 1, file);
	fclose(file);

	buf->def_vol = 1.0f;
	buf->def_pitch = 1.0f;
	buf->def_pan = 0.0f;
	buf->def_panmode = X_SOUND_SEPARATE;
	buf->def_loop = X_SOUND_NO_LOOP;

	X_LOG("Successfully load WAV buffer.");
	return buf;
}

void xSoundFreeBuffer(xSoundBuffer* buf)
{
	X_LOG("Freeing WAV buffer.");
	if (buf != NULL)
	{
		if (buf->data != NULL)
		{
			x_free(buf->data);
		}
		x_free(buf);
	}
}

int xSoundPlay(xSoundBuffer* buf)
{
	if (buf == NULL || sounds == NULL) return -1;
	int i;
	for (i = 0; i < num_sounds; i++)
	{
		if (xSoundGetState(i) == X_SOUND_STOP)
		{
			sounds[i].playptr = 0;
			sounds[i].playptr_fraction = 0;
			sounds[i].src = NULL;
			sounds[i].buffer = buf;
			xSoundSetLoop(i, buf->def_loop);
			xSoundSetPanMode(i, buf->def_panmode);
			xSoundSetVolume(i, buf->def_vol);
			xSoundSetPan(i, buf->def_pan);
			xSoundSetPitch(i, buf->def_pitch);
			sounds[i].playstate = X_SOUND_PLAY;
			return i;
		}
	}
	return -1;
}

int xSoundSetState(int ref, int state)
{
	if (sounds == NULL || ref < 0) return 1;
	switch (state)
	{
	case X_SOUND_STOP:
		if (sounds[ref].playstate == X_SOUND_STOP)
		{
			return 1;
		}
		else
		{
			sounds[ref].playstate = X_SOUND_STOP;
			return 0;
		}
	case X_SOUND_PLAY:
		if (sounds[ref].playstate == X_SOUND_PAUSE)
		{
			sounds[ref].playstate = X_SOUND_PLAY;
			return 0;
		}
		else
		{
			//return xSoundPlay(i);
			return 1;
		}
	case X_SOUND_PAUSE:
		if (sounds[ref].playstate == X_SOUND_PLAY)
		{
			sounds[ref].playstate = X_SOUND_PAUSE;
			return 0;
		}
		else
		{
			return 1;
		}
	}
	return 1;
}

int xSoundGetState(int ref)
{
	if (sounds == NULL || ref < 0) return X_SOUND_STOP;
	return sounds[ref].playstate;
}

int xSoundSetStateAll(int state, int skip_mask)
{
	if (sounds == NULL) return 0;
	int changed = 0;
	int i;
	for (i = 0; i < num_sounds; i++)
	{
		if (!(skip_mask & (1<<i)))
		{
			if (xSoundSetState(i, state) == 0)
			{
				changed += 1;
			}
		}
	}
	return changed;
}

void xSoundSetLoop(int ref, int loop)
{
	if (sounds == NULL || ref < 0) return;
	sounds[ref].loop = loop;
}

void xSoundSetPanMode(int ref, int panmode)
{
	if (sounds == NULL || ref < 0) return;
	sounds[ref].panmode = panmode;
}

void xSoundSetVolume(int ref, float volume)
{
	if (sounds == NULL || ref < 0) return;
	if (volume < 0.0f) volume = 0.0f;
	if (volume > 1.0f) volume = 1.0f;
	sounds[ref].volume = volume;
}

void xSoundSetPan(int ref, float pan)
{
	if (sounds == NULL || ref < 0) return;
	if (pan < -1.0f) pan = -1.0f;
	if (pan >  1.0f) pan =  1.0f;
	pan = (pan + 1.0f)*0.5f;
	sounds[ref].pan = pan;
}

void xSoundSetPitch(int ref, float pitch)
{
	if (sounds == NULL || ref < 0) return;
	if (pitch < 0.0f) pitch = 0.0f;
	u32 rateratio = (u32)(pitch*((sounds[ref].buffer->samplerate*0x4000)/11025));
	/*
	if (rateratio < (2000*0x4000)/11025) rateratio = (2000*0x4000)/11025;
	if (rateratio > (100000*0x4000)/11025) rateratio = (100000*0x4000)/11025;
	*/
	sounds[ref].rateratio = rateratio;
}

float speed_of_sound = 343.3f;
xVector3f listener_pos = {0.0f, 0.0f, 0.0f};
xVector3f listener_right = {1.0f, 0.0f, 0.0f};
xVector3f listener_vel = {0.0f, 0.0f, 0.0f};

void xSound3dSpeedOfSound(float value)
{
	speed_of_sound = value;
}

void xSound3dSetListener(ScePspFMatrix4* orientation, ScePspFVector3* vel)
{
	if (orientation == NULL || vel == NULL) return;
	listener_pos = *(xVector3f*)&orientation->w;
	listener_right = *(xVector3f*)&orientation->x;
	listener_vel = *(xVector3f*)vel;
}

void sound3d_update(int ref)
{
	xVector3f dir;
	xVec3Sub(&dir, (xVector3f*)&sounds[ref].src->pos, &listener_pos);
	float dist_sq = xVec3SqLength(&dir);
	if (dist_sq >= SQR(sounds[ref].src->radius))
	{
		xSoundSetVolume(ref, 0.0f);
	}
	else
	{
		//volume
		xSoundSetVolume(ref, 1.0f - x_sqrtf(dist_sq)/sounds[ref].src->radius);
		//pan
		xVec3Normalize(&dir, &dir);
		xSoundSetPan(ref, xVec3Dot(&dir, &listener_right));
		//pitch
		xVector3f rel_vel;
		xVec3Sub(&rel_vel, (xVector3f*)&sounds[ref].src->vel, &listener_vel);
		xSoundSetPitch(ref, speed_of_sound / (speed_of_sound + xVec3Dot(&dir, &rel_vel)));
	}
}

int xSound3dPlay(xSoundBuffer* buf, xSound3dSource* s, int update)
{
	if (buf == NULL || s == NULL || sounds == NULL) return -1;
	int i;
	for (i = 0; i < num_sounds; i++)
	{
		if (xSoundGetState(i) == X_SOUND_STOP)
		{
			sounds[i].playptr = 0;
			sounds[i].playptr_fraction = 0;
			sounds[i].src = s;
			sounds[i].buffer = buf;
			xSoundSetLoop(i, buf->def_loop);
			xSoundSetPanMode(i, X_SOUND_COMBINED);
			sound3d_update(i);
			//take this out
			if (sounds[i].volume > 0.0f || update)
			{
				if (!update)
					sounds[i].src = NULL;
				sounds[i].playstate = X_SOUND_PLAY;
				return i;
			}
		}
	}
	return -1;
}

void xSound3dUpdate()
{
	if (sounds == NULL) return;
	int i;
	for (i = 0; i < num_sounds; i++)
	{
		if (xSoundGetState(i) == X_SOUND_PLAY && sounds[i].src != NULL)
		{
			sound3d_update(i);
		}
	}

}