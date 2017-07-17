using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security;
using ALboolean = System.Boolean;
using ALchar = System.Byte;
using ALbyte = System.SByte;
using ALubyte = System.Byte;
using ALshort = System.Int16;
using ALushort = System.UInt16;
using ALint = System.Int32;
using ALuint = System.UInt32;
using ALsizei = System.Int32;
using ALenum = System.Int32;
using ALfloat = System.Single;
using ALdouble = System.Double;
using CSharpPlatform.Library;

namespace CSharpPlatform.AL
{
    public unsafe partial class AL
    {
        internal const string DllWindows = "OpenAL32";
        internal const string DllLinux = "libopenal.so.1";
        internal const string DllMac = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
        internal const string DllAndroid = "libopenal.so.1";

        static AL()
        {
            DynamicLibraryFactory.MapLibraryToType<AL>(
                DynamicLibraryFactory.CreateForLibrary(DllWindows, DllLinux, DllMac, DllAndroid));
        }

        public const int AL_INVALID = (-1);
        public const int AL_ILLEGAL_ENUM = AL_INVALID_ENUM;
        public const int AL_ILLEGAL_COMMAND = AL_INVALID_OPERATION;
        public const int AL_VERSION_1_0 = 1;
        public const int AL_VERSION_1_1 = 1;
        public const int AL_NONE = 0;
        public const int AL_FALSE = 0;
        public const int AL_TRUE = 1;
        public const ALenum AL_SOURCE_RELATIVE = 0x202;
        public const ALenum AL_CONE_INNER_ANGLE = 0x1001;
        public const ALenum AL_CONE_OUTER_ANGLE = 0x1002;
        public const ALenum AL_PITCH = 0x1003;
        public const ALenum AL_POSITION = 0x1004;
        public const ALenum AL_DIRECTION = 0x1005;
        public const ALenum AL_VELOCITY = 0x1006;
        public const ALenum AL_LOOPING = 0x1007;
        public const ALenum AL_BUFFER = 0x1009;
        public const ALenum AL_GAIN = 0x100A;
        public const ALenum AL_MIN_GAIN = 0x100D;
        public const ALenum AL_MAX_GAIN = 0x100E;
        public const ALenum AL_ORIENTATION = 0x100F;
        public const ALenum AL_SOURCE_STATE = 0x1010;
        public const ALenum AL_INITIAL = 0x1011;
        public const ALenum AL_PLAYING = 0x1012;
        public const ALenum AL_PAUSED = 0x1013;
        public const ALenum AL_STOPPED = 0x1014;
        public const ALenum AL_BUFFERS_QUEUED = 0x1015;
        public const ALenum AL_BUFFERS_PROCESSED = 0x1016;
        public const ALenum AL_SEC_OFFSET = 0x1024;
        public const ALenum AL_SAMPLE_OFFSET = 0x1025;
        public const ALenum AL_BYTE_OFFSET = 0x1026;
        public const ALenum AL_SOURCE_TYPE = 0x1027;
        public const ALenum AL_STATIC = 0x1028;
        public const ALenum AL_STREAMING = 0x1029;
        public const ALenum AL_UNDETERMINED = 0x1030;
        public const ALenum AL_FORMAT_MONO8 = 0x1100;
        public const ALenum AL_FORMAT_MONO16 = 0x1101;
        public const ALenum AL_FORMAT_STEREO8 = 0x1102;
        public const ALenum AL_FORMAT_STEREO16 = 0x1103;
        public const ALenum AL_REFERENCE_DISTANCE = 0x1020;
        public const ALenum AL_ROLLOFF_FACTOR = 0x1021;
        public const ALenum AL_CONE_OUTER_GAIN = 0x1022;
        public const ALenum AL_MAX_DISTANCE = 0x1023;
        public const ALenum AL_FREQUENCY = 0x2001;
        public const ALenum AL_BITS = 0x2002;
        public const ALenum AL_CHANNELS = 0x2003;
        public const ALenum AL_SIZE = 0x2004;
        public const ALenum AL_UNUSED = 0x2010;
        public const ALenum AL_PENDING = 0x2011;
        public const ALenum AL_PROCESSED = 0x2012;
        public const ALenum AL_NO_ERROR = AL_FALSE;
        public const ALenum AL_INVALID_NAME = 0xA001;
        public const ALenum AL_INVALID_ENUM = 0xA002;
        public const ALenum AL_INVALID_VALUE = 0xA003;
        public const ALenum AL_INVALID_OPERATION = 0xA004;
        public const ALenum AL_OUT_OF_MEMORY = 0xA005;
        public const ALenum AL_VENDOR = 0xB001;
        public const ALenum AL_VERSION = 0xB002;
        public const ALenum AL_RENDERER = 0xB003;
        public const ALenum AL_EXTENSIONS = 0xB004;
        public const ALenum AL_DOPPLER_FACTOR = 0xC000;
        public const ALenum AL_DOPPLER_VELOCITY = 0xC001;
        public const ALenum AL_SPEED_OF_SOUND = 0xC003;
        public const ALenum AL_DISTANCE_MODEL = 0xD000;
        public const ALenum AL_INVERSE_DISTANCE = 0xD001;
        public const ALenum AL_INVERSE_DISTANCE_CLAMPED = 0xD002;
        public const ALenum AL_LINEAR_DISTANCE = 0xD003;
        public const ALenum AL_LINEAR_DISTANCE_CLAMPED = 0xD004;
        public const ALenum AL_EXPONENT_DISTANCE = 0xD005;
        public const ALenum AL_EXPONENT_DISTANCE_CLAMPED = 0xD006;

