#ifndef __SASCORE_H
#define __SASCORE_H

struct SasHeader
{
	int unk1End; // always 00180990?  0x00180000 + 0x10 + 32 * 76.
	char revType; // default -1
	char unk; // default 0x0b -> changes to 0 after first sceSasCore...
	char revDelay; // default 0
	char revFeedback; // default 0
	char grainFactor; // 0x40=0x2 - 0x800=0x40
	char outMode; // stereo/mono
	char dryWet; // 1 = dry, 2 = wet, 3 = both, 0 = neither, default = 1
	char unkOrPad; // default = 0
	short revVolLeft; // default = 0
	short revVolRight; // default = 0
	int unk2; // default = -1
};

typedef enum
{
	SAS_VOICE_TYPE_NONE = 0x00,
	SAS_VOICE_TYPE_VAG = 0x01,
	SAS_VOICE_TYPE_NOISE = 0x02,
	SAS_VOICE_TYPE_TRIANGLE = 0x03,
	SAS_VOICE_TYPE_STEEP = 0x04,
	SAS_VOICE_TYPE_PCM = 0x05,
	// TODO: Not sure.
	SAS_VOICE_TYPE_MASK = 0x0F,

	SAS_VOICE_FLAG_PAUSED = 0x10,
	SAS_VOICE_FLAG_LOOP = 0x100,
} SasVoiceFlags;

struct SasVoice
{
	union // default 0x00000000 0x00000000
	{
		u32 unkNone[2];
		struct
		{
			void *vag;
			u32 vagLength;
		};
		int unkNoise[2];
		struct
		{
			void *pcm;
			short pcmLength; // actual length - 1
			short pcmLoops;
		};
		struct
		{
			short triangleOrSteep; // 0=1, 1=441, 2=441*2... max 100=441*100
			short unkTriangle[3];
		};
		// TODO: atrac3+...
	};
	u16 flags; // 0x10 == pause, 0x100 = loop, 0x1 = vag, 0x2 = noise, 0x3 = trangle, 0x4 = steep, 0x5 = pcm, default 0
	short pitch; // default = 0x1000
	short leftVolume; // default = 0x1000
	short rightVolume; // default = 0x1000
	short effectLeftVolume; // default = 0x1000
	short effectRightVolume; // default = 0x1000
	int unk1; // 0x00180000 + 0x10 + voiceNum * 76.  Repeats for second set of voices.
	int attackRate; // default 0
	int decayRate; // default 0
	int sustainRate; // default 0
	int releaseRate; // default 0
	int sustainLevel; // default 0x100
	char attackType; // default 0
	char decayType; // default 1
	char sustainType; // default 0
	char releaseType; // default 1
	u16 unk2; // ?? default=0707, likely ADSR related?
	u16 unk3; // more flags? fe00 when voice on?  but 0400 when voice off?
	int unk4; // could be padding, always 0 so far...
};

struct SasFooter
{
	int unk1; // default = -1
	int unk2; // default 0, changes to 1 after first sceSasCore().
	int unk3; // default 0
};

typedef struct {
	struct SasHeader header;
	struct SasVoice voices[64];
	struct SasFooter footer;
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
int __sceSasSetADSR(SasCore *sasCore, int voice, int flag, int attack, int decay, int sustain, int release);
int __sceSasRevParam(SasCore *sasCore, int delay, int feedback);
int __sceSasGetPauseFlag(SasCore *sasCore);
int __sceSasRevType(SasCore *sasCore, int type);
int __sceSasSetVolume(SasCore *sasCore, int voice, int leftVolume, int rightVolume, int effectLeftVolume, int effectRightVolume);
int __sceSasCoreWithMix(SasCore *sasCore, void *sasInOut, int leftVol, int rightVol);
int __sceSasSetSL(SasCore *sasCore, int voice, int level);
int __sceSasGetEndFlag(SasCore *sasCore);
int __sceSasGetEnvelopeHeight(SasCore *sasCore, int voice);
int __sceSasSetKeyOn(SasCore *sasCore, int voice);
int __sceSasSetPause(SasCore *sasCore, int voice_bit, int setPause);
int __sceSasSetVoice(SasCore *sasCore, int voice, void *vagPointer, int vagSize, int loopCount);
int __sceSasSetADSRmode(SasCore *sasCore, int voice, int flag, int attackType, int decayType, int sustainType, int releaseType);
int __sceSasSetKeyOff(SasCore *sasCore, int voice);
int __sceSasSetTrianglarWave(SasCore *sasCore, int voice, int unk);
int __sceSasCore(SasCore *sasCore, void *sasOut);
int __sceSasSetPitch(SasCore *sasCore, int voice, int pitch);
int __sceSasSetNoise(SasCore *sasCore, int voice, int freq);
int __sceSasGetGrain(SasCore *sasCore);
int __sceSasSetSimpleADSR(SasCore *sasCore, int voice, int ADSREnv1, int ADSREnv2);
int __sceSasSetGrain(SasCore *sasCore, int grain);
int __sceSasRevEVOL(SasCore *sasCore, int leftVol, int rightVol);
int __sceSasSetSteepWave(SasCore *sasCore, int voice, int unk);
int __sceSasGetOutputmode(SasCore *sasCore);
int __sceSasSetOutputmode(SasCore *sasCore, int outputMode);
int __sceSasRevVON(SasCore *sasCore, int dry, int wet);
int __sceSasGetAllEnvelopeHeights(SasCore *sasCore, int *heights);
int __sceSasSetVoicePCM(SasCore *sasCore, int voice, void *pcm, int size, int loop);

// TODO: Context struct
int __sceSasSetVoiceATRAC3(SasCore *sasCore, int voice, void *atrac3Context);
int __sceSasConcatenateATRAC3(SasCore *sasCore, int voice, void *data, int size);
int __sceSasUnsetATRAC3(SasCore *sasCore, int voice);

#endif