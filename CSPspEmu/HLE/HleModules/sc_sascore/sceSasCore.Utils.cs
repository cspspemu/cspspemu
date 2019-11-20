using System;
using System.Collections.Generic;
using CSPspEmu.Hle.Formats.audio;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
    public unsafe partial class sceSasCore
    {
        Dictionary<uint, SasCore> SasCoreList = new Dictionary<uint, SasCore>();

        /// <summary>
        /// 
        /// </summary>
        static sceSasCore()
        {
            //if (sizeof(SasCore) > 512 * 4) throw (new InvalidProgramException("SasCore too big"));
        }

        public SasCore GetSasCore(uint SasCorePointer, bool CreateIfNotExists = false)
        {
            if (SasCorePointer == 0 || SasCorePointer % 64 != 0)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_ADDRESS);
            }

            if (CreateIfNotExists)
            {
                if (!SasCoreList.ContainsKey(SasCorePointer))
                {
                    SasCoreList[SasCorePointer] = new SasCore();
                }
            }

            if (!SasCoreList.ContainsKey(SasCorePointer))
            {
                throw new SceKernelException(SceKernelErrors.ERROR_SAS_NOT_INIT);
            }

            return SasCoreList[SasCorePointer];
        }

        /// <summary>
        /// Checks if a SasCore pointer structure is fine.
        /// </summary>
        /// <param name="SasCorePointer"></param>
        /// <param name="Voice"></param>
        private SasVoice GetSasCoreVoice(uint SasCorePointer, int Voice)
        {
            var SasCore = GetSasCore(SasCorePointer);
            _CheckVoice(Voice);
            return SasCore.Voices[Voice];
        }

        /// <summary>
        /// Checks a voice.
        /// </summary>
        /// <param name="Voice"></param>
        private static void _CheckVoice(int Voice)
        {
            if (Voice < 0 || Voice >= 32)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_VOICE);
            }
        }

        public const int PSP_SAS_VOICES_MAX = 32;
        public const int PSP_SAS_GRAIN_SAMPLES = 256;
        public const int PSP_SAS_VOL_MAX = 0x1000;
        public const int PSP_SAS_LOOP_MODE_OFF = 0;
        public const int PSP_SAS_LOOP_MODE_ON = 1;
        public const int PSP_SAS_PITCH_MIN = 0x1;
        public const int PSP_SAS_PITCH_BASE = 0x1000;
        public const int PSP_SAS_PITCH_MAX = 0x4000;
        public const int PSP_SAS_NOISE_FREQ_MAX = 0x3F;
        public const int PSP_SAS_ENVELOPE_HEIGHT_MAX = 0x40000000;
        public const int PSP_SAS_ENVELOPE_FREQ_MAX = 0x7FFFFFFF;
        public const int PSP_SAS_ADSR_ATTACK = 1;
        public const int PSP_SAS_ADSR_DECAY = 2;
        public const int PSP_SAS_ADSR_SUSTAIN = 4;
        public const int PSP_SAS_ADSR_RELEASE = 8;
    }

    public enum WaveformEffectType : int
    {
        PSP_SAS_EFFECT_TYPE_OFF = -1,
        PSP_SAS_EFFECT_TYPE_ROOM = 0,
        PSP_SAS_EFFECT_TYPE_UNK1 = 1,
        PSP_SAS_EFFECT_TYPE_UNK2 = 2,
        PSP_SAS_EFFECT_TYPE_UNK3 = 3,
        PSP_SAS_EFFECT_TYPE_HALL = 4,
        PSP_SAS_EFFECT_TYPE_SPACE = 5,
        PSP_SAS_EFFECT_TYPE_ECHO = 6,
        PSP_SAS_EFFECT_TYPE_DELAY = 7,
        PSP_SAS_EFFECT_TYPE_PIPE = 8,
    }

    [Flags]
    public enum AdsrFlags : uint
    {
        HasAttack = 1 << 0,
        HasDecay = 1 << 1,
        HasSustain = 1 << 2,
        HasRelease = 1 << 3,
    }

    public enum OutputMode : uint
    {
        PSP_SAS_OUTPUTMODE_STEREO = 0,
        PSP_SAS_OUTPUTMODE_MULTICHANNEL = 1,
    }

    public enum AdsrCurveMode : uint
    {
        PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE = 0,
        PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE = 1,
        PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT = 2,
        PSP_SAS_ADSR_CURVE_MODE_EXPONENT_REV = 3,
        PSP_SAS_ADSR_CURVE_MODE_EXPONENT = 4,
        PSP_SAS_ADSR_CURVE_MODE_DIRECT = 5,
    }

    public struct SasEnvelope
    {
        public int AttackRate;
        public int DecayRate;
        public int SustainRate;
        public int ReleaseRate;
        public AdsrCurveMode AttackCurveMode;
        public AdsrCurveMode DecayCurveMode;
        public AdsrCurveMode SustainCurveMode;
        public AdsrCurveMode ReleaseCurveMode;
        public int SustainLevel;
        public int Height;
    }

    public class SasVoice
    {
        /// <summary>
        /// 
        /// </summary>
        public int Index;

        /// <summary>
        /// Voice enabled.
        /// </summary>
        protected bool On;

        public void SetOn(bool Set)
        {
            On = Set;
            SetPlaying(Set);
        }

        public void SetPlaying(bool Set)
        {
            Playing = Set;
            if (Vag != null) Vag.Reset();
        }

        /// <summary>
        /// Voice is playing.
        /// </summary>
        public bool Playing;

        /// <summary>
        /// Voice is paused.
        /// </summary>
        public bool Paused;

        /// <summary>
        /// Voice has ended.
        /// </summary>
        public bool Ended => !Playing;

        /// <summary>
        /// 
        /// </summary>
        public uint VagAddress;

        /// <summary>
        /// 
        /// </summary>
        public int VagSize;

        /// <summary>
        /// 
        /// </summary>
        public ISoundDecoder Vag = null;

        /// <summary>
        /// 
        /// </summary>
        public int Pitch = sceSasCore.PSP_SAS_PITCH_BASE;

        public int LeftVolume = sceSasCore.PSP_SAS_VOL_MAX;
        public int RightVolume = sceSasCore.PSP_SAS_VOL_MAX;
        public int EffectLeftVolume;
        public int EffectRightVolume;

        public int SustainLevel;
        public int EnvelopeHeight;

        public SasEnvelope Envelope;

        public SasVoice(int Index)
        {
            this.Index = Index;
        }

        public bool OnAndPlaying => On && Playing;
    }

    public class SasCore
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Initialized;

        /// <summary>
        /// 
        /// </summary>
        public int GrainSamples;

        /// <summary>
        /// 
        /// </summary>
        public int MaxVoices;

        /// <summary>
        /// 
        /// </summary>
        public OutputMode OutputMode;

        /// <summary>
        /// 
        /// </summary>
        public int SampleRate;

        /// <summary>
        /// 
        /// </summary>
        public WaveformEffectType WaveformEffectType;

        /// <summary>
        /// 
        /// </summary>
        public bool WaveformEffectIsDry;

        /// <summary>
        /// 
        /// </summary>
        public bool WaveformEffectIsWet;

        /// <summary>
        /// Voices
        /// </summary>
        public SasVoice[] Voices;

        /// <summary>
        /// 
        /// </summary>
        public int LeftVolume;

        /// <summary>
        /// 
        /// </summary>
        public int RightVolume;

        public int Delay;
        public int Feedback;

        public SasCore()
        {
            Voices = new SasVoice[32];
            for (int n = 0; n < Voices.Length; n++) Voices[n] = new SasVoice(n);
        }

        /// <summary>
        /// 
        /// </summary>
        public uint EndFlags
        {
            get
            {
                uint Value = 0;
                for (int n = 0; n < 32; n++)
                {
                    if (Voices[n].Ended)
                    {
                        Value |= (uint) (1 << n);
                    }
                }
                return Value;
            }
        }
    }

    /*
    public unsafe struct SasCore
    {
        /// <summary>
        /// 
        /// </summary>
        public uint Index;
    }
    */
}