        public static readonly alGetError alGetError;
        public static readonly alSourcef alSourcef;
        public static readonly alSource3f alSource3f;
        public static readonly alGenSources alGenSources;
        public static readonly alDeleteSources alDeleteSources;
        public static readonly alIsSource alIsSource;
        public static readonly alEnable alEnable;
        public static readonly alDisable alDisable;
        public static readonly alIsEnabled alIsEnabled;
        public static readonly alGetSourcei alGetSourcei;
        public static readonly alBufferData alBufferData;
        public static readonly alGetString alGetString;
        public static readonly alGetBooleanv alGetBooleanv;
        public static readonly alGetIntegerv alGetIntegerv;
        public static readonly alGetFloatv alGetFloatv;
        public static readonly alGetDoublev alGetDoublev;
        public static readonly alGetBoolean alGetBoolean;
        public static readonly alGetInteger alGetInteger;
        public static readonly alGetFloat alGetFloat;
        public static readonly alGetDouble alGetDouble;
        public static readonly alIsExtensionPresent alIsExtensionPresent;
        public static readonly alGetProcAddress alGetProcAddress;
        public static readonly alGetEnumValue alGetEnumValue;
        public static readonly alListenerf alListenerf;
        public static readonly alListener3f alListener3f;
        public static readonly alListenerfv alListenerfv;
        public static readonly alListeneri alListeneri;
        public static readonly alListener3i alListener3i;
        public static readonly alListeneriv alListeneriv;
        public static readonly alGetListenerf alGetListenerf;
        public static readonly alGetListener3f alGetListener3f;
        public static readonly alGetListenerfv alGetListenerfv;
        public static readonly alGetListeneri alGetListeneri;
        public static readonly alGetListener3i alGetListener3i;
        public static readonly alGetListeneriv alGetListeneriv;
        public static readonly alSourcefv alSourcefv;
        public static readonly alSourcei alSourcei;
        public static readonly alSource3i alSource3i;
        public static readonly alSourceiv alSourceiv;
        public static readonly alGetSourcef alGetSourcef;
        public static readonly alGetSource3f alGetSource3f;
        public static readonly alGetSourcefv alGetSourcefv;
        public static readonly alGetSource3i alGetSource3i;
        public static readonly alGetSourceiv alGetSourceiv;
        public static readonly alSourcePlayv alSourcePlayv;
        public static readonly alSourceStopv alSourceStopv;
        public static readonly alSourceRewindv alSourceRewindv;
        public static readonly alSourcePausev alSourcePausev;
        public static readonly alSourcePlay alSourcePlay;
        public static readonly alSourceStop alSourceStop;
        public static readonly alSourceRewind alSourceRewind;
        public static readonly alSourcePause alSourcePause;
        public static readonly alSourceQueueBuffers alSourceQueueBuffers;
        public static readonly alSourceUnqueueBuffers alSourceUnqueueBuffers;
        public static readonly alGenBuffers alGenBuffers;
        public static readonly alDeleteBuffers alDeleteBuffers;
        public static readonly alIsBuffer alIsBuffer;
        public static readonly alBufferf alBufferf;
        public static readonly alBuffer3f alBuffer3f;
        public static readonly alBufferfv alBufferfv;
        public static readonly alBufferi alBufferi;
        public static readonly alBuffer3i alBuffer3i;
        public static readonly alBufferiv alBufferiv;
        public static readonly alGetBufferf alGetBufferf;
        public static readonly alGetBuffer3f alGetBuffer3f;
        public static readonly alGetBufferfv alGetBufferfv;
        public static readonly alGetBufferi alGetBufferi;
        public static readonly alGetBuffer3i alGetBuffer3i;
        public static readonly alGetBufferiv alGetBufferiv;

