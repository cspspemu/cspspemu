/*
 * xlib - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#ifndef __X_SOUND_H__
#define __X_SOUND_H__

#include "xconfig.h"

#ifdef __cplusplus
extern "C" {
#endif

/* TODO:
	add id/ref system so that sounds arent changed if they are different now from what the user is trying to change
*/

/* play states */
#define X_SOUND_STOP 0
#define X_SOUND_PLAY 1
#define X_SOUND_PAUSE 2

/* loop modes */
#define X_SOUND_NO_LOOP 0
#define X_SOUND_LOOP 1

/* pan modes */
#define X_SOUND_SEPARATE 0
#define X_SOUND_COMBINED 1

typedef struct xSoundBuffer {
	float def_vol;
	float def_pitch;
	float def_pan;
	int def_panmode;
	int def_loop;

	u32 channels;
	u16 samplesize;
	u32 samplerate;
	u32 samplecount;
	void* data;
} xSoundBuffer;

typedef struct xSound3dSource {
	ScePspFVector3 pos;
	ScePspFVector3 vel;
	float radius;
} xSound3dSource;

/* initialize sound. returns 0 on success, 1 on failure */
int xSoundInit(int max_sounds);

void xSoundEnd();

void xSoundGlobalVolume(float volume);

xSoundBuffer* xSoundLoadBufferWav(char* filename);

void xSoundFreeBuffer(xSoundBuffer* buf);

/* play a sound, returns ref on success, -1 on failure */
int xSoundPlay(xSoundBuffer* buf);

/* set play state - X_SOUND_STOP, X_SOUND_PLAY, or X_SOUND_PAUSE. returns 0 on success, 1 on failure */
int xSoundSetState(int ref, int state);

/* get play state - X_SOUND_STOP, X_SOUND_PLAY, X_SOUND_PAUSE */
int xSoundGetState(int ref);

/* set state for all slots, does not effect stopped slots. returns num changed */
int xSoundSetStateAll(int state, int skip_mask);

/* set loop - X_SOUND_NO_LOOP or X_SOUND_LOOP */
void xSoundSetLoop(int ref, int loop);

/* set pan mode - X_SOUND_SEPARATE or X_SOUND_COMBINED */
void xSoundSetPanMode(int ref, int panmode);

/* volume should be a float from 0.0f (silent) to 1.0f (full volume) */
void xSoundSetVolume(int ref, float volume);

/* pan should be a float from -1.0f (fully left) to 1.0f (fully right) */
void xSoundSetPan(int ref, float pan);

/* pitch should be a float >= 0.0f, 1.0f = normal */
void xSoundSetPitch(int ref, float pitch);

/* set speed of sound, used for doppler effects */
void xSound3dSpeedOfSound(float value);

/* set listener values */
void xSound3dSetListener(ScePspFMatrix4* orientation, ScePspFVector3* vel);

/* play sound with 3d effects. returns ref on success, -1 on failure */
int xSound3dPlay(xSoundBuffer* buf, xSound3dSource* s, int update);

/* update sounds with 3d effects */
void xSound3dUpdate();

#ifdef __cplusplus
}
#endif

#endif