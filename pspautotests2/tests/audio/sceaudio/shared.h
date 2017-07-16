#include <common.h>
#include <pspaudio.h>
#include <psputility.h>
#include <pspaudiolib.h>

typedef enum PspVaudioSFXType
{
	PSP_VAUDIO_FX_TYPE_THRU = 0,
	PSP_VAUDIO_FX_TYPE_HEAVY = 1,
	PSP_VAUDIO_FX_TYPE_POPS = 2,
	PSP_VAUDIO_FX_TYPE_JAZZ = 3,
	PSP_VAUDIO_FX_TYPE_UNIQUE = 4,
} PspVaudioSFXType;

typedef enum PspVaudioAlcMode
{
	PSP_VAUDIO_ALC_OFF = 0,
	PSP_VAUDIO_ALC_MODE1 = 1
} PspVaudioAlcMode;

// Not at all sure about params.
int sceVaudioOutputBlocking(int vol, void *buf);
int sceVaudioChReserve(int sampleCount, int freq, int channels);
int sceVaudioChRelease();
int sceVaudioSetEffectType(PspVaudioSFXType type, int vol);
int sceVaudioSetAlcMode(PspVaudioAlcMode mode);