#ifndef __SASCORE_H
#define __SASCORE_H

typedef struct {
	unsigned int data[512];
} SasCore;

#define PSP_SAS_ERROR_ADDRESS        0x80420005
#define PSP_SAS_ERROR_VOICE_INDEX    0x80420010
#define PSP_SAS_ERROR_NOISE_CLOCK    0x80420011
#define PSP_SAS_ERROR_PITCH_VAL      0x80420012
#define PSP_SAS_ERROR_ADSR_MODE      0x80420013
#define PSP_SAS_ERROR_ADPCM_SIZE     0x80420014
#define PSP_SAS_ERROR_LOOP_MODE      0x80420015
#define PSP_SAS_ERROR_INVALID_STATE  0x80420016
#define PSP_SAS_ERROR_VOLUME_VAL     0x80420018
#define PSP_SAS_ERROR_ADSR_VAL       0x80420019
#define PSP_SAS_ERROR_FX_TYPE        0x80420020
#define PSP_SAS_ERROR_FX_FEEDBACK    0x80420021
#define PSP_SAS_ERROR_FX_DELAY       0x80420022
#define PSP_SAS_ERROR_FX_VOLUME_VAL  0x80420023
#define PSP_SAS_ERROR_BUSY           0x80420030
#define PSP_SAS_ERROR_NOTINIT        0x80420100
#define PSP_SAS_ERROR_ALRDYINIT      0x80420101


#define PSP_SAS_EFFECT_TYPE_OFF   -1
#define PSP_SAS_EFFECT_TYPE_ROOM   0
#define PSP_SAS_EFFECT_TYPE_UNK1   1
#define PSP_SAS_EFFECT_TYPE_UNK2   2
#define PSP_SAS_EFFECT_TYPE_UNK3   3
#define PSP_SAS_EFFECT_TYPE_HALL   4
#define PSP_SAS_EFFECT_TYPE_SPACE  5
#define PSP_SAS_EFFECT_TYPE_ECHO   6
#define PSP_SAS_EFFECT_TYPE_DELAY  7
#define PSP_SAS_EFFECT_TYPE_PIPE   8

#define PSP_SAS_VOICES_MAX          32
#define PSP_SAS_GRAIN_SAMPLES       256
#define PSP_SAS_VOL_MAX             0x1000
#define PSP_SAS_LOOP_MODE_OFF       0
#define PSP_SAS_LOOP_MODE_ON        1
#define PSP_SAS_PITCH_MIN           0x1
#define PSP_SAS_PITCH_BASE          0x1000
#define PSP_SAS_PITCH_MAX           0x4000
#define PSP_SAS_NOISE_FREQ_MAX      0x3F;
#define PSP_SAS_ENVELOPE_HEIGHT_MAX 0x40000000
#define PSP_SAS_ENVELOPE_FREQ_MAX   0x7FFFFFFF;

#define PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE 0
#define PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE 1
#define PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT     2
#define PSP_SAS_ADSR_CURVE_MODE_EXPONENT_REV    3
#define PSP_SAS_ADSR_CURVE_MODE_EXPONENT        4
#define PSP_SAS_ADSR_CURVE_MODE_DIRECT          5

#define PSP_SAS_ADSR_ATTACK  1
#define PSP_SAS_ADSR_DECAY   2
#define PSP_SAS_ADSR_SUSTAIN 4
#define PSP_SAS_ADSR_RELEASE 8

#define PSP_SAS_OUTPUTMODE_STEREO       0
#define PSP_SAS_OUTPUTMODE_MULTICHANNEL 1

int __sceSasInit(SasCore* sasCore, int grainSamples, int maxVoices, int outMode, int sampleRate);
int __sceSasSetOutputmode(SasCore* sasCore, int outputMode);
int __sceSasSetVoice(SasCore* sasCore, int voice, char* vagPointer, int vagSize, int loopCount);
int __sceSasSetPitch(SasCore* sasCore, int voice, int pitch);
int __sceSasSetVolume(SasCore* sasCore, int voice, int leftVolume, int rightVolume);
int __sceSasSetKeyOn(SasCore* sasCore, int voice);
unsigned int __sceSasCore(SasCore* sasCore, void* SasOut);

#endif