        public static string alGetErrorString(ALenum error)
        {
            return "Unknown(" + error + ")";
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALenum alGetError();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcef(ALuint sid, ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSource3f(ALuint sid, ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGenSources(ALsizei n, ALuint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alDeleteSources(ALsizei n, ALuint* sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALboolean alIsSource(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alEnable(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alDisable(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALboolean alIsEnabled(ALenum capability);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcei(ALuint sid, ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferData(ALuint bid, ALenum format, void* data, ALsizei size, ALsizei freq);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALchar* alGetString(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBooleanv(ALenum param, ALboolean* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetIntegerv(ALenum param, ALint* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetFloatv(ALenum param, ALfloat* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetDoublev(ALenum param, ALdouble* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALboolean alGetBoolean(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALint alGetInteger(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALfloat alGetFloat(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALdouble alGetDouble(ALenum param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALboolean alIsExtensionPresent(ALchar* extname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void* alGetProcAddress(ALchar* fname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALenum alGetEnumValue(ALchar* ename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListenerf(ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListener3f(ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListenerfv(ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListeneri(ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListener3i(ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alListeneriv(ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListenerf(ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListener3f(ALenum param, ALfloat*value1, ALfloat*value2, ALfloat*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListenerfv(ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListeneri(ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListener3i(ALenum param, ALint*value1, ALint*value2, ALint*value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetListeneriv(ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcefv(ALuint sid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcei(ALuint sid, ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSource3i(ALuint sid, ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceiv(ALuint sid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcef(ALuint sid, ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSource3f(ALuint sid, ALenum param, ALfloat* value1, ALfloat* value2,
        ALfloat* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourcefv(ALuint sid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSource3i(ALuint sid, ALenum param, ALint* value1, ALint* value2, ALint* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetSourceiv(ALuint sid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePlayv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceStopv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceRewindv(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePausev(ALsizei ns, ALuint*sids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePlay(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceStop(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceRewind(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourcePause(ALuint sid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceQueueBuffers(ALuint sid, ALsizei numEntries, ALuint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alSourceUnqueueBuffers(ALuint sid, ALsizei numEntries, ALuint*bids);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGenBuffers(ALsizei n, ALuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alDeleteBuffers(ALsizei n, ALuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate ALboolean alIsBuffer(ALuint bid);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferf(ALuint bid, ALenum param, ALfloat value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBuffer3f(ALuint bid, ALenum param, ALfloat value1, ALfloat value2, ALfloat value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferfv(ALuint bid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferi(ALuint bid, ALenum param, ALint value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBuffer3i(ALuint bid, ALenum param, ALint value1, ALint value2, ALint value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alBufferiv(ALuint bid, ALenum param, ALint* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferf(ALuint bid, ALenum param, ALfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBuffer3f(ALuint bid, ALenum param, ALfloat* value1, ALfloat* value2,
        ALfloat* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferfv(ALuint bid, ALenum param, ALfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferi(ALuint bid, ALenum param, ALint* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBuffer3i(ALuint bid, ALenum param, ALint* value1, ALint* value2, ALint* value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void alGetBufferiv(ALuint bid, ALenum param, ALint* values);
}